# Data Foundation — Implementation Slice 01

## Purpose

Prove the smallest end-to-end data foundation that protects the long-term architecture: typed identity, franchise continuity, season-specific participation, player continuity, immutable JSON input, explicit schema recognition, structured validation, and automated tests.

This is an implementation plan only. Do not implement it until explicitly authorized and the blockers below are resolved.

## Preconditions

1. Use the recorded `net10.0` target (ADR-014).
2. Ensure the permanent project foundation files remain present in the authoritative repository.
3. If IDs will be deterministically generated from fixture source keys, first record the UUID algorithm, namespace UUIDs, normalization, and collision rules. Otherwise use fixed canonical GUID constants in fixtures.

## Decisions applied

- ADR-001: core logic is independent from Unity.
- ADR-003: source packages and runtime state are separate.
- ADR-004: historical variation is tested early.
- ADR-006: C#/.NET, NUnit, smallest useful project set, minimal dependencies.
- ADR-007: strongly typed GUID IDs and separate external identity.
- ADR-008: JSON V1 authoring/interchange, separate DTO/domain models.
- ADR-010: explicit schema recognition and graceful unsupported-version failure.
- ADR-011: Franchise persists while TeamSeason changes by season.
- ADR-012: unknown values remain distinct from known values.
- ADR-013: synthetic fictional data only.

## Minimal solution shape

Create only these projects:

```text
src/
  NBATheAssociation.Core/
  NBATheAssociation.Data/
tests/
  NBATheAssociation.Tests/
```

- `Core` owns typed IDs, immutable domain records, and domain invariants that do not depend on JSON.
- `Data` owns V1 JSON DTOs, schema envelope recognition, DTO-to-domain materialization, and package-level validation.
- `Tests` contains NUnit unit and integration tests for both. Split test projects only when size or dependency isolation warrants it.

Do not create `Simulation`, CLI, Unity, database, save, or editor projects in this slice.

Dependencies:

- NUnit and its test adapter/test SDK are the only expected non-framework test dependencies.
- Use `System.Text.Json` unless an actual compatibility requirement justifies another serializer.
- Record exact package versions and reasons when implementation is authorized.

## Domain scope

### Typed IDs

Implement small value types backed by `Guid`:

- `LeagueId`
- `SeasonId`
- `FranchiseId`
- `TeamSeasonId`
- `PersonId`
- `PlayerId`
- `PlayerSeasonProfileId`
- `RosterMembershipId`
- `RuleSetId`
- `GameId`
- `DataPackageId`

Each type must:

- reject `Guid.Empty` at its construction boundary;
- support equality and safe dictionary/set use;
- serialize through Data-layer DTO mapping as a canonical lowercase hyphenated GUID string;
- avoid implicit conversion between entity ID types;
- have concise parse/format tests.

Do not create a reflection-heavy universal ID framework. A small repeated pattern or narrowly scoped shared helper is acceptable if it remains readable.

### League

Minimum fields:

- `LeagueId Id`
- display name

No permanent team count, conferences, divisions, or modern-NBA assumptions.

### Season

Minimum fields:

- `SeasonId Id`
- `LeagueId LeagueId`
- label
- start and end dates
- `RuleSetId RuleSetId`

Team participation is established by `TeamSeason` records, not a hard-coded count.

### Franchise

Minimum fields:

- `FranchiseId Id`
- optional stable internal display label for debugging only

Do not put current city, nickname, abbreviation, arena, alignment, or roster on Franchise.

### TeamSeason

Minimum fields:

- `TeamSeasonId Id`
- `FranchiseId FranchiseId`
- `SeasonId SeasonId`
- market/city
- team name
- abbreviation
- optional branding reference string

Each season receives a different `TeamSeasonId`. Ordinary relocation/rebranding retains `FranchiseId`.

Do not implement predecessor/successor graphs, official history reassignment, mid-season identity periods, arenas, conferences, or divisions yet.

### Person and Player

`Person` minimum fields:

- `PersonId Id`
- given name
- family name

`Player` minimum fields:

- `PlayerId Id`
- `PersonId PersonId`

