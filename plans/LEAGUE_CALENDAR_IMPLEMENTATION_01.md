# League, Calendar, and Schedule — Implementation 01

## Purpose

Create the smallest headless Phase 2 foundation that can open a validated synthetic `LeagueWorld`, establish a season-oriented runtime session, query its participants and schedule, and advance an authoritative league date through the season without mutating source data.

This is a plan only. Implementation requires explicit approval.

## Guardrails

This slice must not add basketball simulation, results, scores, box scores, possessions, statistics, standings, contracts, transactions, salary cap, draft, free agency, injuries, development, AI, saves, real NBA data, Unity, UI, dependency injection, a generic event bus, ECS, or a database.

Continue using `net10.0`, NUnit, standard-library types, fixed synthetic IDs, and deterministic ordering. Do not resume import-pipeline expansion during this phase.

## Assessment of the existing foundation

Data Foundation Slices 01 and 02 are a sound base:

- `LeagueWorld` is independently materialized and exposes read-only entity dictionaries.
- `Season`, `TeamSeason`, and `Game` already carry the minimum identities and references needed by this slice.
- `Game.ScheduledStart` uses `DateTimeOffset`, preserving the scheduled local clock time and its UTC offset.
- V2 validation already rejects duplicate game IDs, missing participants, wrong-season participants, same-team games, unsupported status, and games outside season bounds.
- The V2 cross-era fixture proves multiple seasons and changing rules, while the sparse fixture proves a smaller historical-style structure.

No rewrite is justified. Phase 2 should build on the materialized Core model rather than introduce a second league model.

### Bounded corrections before building on it

1. Give `Game` one computed Core concept, `ScheduledDate`, defined from the civil date encoded in `ScheduledStart` at its stored offset. Data validation and schedule queries must use the same rule.
2. Reject a default/uninitialized `ScheduledStart` with a structured validation issue.
3. Add same-league Season overlap validation because an unambiguous active Season cannot otherwise be derived for a league date. Adjacent seasons are valid; overlapping inclusive date ranges within one League are not.
4. Add exact duplicate-fixture and team-at-the-same-instant conflict validation described below. Do not add rest-day or one-game-per-date rules.
These are extensions of existing validation and date semantics, not changes to durable identity or package architecture. V2 remains the package schema unless a new persisted field becomes necessary; this slice does not require one.

## 1. Authoritative time model

### Decision

Use `DateOnly` for the authoritative simulated league date.

Keep `DateTimeOffset` for each game’s scheduled tipoff. The game’s **schedule date** is the local civil date represented by `ScheduledStart` with its stored offset:

```text
Game.ScheduledDate = DateOnly.FromDateTime(Game.ScheduledStart.DateTime)
```

Do not convert the tipoff to UTC before assigning its schedule date. A 10:30 PM game stored at `-08:00` belongs to that local calendar date even if its UTC instant falls on the following day.

### Distinctions

- **Wall-clock time:** never authoritative and never read implicitly by Core/Application. Tests and callers supply all dates.
- **League current date:** a `DateOnly` owned by runtime state.
- **Season bounds:** inclusive `DateOnly` values already stored by `Season.StartsOn` and `Season.EndsOn`.
- **Tipoff:** `DateTimeOffset`, retaining a specific instant, local clock time, and numeric offset.

### Time-zone limit for V1

V1 does not store IANA/Windows time-zone identifiers and does not recalculate daylight-saving rules. Imported/authored tipoffs must already contain the intended offset. International games work when their intended local offset is present. A game crossing midnight during play remains assigned to its tipoff date; game duration and completion time do not exist yet.

This avoids false time-zone precision while preserving a clean future seam for venue time zones if they become necessary.

## 2. Runtime/application boundary

Add one project with a clear responsibility:

```text
src/NBATheAssociation.Application/
```

Dependency direction:

```text
Data → validated immutable LeagueWorld (Core)
                                  ↑
                    Application session/queries
```

`NBATheAssociation.Application` references Core only. It must not reference Data, JSON, filesystems, CLI, Unity, or external providers. Data loading remains the caller’s responsibility.

