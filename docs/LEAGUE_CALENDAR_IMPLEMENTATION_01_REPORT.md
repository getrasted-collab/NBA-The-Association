# League, Calendar, and Schedule Implementation 01 Report

## Outcome

Phase 2 Implementation Slice 01 is complete. A validated synthetic `LeagueWorld` can now create independent headless season sessions, answer deterministic schedule and participant queries, and advance an authoritative civil league date through opening, off days, scheduled game days, and the inclusive Season end.

No basketball simulation, scores, results, statistics, standings, playoffs, offseason, persistence, Unity, UI, real data, or schedule generation was added.

## Implemented

- `Game.ScheduledDate`, derived from the civil date at `ScheduledStart`'s stored offset.
- A new `NBATheAssociation.Application` project referencing Core only.
- Season-oriented `LeagueSession` with an independently mutable `CurrentDate`.
- `SeasonDatePosition`, `LeagueAdvanceResult`, and `LeagueAdvanceStopReason`.
- Immutable `ScheduleIndex` read models and deterministic schedule queries.
- Season participant queries derived from `TeamSeason` records.
- Explicit `AdvanceDay`, `AdvanceToDate`, and `AdvanceToNextGameDay` commands.
- V2 validation for overlapping same-league Seasons, invalid scheduled starts, duplicate fixtures, and same-team/same-instant conflicts.
- Two fictional calendar fixtures with variable league sizes, rules, offsets, off days, and schedule shapes.
- Fourteen focused Phase 2 NUnit tests while preserving all 19 Data Foundation tests.

## Resulting architecture

```text
NBATheAssociation.Data
  validates/materializes packages
             ↓
NBATheAssociation.Core
  immutable LeagueWorld, Season, TeamSeason, Game
             ↑
NBATheAssociation.Application
  LeagueSession, ScheduleIndex, advancement/query results
             ↑
future hosts (CLI/Unity/UI)
```

Application has one project reference—to Core—and no reference to Data, JSON, filesystem package loading, CLI, Unity, or an external provider. It contains no third-party runtime dependency.

## Application responsibilities

`LeagueSession` selects one existing Season, exposes its League and deterministically ordered participants, owns the mutable current `DateOnly`, and holds a derived schedule index. A default session starts at `Season.StartsOn`; an explicit pre-opening date is permitted. The selected Season is active from opening through its inclusive end. The session cannot advance into another Season.

`LeagueWorld`, its Games, package DTOs, and fixture JSON remain unchanged. Multiple sessions over the same world advance independently.

## Schedule-query behavior

- `AllGames` returns only target-Season Games.
- `GamesOn(date)` uses the local civil `Game.ScheduledDate`.
- `UpcomingGames(afterDate, limit)` and next-game queries use strict-after date semantics.
- `GamesForTeam` includes home and away appearances and rejects a nonparticipant.
- `NextGameAfter` follows deterministic Game ordering.
- `NextGameDayAfter` returns the earliest later civil game date.
- Game collections are ordered by UTC tipoff instant and then `GameId`.
- Source collection order does not affect results.

## Date-advancement behavior

- `AdvanceDay` moves one civil day, including across off days.
- `AdvanceToDate` rejects backward targets, reports equal-date no-ops, and clamps targets beyond Season end.
- `AdvanceToNextGameDay` skips off days to the earliest strictly later game date.
- Reaching opening and Season end returns explicit reasons.
- Requests at Season end and no-future-game requests preserve state and report why no movement occurred.
- No operation reads the wall clock, invokes randomness, simulates a Game, or publishes generic events.

## Validation added

- `season.overlap` for overlapping inclusive Season ranges within one League.
- `game.scheduled_start.invalid` for default/uninitialized tipoffs.
- `game.schedule_duplicate` for the same unordered matchup in one Season at the same instant, including reversed home/away.
- `game.team_time_conflict` when one TeamSeason appears in multiple Games at the same instant.

Adjacent and gapped Seasons remain valid. Consecutive-day Games, repeated matchups at different times, and same-day Games at different times remain valid. No rest-day, travel, schedule-balance, venue, or modern league-size rule was added.

## Fixtures

### Short four-team league

The fictional Northstar Basketball League has four TeamSeasons, a twelve-day Season, six Games, opening-day double scheduling for different teams, off days, consecutive-day participation, multiple UTC offsets, and one 10:30 PM `-08:00` tipoff whose UTC date is the following day while its schedule date remains local opening day.

### Historical-style league

The fictional Tri-City Winter Circuit has three TeamSeasons, a different February calendar, a two-half/no-three-point RuleSet, three Games, and a long off-day gap. It proves no assumption of 30 teams, 82 Games, conferences, modern periods, or modern dates.

## Tests and verification

- Baseline before implementation: **19 passed**.
- Final full solution build: successful with zero warnings/errors.
- Final complete NUnit suite: **33 passed, 0 failed, 0 skipped**.
- Phase 2 tests added: **14**.

Coverage includes fixture loading, variable participation, local-versus-UTC schedule dates, every approved query, deterministic ordering, all three advancement commands, pre-opening/active/end positions, off days, backward rejection, no-future-game behavior, session isolation, source immutability, Season overlap/gap/adjacency, duplicate/conflicting schedules, and deliberately valid historical patterns.

## Regressions found and fixed

No existing Data Foundation regression failed. During code review, `AdvanceToDate` at an already-ended Season was tightened so a farther target reports `AlreadyAtSeasonEnd` without claiming movement.

## Deviations

No architectural or scope deviation was required. `SeasonQueries.cs` was not created because participant selection is a small cohesive responsibility of `LeagueSession`; a separate service would add abstraction without reuse. No persisted schema field was added, so the package remains V2.

## Technical debt and risks

- Numeric tipoff offsets do not preserve a venue time-zone identity or historical daylight-saving rule. Authored offsets are authoritative for this slice.
- `Season.StartsOn`/`EndsOn` describe one inclusive overall Season range; explicit competition phases remain unresolved.
- The session deliberately stops at Season end. Offseason work and rollover require a separately approved design.
- Conflict validation covers exact same-instant structural impossibilities only. Venue, travel, rest, and schedule feasibility remain deferred.
- Application must remain a narrow workflow layer and not become a generic manager container.

## Recommended next slice

Do not start another slice automatically. The next planning decision should choose between:

1. a small continuation of League/Calendar focused on explicit calendar events or carefully scoped season transition prerequisites; or
2. Phase 3 planning for expanded player/team/roster/lineup models before basketball simulation.

Season phases, schedule generation, automatic rollover, results, standings, saves, and basketball simulation should each remain outside the next slice unless explicitly approved.