Names are presentation data, never identity. Do not model coaches or staff yet; the Person/role separation proves the extension point.

### PlayerSeasonProfile

Minimum fields:

- `PlayerSeasonProfileId Id`
- `PlayerId PlayerId`
- `SeasonId SeasonId`
- optional listed position
- one or two deliberately generic optional example values sufficient to prove known-versus-unknown handling

Recommended example values: optional height and one optional placeholder skill rating. Do not define the permanent ratings, attributes, tendency, potential, development, or position systems in this slice.

Unknown values must remain null/absent and must not receive modern defaults during materialization.

### RosterMembership

Minimum fields:

- `RosterMembershipId Id`
- `PlayerId PlayerId`
- `TeamSeasonId TeamSeasonId`
- inclusive start date
- optional inclusive end date

Rules:

- end date cannot precede start date;
- membership dates must fall within the referenced season;
- references must exist;
- a duplicate membership ID is invalid.

Do not implement jersey numbers, depth charts, active/inactive lists, transaction causes, two-way rules, roster limits, or overlapping-membership policy beyond what the fixture needs.

### RuleSet

Minimum fields:

- `RuleSetId Id`
- name/label
- `bool ThreePointEnabled`
- regulation period count
- regulation period duration

This only proves that seasons can reference different rule configurations. Do not model salary cap, roster, draft, playoff, foul, or complete on-court rules.

### Game

Minimum fields:

- `GameId Id`
- `SeasonId SeasonId`
- scheduled date/time representation chosen consistently for V1
- home `TeamSeasonId`
- away `TeamSeasonId`
- status limited to `Scheduled`

Validation requires distinct home/away participants, both participants belonging to the referenced season, and the date falling within the season.

Do not add scores, box scores, events, officials, venue, postponement history, results, or simulation state.

### DataPackage and runtime world

The JSON document uses a persistence envelope conceptually containing:

```json
{
  "schemaVersion": 1,
  "packageId": "00000000-0000-0000-0000-000000000001",
  "packageVersion": "1.0.0",
  "data": {
    "leagues": [],
    "ruleSets": [],
    "seasons": [],
    "franchises": [],
    "teamSeasons": [],
    "people": [],
    "players": [],
    "playerSeasonProfiles": [],
    "rosterMemberships": [],
    "games": []
  }
}
```

`DataPackage` is a Data-layer DTO/envelope, not a mutable domain aggregate. Materialization creates a separate immutable/read-only `LeagueWorld` containing validated domain records and indexes by typed ID. No domain type depends on `System.Text.Json` attributes or JSON element types.

`SaveWorld` is not implemented. The materialized `LeagueWorld` merely proves the boundary that later saves will capture.

## JSON behavior

- UTF-8 JSON.
- Explicit integer `schemaVersion` with V1 equal to `1`.
- Canonical GUID output: lowercase `D` format with hyphens.
- ISO 8601 date/date-time text with a single documented interpretation.
- Stable camelCase property names.
- Unknown historical facts use null/omission according to one recorded V1 convention; they never become zero/default silently.
- Duplicate JSON keys should fail or be rejected by validation if serializer behavior cannot guarantee it.
- Domain objects are created only after DTO parsing and validation.

Supported schema `1` loads. Missing, malformed, zero/negative, or unsupported newer schema versions return clear structured failures. Do not build migration steps until schema `2` exists.

## Structured validation

Return all safely discoverable issues in one result rather than throwing on the first domain error. Parsing failures and impossible envelope recognition may return a single fatal issue.

Minimum issue structure:

```text
ValidationIssue
  Code
  Severity: Error | Warning
  EntityType (optional)
  EntityId (optional string)
  Path
  Message
```

Minimum stable issue codes:

- `schema.missing`
- `schema.malformed`
- `schema.unsupported`
- `id.empty`
- `id.duplicate`
- `reference.missing`
- `season.date_range.invalid`
- `roster.date_range.invalid`
- `roster.outside_season`
- `game.same_participant`
- `game.participant_wrong_season`
- `game.outside_season`

Validation order:

1. envelope/schema recognition;
2. syntactic DTO checks;
3. duplicate IDs within each entity type;
4. references;
5. date/temporal rules;
6. materialization.

No generic rule engine is needed.

## Synthetic world fixture

Create one compact fictional JSON package:

- 1 league;
- 2 consecutive seasons;
- 2 or 3 franchises;
- one franchise represented by differently named/city `TeamSeason` records in the second season while keeping one `FranchiseId`;
- 2 rule sets, preferably one with and one without a three-point rule;
- approximately 4–6 fictional people/players;
- a profile for each relevant player-season, with at least one explicitly unknown optional value;
- roster memberships showing at least one `PlayerId` on different teams across seasons;
- 2–4 scheduled games across the seasons.

Use fixed, documented GUID constants unless the deterministic import algorithm has separately been approved. Keep the fixture small enough to understand during a code review.

Create invalid fixtures by minimal mutation/builders in tests rather than maintaining many nearly identical large JSON files.

## Required automated tests

### Typed IDs

- Each ID round-trips through its canonical string form.
- Empty GUID construction/parsing is rejected.
- Different ID types are not interchangeable at compile time/API boundaries.

### Identity continuity

- One `PlayerId` remains unchanged across two roster memberships for different team-seasons.
- One `FranchiseId` is referenced by differently branded/city TeamSeason records in two seasons.
- TeamSeason IDs are distinct across seasons.

### Validation

- Duplicate IDs are rejected with `id.duplicate` and a useful path.
- Missing League, Season, Franchise, TeamSeason, Person, Player, RuleSet, and Game references are rejected as applicable.
- Roster end-before-start and dates outside the season are rejected.
- A game's home/away team-season mismatch, same participant, or out-of-season date is rejected.
- Unknown optional profile values remain unknown and do not fail unless genuinely required.

### Serialization and schema

- Valid fixture JSON deserializes, validates, materializes, and serializes successfully.
- A DTO round trip preserves canonical meaning and IDs.
- Schema version `1` is recognized.
- Missing/malformed schema is rejected clearly.
- Unsupported newer schema fails with `schema.unsupported` and does not partially materialize.

### Immutability

- Materializing and querying a runtime world does not modify the source DTO/package or its serialized canonical meaning.
- Two worlds materialized from the same package do not share mutable collections/state.
- Exposed domain collections cannot be mutated by callers.

## Definition of done

- Only the three planned projects exist.
- Build and NUnit tests pass from the command line.
- No Unity reference or Unity-generated file exists.
- No basketball simulation, save persistence, database, or real NBA data exists.
- The synthetic package is readable in code review.
- All required validation failures are structured and tested.
- Domain types have no JSON dependency.
- Documentation is updated with any implementation-time decision that materially changes this plan.

## Explicitly deferred

- `StaffAssignment` and staff/coach careers
- `Arena` and venue/name histories
- contracts and salary/cap rules
- transactions and asset legs
- drafts, draft classes, draft picks, and selections
- injuries and availability
- awards and voting
- statistics, standings, box scores, and game results
- jersey/role assignments, depth charts, and roster-limit rules
- external provider importers and a full provenance system
- deterministic-ID generation unless separately approved
- content manifests/checksums beyond what is necessary for the single fixture
- actual migrations until schema V2 exists
- save containers and `SaveWorld`
- CLI, Unity, basketball simulation, AI, and presentation

Adding any deferred item requires a demonstrated dependency, not merely anticipated future usefulness.

## Implementation sequence

1. Restore foundation documents and record the .NET target decision.
2. Scaffold Core, Data, and one NUnit test project.
3. Implement typed IDs and their tests.
4. Implement the minimal immutable domain records.
5. Implement V1 JSON DTOs and schema envelope recognition.
6. Implement structured validation in the stated order.
7. Implement DTO-to-domain materialization with defensive copies/read-only collections.
8. Add the valid synthetic fixture and continuity tests.
9. Add invalid-reference, date-range, duplicate-ID, and schema-version tests.
10. Run all tests, review dependency direction, and update decisions/backlog without proceeding to Slice 02.