### Why a new assembly is warranted

Core currently contains immutable domain facts. Data owns persistence boundaries. A mutable league session and its advancement operations are application workflows, not source data and not JSON concerns. Placing them in a small Application project preserves the existing dependency direction and gives future CLI/Unity hosts a headless API without turning `LeagueWorld` into mutable global state.

Do not create a `Simulation` project in this slice; no basketball simulation exists yet.

## 3. Minimal runtime state

Introduce a season-oriented `LeagueSession` (name may change only for a concrete naming conflict):

```text
LeagueSession
  World                 immutable LeagueWorld reference
  LeagueId              selected league
  TargetSeasonId        selected season context
  CurrentDate           mutable DateOnly, privately set
  Schedule              immutable ScheduleIndex for TargetSeasonId
```

Creation requires an existing Season and its League. The default factory starts on `Season.StartsOn`. An explicit initial date may be supplied for a future reconstructed session or a pre-opening scenario, but it must not be after `Season.EndsOn`. A pre-start date is permitted; no arbitrary lower calendar boundary is invented.

### Active season

For this first slice:

- before `StartsOn`: `ActiveSeason` is null and the session position is `BeforeSeason`;
- from `StartsOn` through `EndsOn`, inclusively: `ActiveSeason` is the selected Season;
- the session never advances after `EndsOn`; at the end it reports `SeasonEnd`.

The session is deliberately scoped to one target Season. Automatic transition into a later Season/offseason is deferred because the project has no offseason workflow or future-season generation yet. `LeagueWorld` may still contain multiple Seasons.

### Immutable starting data versus mutable runtime state

Immutable starting data owns League, Season, TeamSeason, Franchise, Game, RuleSet, roster, and all scheduled tipoff facts. The runtime session owns only the current date and later may own save-world mutations. Schedule indexes are derived read models rebuilt from `LeagueWorld`; they are not authoritative persisted state.

The session must never modify `LeagueWorld`, package DTOs, Games, or collections. This makes later save state straightforward: a save can persist the selected identities and current date without serializing application services or derived indexes. No save DTO is added now.

## 4. Season calendar model

Do **not** add `SeasonPhase` records in this slice.

The current `Season.StartsOn`/`EndsOn` range is sufficient to prove opening, active dates, gaps in games, final scheduled day, and season end. Adding preseason/regular/postseason/offseason phases now would require unresolved definitions, game classification, and cross-era phase semantics without serving the required operations.

Expose a small derived position enum such as:

```text
SeasonDatePosition
  BeforeSeason
  Active
  SeasonEnd
```

`SeasonEnd` means `CurrentDate == EndsOn`; it remains part of the inclusive active Season. Do not model `AfterSeason` as a reachable runtime state in this slice.

Phase records should be reconsidered when postseason/standings or offseason progression needs explicit boundaries.

## 5. Season participation

Provide an immutable query returning all `TeamSeason` records whose `SeasonId` equals the target Season, ordered deterministically by `TeamSeasonId` (presentation may later apply name/standings order).

Each participant’s `FranchiseId` must resolve in `LeagueWorld`; package validation already owns that invariant. No assumption is made about participant count, conferences, divisions, or modern league structure.

Do not cache a second authoritative participant list. A session or season query may hold a derived read-only array for efficient repeated access.

## 6. Schedule responsibilities

Add a small immutable `ScheduleIndex`, created from a validated `LeagueWorld` and one `SeasonId`. It owns derived lookup/order structures, not Game state.

Required operations:

```text
AllGames
GamesOn(DateOnly date)
UpcomingGames(DateOnly afterDate, int? limit = null)
GamesForTeam(TeamSeasonId teamSeasonId)
NextGameAfter(DateOnly date)
NextGameDayAfter(DateOnly date)
```

Semantics:

- `AllGames` contains only Games for the selected Season.
- `GamesOn` compares `Game.ScheduledDate`.
- “after” is strict. Games on the supplied date are not upcoming/next.
- Team schedule includes home and away games.
- Results are read-only and deterministic, ordered by `ScheduledStart.UtcDateTime`, then `GameId`.
- `NextGameAfter` returns the first Game under that ordering.
- `NextGameDayAfter` returns the earliest strictly later `ScheduledDate` containing at least one Game.
- Unknown or wrong-season `TeamSeasonId` is an explicit argument error/result, not an empty result that hides misuse. The implementation plan should choose one consistent public pattern; a small `Try...` plus throwing query overload is sufficient without a result framework.

No scores, completion state, simulation hooks, standings, calendar event union, or generic query language is added.

## 7. Date advancement

`LeagueSession` exposes exactly three commands:

```text
AdvanceDay()
AdvanceToDate(DateOnly target)
AdvanceToNextGameDay()
```

Do not add generic `AdvanceUntil` in this slice.

Each returns an immutable `LeagueAdvanceResult` containing:

```text
PreviousDate
CurrentDate
DidAdvance
StopReason
```

Recommended `LeagueAdvanceStopReason` values:

- `DayAdvanced`
- `RequestedDateReached`
- `SeasonStartReached`
- `NextGameDayReached`
- `SeasonEndReached`
- `AlreadyAtRequestedDate`
- `AlreadyAtSeasonEnd`
- `NoFutureGameDay`
- `BackwardTargetRejected`

Rules:

- Advancement is synchronous, deterministic, and uses no wall clock or randomness.
- A backward target returns `BackwardTargetRejected` and leaves state unchanged.
- A target equal to current date is a no-op.
- `AdvanceDay` advances by one civil day, except at season end.
- Reaching opening day from a pre-start date reports `SeasonStartReached`.
- `AdvanceToDate` moves directly to the target when it is within bounds. If the target exceeds `EndsOn`, it clamps to `EndsOn` and reports `SeasonEndReached`.
- `AdvanceToNextGameDay` chooses the earliest game date strictly after `CurrentDate`. If none exists, it leaves the date unchanged and reports `NoFutureGameDay`; it does not silently jump to season end.
- Callers that want to process games on the current day query `GamesOn(CurrentDate)` before advancing.

No event bus, callback pipeline, async loop, or automatic game simulation occurs.

## 8. Boundary behavior

| Situation | Required behavior |
|---|---|
| Initial default session | `CurrentDate == Season.StartsOn`; Season is active |
| Explicit date before opening | Allowed; `ActiveSeason == null`, `BeforeSeason` |
| Advance onto opening day | Date changes to `StartsOn`; reason `SeasonStartReached` |
| Ordinary active date | Active Season and participant/schedule queries available |
| Off day | Valid date; `GamesOn` returns empty |
| Final scheduled day before season end | Games query normally; later dates remain valid off days |
| Season end date | Inclusive active date and `SeasonEnd` position |
| Advance beyond season end | Clamp/no-op at `EndsOn` with explicit reason |
| No future games | `AdvanceToNextGameDay` does not move |

This slice recognizes but does not execute offseason work or transition automatically into the next Season.

## 9. Schedule and season validation

Retain all current V2 validation and add stable codes for:

- `season.overlap`: two Seasons for the same League have overlapping inclusive ranges. Adjacent non-overlapping ranges and gaps are valid.
- `game.scheduled_start.invalid`: default/uninitialized or otherwise non-materializable scheduled tipoff.
- `game.schedule_duplicate`: two distinct Game IDs have the same Season, same scheduled instant, and same unordered pair of TeamSeason participants. Reversed home/away at the identical instant is still a duplicate fixture.
- `game.team_time_conflict`: one TeamSeason appears in two distinct Games at the same instant.

Existing codes continue covering:

- duplicate Game IDs (`id.duplicate`);
- missing TeamSeason/Season references (`reference.missing`);
- wrong-season teams (`game.participant_wrong_season`);
- same home/away team (`game.same_participant`);
- game outside Season bounds (`game.outside_season`);
- unsupported status (`game.status.invalid`).

### Deliberately valid situations

- consecutive-day games;
- more than one game for a team on the same civil date when tipoffs differ (historical doubleheaders remain representable);
- repeated matchups on different instants;
- off days and long gaps;
- simultaneous games involving different teams;
- variable team and game counts.

Do not validate rest, travel, schedule balance, venue availability, playoff format, or number of games.

## 10. Synthetic fixtures

Add two fictional V2 fixtures dedicated to Phase 2. Do not alter the existing V1/V2 regression fixtures except for a proven correction.

### Fixture A — short four-team league

- one League and one short Season;
- four Franchises and four TeamSeasons;
- a compact range such as ten to fourteen days;
- opening-day games, multiple later game days, explicit off days, and a final scheduled day before or on Season end;
- games with different numeric UTC offsets, including one whose UTC date differs from its local schedule date;
- a non-modern-neutral fictional RuleSet (no schedule assumption depends on it).

### Fixture B — small historical-style league

- one League with two or three TeamSeasons;
- different month/date range and different RuleSet;
- fewer games and a long off-day gap;
- no conferences/divisions or modern calendar assumptions.

All identities and dates are fixed. No real teams, players, schedules, or imported data are used.

## 11. Required tests

### Regression and loading

- All existing 19 tests remain green.
- Both new V2 fixtures load and validate.
- Runtime construction starts from a validated materialized `LeagueWorld`.

### Authoritative date and season

- Default session date equals `Season.StartsOn`.
- Explicit pre-start date reports no active Season.
- Opening date, ordinary active date, and inclusive end date report the correct position.
- Same stored tipoff offset produces the expected local `ScheduledDate`, even when its UTC date differs.

### Participation and queries

- Four-team fixture returns exactly four participants; historical fixture returns its smaller count.
- Participants resolve to the correct Franchises.
- `GamesOn` returns correct games and an empty collection on off days.
- `NextGameAfter` and `NextGameDayAfter` are correct.
- `GamesForTeam` includes home and away appearances only for that TeamSeason.
- `UpcomingGames` obeys strict-after semantics and optional limit.
- Query ordering is stable regardless of source JSON collection order.

### Advancement

- `AdvanceDay` advances one date.
- Advancing through off days is allowed.
- `AdvanceToDate` reaches a valid future date.
- `AdvanceToNextGameDay` skips off days and stops on the expected game date.
- Backward advancement is rejected without mutation.
- Advancing from pre-start onto opening reports the opening boundary.
- Advancing past Season end clamps/stops correctly.
- No future game day returns a no-op result.

### Validation

- overlapping Seasons in one League fail; gaps and adjacency pass;
- invalid/missing/wrong-season participants retain stable failures;
- same-team Game and duplicate Game IDs fail;
- exact duplicate fixtures fail;
- one team double-booked at the exact same instant fails;
- consecutive-day games and same-day games at different instants pass;
- default/malformed scheduled time fails structurally.

### Isolation

- advancement never mutates `LeagueWorld`, source package DTOs, Games, or source JSON;
- two sessions over the same world advance independently;
- repeated queries and advancement sequences are deterministic.

Target approximately 12–18 focused Phase 2 tests rather than one test per method permutation.

## 12. Expected project and file changes

### New project

```text
src/NBATheAssociation.Application/
  NBATheAssociation.Application.csproj
  LeagueSession.cs
  LeagueAdvanceResult.cs
  ScheduleIndex.cs
  SeasonQueries.cs              # only if separation improves clarity
```

The solution and test project reference Application. No new NuGet runtime dependency is expected.

### Existing Core

- Add `Game.ScheduledDate` as a computed domain property, or an equally small named Core helper if record syntax makes the property clearer outside the primary constructor.
- Do not change Game identity, Season identity, `ScheduledStart`, or `LeagueWorld` mutability.

### Existing Data

- Extend V2 validation for Season overlaps, scheduled-start validity, exact duplicate fixtures, and same-team/same-instant conflicts.
- Make existing game-bound checks use the shared scheduled-date rule.
- Keep V1 frozen except for a safety check needed by migration/regression; Phase 2 runtime should use validated V2 packages.

### Tests and fixtures

```text
tests/NBATheAssociation.Tests/LeagueCalendarTests.cs
tests/NBATheAssociation.Tests/Fixtures/league-calendar-short.json
tests/NBATheAssociation.Tests/Fixtures/league-calendar-historical.json
```

Update documentation, roadmap, open questions, backlog, and the implementation report after implementation. Do not add save or UI DTOs.

## 13. Implementation sequence

1. Preserve the 19-test baseline.
2. Add the named scheduled-date semantic to Core and focused tests.
3. Harden V2 schedule/Season validation and test valid/invalid edge cases.
4. Add the two synthetic Phase 2 fixtures.
5. Create the Application project referencing Core only.
6. Implement season participation and the immutable `ScheduleIndex`.
7. Implement `LeagueSession` and explicit advancement results.
8. Test boundaries, deterministic ordering, independent sessions, and source immutability.
9. Run the full solution build/test suite and review for unnecessary abstractions.
10. Record accepted decisions, update roadmap/questions/backlog, and write the Phase 2 Slice 01 report.

## 14. Architectural decisions to record upon approval

Implementation should add one ADR covering:

1. the authoritative league date is `DateOnly`;
2. tipoffs remain `DateTimeOffset`, and schedule date uses the civil date at the stored offset;
3. immutable `LeagueWorld` is starting truth while `LeagueSession` owns mutable runtime date;
4. Phase 2 application workflows live in a Core-only Application assembly;
5. the first runtime session is season-oriented and stops at Season end;
6. explicit Season phases and automatic inter-season/offseason progression are deferred.

The ADR should be recorded only after this plan is approved, not during planning.

## 15. Risks

- A numeric offset is sufficient for authored tipoffs but cannot answer future venue-zone/DST reinterpretation questions. Do not add a time-zone database until a real requirement exists.
- A Season-level `StartsOn` may eventually mean a broader league-year boundary rather than first competition day. Explicit phases can resolve that later without changing the authoritative date type.
- A season-oriented session deliberately cannot progress across multiple seasons. Automatic rollover must be designed with offseason and future-season creation rather than guessed now.
- Schedule conflicts beyond exact same-instant double-booking are context-dependent. Over-validation would erase historically possible doubleheaders.
- `NBATheAssociation.Application` must remain workflow-focused and must not grow into a generic manager dumping ground.

## 16. Intentionally deferred

- SeasonPhase records and game phase/type classification
- postseason brackets and playoff formats
- automatic season rollover and offseason workflows
- wall-clock-driven advancement, pauses, speeds, and background execution
- venue time-zone identifiers and DST rule databases
- rescheduling/postponement/cancellation/completion state
- scores, simulation, statistics, and standings
- generic calendar events or event bus
- schedule generation and optimization
- rest/travel/arena validation
- persistence/save DTOs
- UI/Unity integration
- real NBA schedules or imports

## 17. Approval decisions and blockers

No external blocker prevents this synthetic, headless slice. Approval of this plan should explicitly accept these bounded choices:

1. `DateOnly` is the authoritative league date; `DateTimeOffset` remains the tipoff representation.
2. Game schedule date is the civil date encoded by its stored offset, not its UTC date.
3. Runtime is season-oriented and cannot advance beyond the target Season in this slice.
4. `SeasonPhase` is deferred.
5. A small `NBATheAssociation.Application` project is warranted for mutable workflows and schedule queries.
6. Exact same-instant team double-booking is invalid, but same-day games at different times remain allowed.

If any of these choices are rejected, resolve that item before implementation rather than silently redesigning it.

## Definition of done

- A validated synthetic V2 `LeagueWorld` can create an independent headless session.
- The session has a deterministic authoritative date and unambiguous selected/active Season behavior.
- Participants and schedules are queryable without modern-NBA assumptions.
- Date advancement works across game days/off days and stops explicitly at boundaries.
- Schedule validation covers only clear structural impossibilities.
- Two variable-size fictional fixtures prove historical flexibility.
- Existing data-foundation behavior remains green and source data remains unchanged.
- No excluded gameplay, save, real-data, Unity, or presentation system is introduced.

## Recommendation

Phase 2 Implementation 01 can safely begin after the six approval decisions above are accepted. No additional user choice is required for implementation mechanics.
