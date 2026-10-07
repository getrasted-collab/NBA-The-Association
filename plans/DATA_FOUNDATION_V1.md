# Data Foundation V1 Implementation Plan

## Status and scope

- **Phase:** 1 — Data Foundation
- **Status:** Approved direction; first implementation slice awaits only the remaining blockers listed below
- **Date:** 2026-10-06
- **In scope:** identity, canonical schemas, data layers, packaging, provenance, validation, migrations, import/export contracts, and synthetic fixtures
- **Out of scope:** Unity, basketball simulation, full production save UI, and importing a full NBA dataset

This plan refines the accepted foundation: a Unity-independent core, one shared world for all modes, strict separation of base data from saves, and historical variability from the beginning.

## V1 goal and proof

V1 must describe and validate different basketball worlds without code changes or name-based references. It is complete when one canonical model loads three small synthetic packages:

1. a modern-like league;
2. a smaller historical league without modern alignment or rules;
3. a league whose membership, branding, venue, and selected rules change between seasons.

## The five data layers

### 1. Immutable/base source data

Raw or manually authored source material: spreadsheets, public data, research notes, corrections, mappings, and provenance. It may be provider-specific, incomplete, contradictory, or denormalized. Runtime code never reads arbitrary source files directly.

Each source record needs a source ID, provider/publication, retrieval date, source locator, license/usage status, transformation notes, and confidence where facts are uncertain. Corrections create versioned revisions or overrides rather than erasing provenance.

### 2. Starting-season data

A validated immutable content package used to create a world. It contains canonical identities, pre-start historical facts, season and league structure, participating teams, rules references, starting rosters/contracts/injuries as applicable, and a schedule or scheduling inputs.

Each package has `DataPackageId`, content version, schema version, manifest/checksums, dependencies, start boundary, and provenance. A playthrough never edits it.

### 3. Runtime league state

The authoritative in-memory state for one world: date, active memberships, player state, contracts, injuries, draft assets, scheduled/completed games, transactions, accumulated records, and generated entities. Domain objects use typed IDs and invariants; they are not storage DTOs. Rebuildable indexes and read models are not independent truth.

### 4. Save-game state

A durable serialization of authoritative runtime state plus compatibility metadata. It must resume the same world without silently reinterpreting it against a newer content package. Exact snapshot/content-pack policy is blocking and remains behind a persistence interface.

### 5. Generated future data

People, draft classes, schedules, games, contracts, injuries, awards, transactions, league changes, and history created after the save boundary. These use the same canonical types and identity rules as imported records, while recording generator/version, `WorldId`, world creation date, and relevant deterministic seed/stream provenance. They never flow back into the starting package.

## Stable identity system

### Rules

- Durable entities use opaque IDs independent of names, abbreviations, jersey numbers, teams, or years.
- Domain code uses typed IDs: `PlayerId` cannot substitute for `FranchiseId`.
- IDs are never recycled after retirement, contraction, deletion, or correction.
- External IDs live in provider-namespaced mapping records, never as canonical keys.
- Imported and generated identities use a collision-safe allocation policy.
- Merges/splits preserve aliases and provenance; they do not silently rewrite history.
- Mutable slugs may aid debugging but never serve as references.

Per ADR-007, physical IDs are UUID/GUID values serialized in canonical string form and wrapped in strongly typed domain IDs. Examples retain readable labels for explanatory clarity; executable fixtures must use canonical GUID strings. Deterministic imported IDs require a separately documented namespace, algorithm, source-key normalization, and collision policy before implementation.

| Entity | Typed ID | Lifetime |
|---|---|---|
| Person | `PersonId` | One human across roles |
| Player career | `PlayerId` | One playing career linked to a person |
| Franchise | `FranchiseId` | Enduring lineage under the chosen lineage policy |
| Team-season | `TeamSeasonId` | One league participant in one season |
| League | `LeagueId` | Enduring competition identity |
| Season | `SeasonId` | One league season definition |
| Coach career | `CoachId` | Coaching identity linked to a person |
| Staff career | `StaffMemberId` | Staff identity linked to a person |
| Arena | `ArenaId` | Physical venue across name changes |
| Game | `GameId` | One scheduled contest through status changes |
| Contract | `ContractId` | One executed agreement and amendment history |
| Transaction | `TransactionId` | One atomic/auditable league transaction |
| Draft | `DraftId` | One draft event |
| Draft class | `DraftClassId` | One prospect/eligibility cohort |
| Draft-pick asset | `DraftPickId` | One tradable selection right |
| Draft selection | `DraftSelectionId` | The act/result of using a pick |
| Injury | `InjuryId` | One injury episode |
| Award definition | `AwardId` | Enduring award identity |
| Award result | `AwardResultId` | One season/event result |
| Rule configuration | `RuleSetId` | Versioned composed rules |
| Content package | `DataPackageId` | Immutable starting package |
| Save world | `WorldId` | One alternate timeline/playthrough |

Separating `PersonId` from role IDs allows one human to play, coach, and work as staff without becoming unrelated identities. Whether all role records ship in the first schema is non-blocking if the separation is reserved now.

## Major entities and relationships

### V1 complexity review

The entity catalogue below preserves long-term boundaries; it is not a command to implement every entity now.

| Entity | Slice 01 treatment |
|---|---|
| Person, Player, PlayerSeasonProfile | Implement minimally |
| League, Season, Franchise, TeamSeason | Implement minimally |
| RuleSet, RosterMembership, Game | Implement minimally |
| DataPackage | Implement only the JSON schema/version envelope and collections needed by the fixture |
| SaveWorld | Do not implement; ADR-009 records its future boundary |
| StaffAssignment, Arena | Defer; optional strings or IDs must not create fake entities in Slice 01 |
| Contract, Transaction, Draft, DraftPick, DraftSelection | Defer completely |
| Injury, Award, statistics | Defer completely |
| External-ID mappings and full provenance | Preserve a clean extension point; add only if needed to prove the synthetic package |

Avoid generic entity frameworks, arbitrary metadata dictionaries, universal effective-dated base classes, event sourcing, repository abstractions, and lineage graph engines in Slice 01. Concrete immutable records and focused validators are enough.

### League, season, and rules

`League` owns enduring competition identity, not a fixed team count or alignment. `Season` belongs to a league and owns date bounds, stages/structure references, participating `TeamSeasonId` values, schedule configuration, rule-set references, standings rules, and a statistical-definition version.

`RuleSet` composes versioned rule components selectable by season, stage, competition, or effective date. Do not create one large `Era` switch.

### Franchise versus team identity in a season

`Franchise` represents organizational lineage, continuity, and franchise-level history. It contains no timeless “current city,” nickname, arena, conference, or roster.

`TeamSeason` is a franchise’s participant record in a particular league season. It references exactly one `FranchiseId` and `SeasonId` and owns season-specific identity and structure:

- market/city, display name, abbreviation, and branding reference;
- arena/home-venue assignments;
- conference/division/group memberships;
- active dates if applicable;
- roster and staff assignments;
- season-specific strategy/finance references when those systems exist.

A relocation or rebrand creates a new season-specific identity while retaining `FranchiseId`, unless the chosen lineage policy declares a new franchise. Queries can aggregate by franchise, participant, brand, market, or season without conflation. If mid-season identity changes are required, add effective-dated `TeamIdentityPeriod` records beneath `TeamSeason`; do not complicate V1 until confirmed.

### People, players, coaches, and staff

`Person` owns biographical identity independent of role. `Player`, `Coach`, and `StaffMember` reference the person and own role/career identity.

A player does not permanently own a team, number, position, rating, tendency, role, or contract. Time-varying data uses:

- `PlayerSeasonProfile`: season/effective-period ratings, attributes, tendencies, listed positions, measurements, development baseline, and confidence;
- `RosterMembership`: player, team-season, effective dates, roster status, and acquisition/departure links;
- `JerseyAssignment`: number, team-season, and effective dates;
- `RoleAssignment`: basketball/organizational role and effective dates;
- `Contract`/`ContractTerm`: legal agreement and season-specific terms;
- `Injury`/availability records;
- game/season statistics keyed to player, team context, scope, and definition version.

Coach/staff employment is likewise an effective-dated relationship to franchise or team-season, so changing employers or roles does not replace identity.

### Arena

`Arena` is the physical venue. Effective-dated `ArenaNamePeriod` handles naming rights. `TeamSeasonVenueAssignment` links team-seasons to one or more venues with dates and priority, supporting moves and temporary venues.

### Contracts and salaries

`Contract` references the player/person, league, signing franchise, execution/effective dates, currency, rule context, status, and source transaction. Child `ContractTerm` records hold season-specific compensation, guarantees, bonuses, options, and later cap-treatment inputs.

Employment, roster membership, and financial obligation are related but distinct. Trades/amendments append explicit history rather than overwriting the original agreement. Phase 8 will finalize cap/contract semantics; V1 preserves identity and temporal boundaries.

### Drafts and draft assets

`Draft` belongs to a league and rules context. `DraftClass` identifies an imported or generated eligibility/prospect cohort and does not imply selection.

`DraftPick` is the tradable right, with origin, draft/round or slot basis, current controller, conditions/protections, and status. `DraftSelection` records use of that asset, resolved slot, selecting team-season, and player. Selection does not overwrite asset provenance.

### Games, schedules, and statistics

`Game` references season/stage, scheduled time, home/away `TeamSeasonId`, venue, rules, status, and source mapping. Postponement/rescheduling preserves `GameId` and creates auditable schedule history; cancellation never recycles it.

Statistics carry subject, game/season scope, team context, and `StatDefinitionSetId`. Authoritative results and rebuildable aggregates must be distinguished. Event logs and box-score schemas wait for the simulation phases.

### Transactions, injuries, awards, and history

`Transaction` is immutable/auditable and records type, effective time, participants, asset movements, related contracts/memberships, status, and provenance. Complex trades may have multiple transaction legs; details are deferred.

`Injury` is an episode with occurrence, status history, expected/actual recovery, body/type vocabulary, and context. Availability is a consequence/state, not a replacement for episode history.

`Award` is an enduring definition; `AwardResult` records season/event, recipient(s), team context, voting/tie metadata, and provenance. Renaming an award does not replace its identity.

League history is composed from immutable facts/events—seasons, team-seasons, games, transactions, records, awards, and franchise changes—plus optional narrative annotations, not one mutable history blob.

## Relationship map

```text
League 1 ── * Season 1 ── * TeamSeason * ── 1 Franchise
                     │             │
                     │             ├── * RosterMembership * ── 1 Player ── 1 Person
                     │             ├── * StaffAssignment * ── 1 Coach/Staff ── 1 Person
                     │             └── * VenueAssignment * ── 1 Arena
                     │
                     ├── * Game ── home/away TeamSeason
                     ├── 1 composed RuleSet
                     ├── 0..1 Draft ── * DraftPick ── 0..1 DraftSelection
                     └── * statistics / awards / standings inputs

Player ── * PlayerSeasonProfile
Player ── * Contract ── * ContractTerm
Player ── * Injury
Transaction ── * asset/roster/contract movements
DataPackage ── materializes ──> World/save timeline
```

## Ownership and mutability

| Data | Source/base | Starting package | Runtime/save | Future generated |
|---|---|---|---|---|
| Canonical identities | versioned | pinned | referenced/preserved | newly allocated |
| Pre-start history | immutable facts | pinned | read-only context | no |
| Season/rules at start | versioned | pinned | instantiated | future versions/configurations |
| Rosters/contracts/injuries | source facts | initial snapshot | command-mutated authority | world-created changes |
| Schedule/games | source/constraints | initial data | statuses/results authoritative | future schedules/games |
| Statistics/awards | historical facts | optional history | accumulated or rebuildable by policy | world-created facts |
| Provenance/mappings | versioned | included as needed | retained | generation provenance |

## Historical-data rules

- Temporal facts use dates, season IDs, or explicit effective intervals; “current” is a query.
- Corrections produce a new package revision with provenance.
- A save has a history boundary: pre-start facts are imported; post-start facts belong to its alternate timeline.
- Scheduled real-world future changes are optional/configurable content events, never silently forced; final policy is non-blocking now.
- Rules and statistical definitions are versioned by applicable time/scope.
- Unknown historical values stay explicitly unknown; zero, blank, or modern defaults cannot replace missing evidence.

## Save-state requirements

ADR-009 establishes a versioned hybrid/self-contained direction: the save carries enough immutable starting-world information and mutable state to reconstruct the world without the original package. Large presentation assets remain external references with fallbacks. The eventual save must include or resolve:

- `WorldId`, save/schema/build versions;
- exact `DataPackageId`, content version, dependency manifest, and checksum;
- start boundary and current date;
- authoritative snapshots or approved event/snapshot combination;
- generated-ID allocator/generator versions;
- required deterministic RNG state;
- migration history and write metadata;
- integrity checksum and optional backup lineage;
- authoritative data versus rebuildable cache markers.

Writes should use temporary write, validation/checksum, and atomic replace with recoverable backup where supported. This becomes executable in Phase 6.

## Versioning and migrations

Use independent version axes:

- schema version;
- content-package version;
- rules version;
- statistical-definition version;
- generator version;
- application build version.

Friendly package versions should be backed by a checksum-addressed manifest. ADR-010 requires explicit versions, ordered migrations, graceful rejection of unsupported newer versions, and no silent downgrade. The support window and unknown-field policy remain unresolved.

Migration rules:

1. Deserialize into version-specific persistence DTOs, never directly into domain entities.
2. Verify envelope, checksum, and declared version.
3. Apply explicit ordered deterministic N→N+1 migrations.
4. Preserve the original and write migrated output separately.
5. Emit structured steps, warnings, defaults/losses, and resulting checksum.
6. Validate before domain materialization.
7. Maintain golden fixtures for every supported version and multi-hop path.
8. Reject unsupported newer versions rather than best-effort parsing.
9. Lossy semantic migrations require an explicit decision and compatibility communication.

Unknown-field preservation and supported version window are blocking.

## Validation

Every issue contains `code`, `severity`, `entityType`, `entityId`, `fieldPath`, `message`, source/provenance location, and optional remediation.

Validation stages:

1. syntactic: parse, required fields, types, vocabularies;
2. referential: typed references exist and target correct entity types;
3. temporal: valid intervals and prohibited overlaps/gaps;
4. domain: participation, roster, contract, schedule, and draft constraints under referenced rules;
5. accounting: transaction legs, salary terms, game/stat totals, and standings inputs as systems arrive;
6. package: manifest, checksums, dependencies, provenance, licensing declarations;
7. cross-era: no accidental modern-only assumptions.

Validation codes and vocabularies are versioned; severity changes require documentation.

## Import pipeline

```text
Acquire/preserve raw source + provenance
  → provider-specific parser
  → staging records
  → normalize units/dates/vocabularies
  → resolve external IDs
  → deduplicate/conflict review
  → canonical records
  → validate
  → immutable package + manifest/checksums
  → fixture/load test
```

Import adapters are provider-specific; canonical records are provider-neutral. Fuzzy name matching may suggest identity candidates but never silently merges them. Transformations and manual overrides are reproducible/versioned. Failed or ambiguous records enter a review report. Licensing approval gates commitment and distribution.

## Export and debugging

Tools must eventually:

- export any entity by ID with references and provenance;
- diff packages/saves structurally and semantically;
- trace a value to source or generated event;
- inspect franchise/team-season lineage;
- inspect a player timeline across teams, numbers, profiles, roles, contracts, injuries, and statistics;
- produce machine- and human-readable validation/migration reports;
- export a minimal reproducible world subset while preserving dependencies;
- rebuild and compare derived data;
- run without Unity.

## Schema examples

These JSON-like examples demonstrate relationships only. IDs, optionality, money/date representation, and serialization syntax are not final.

### Player

```json
{
  "playerId": "player_example_001",
  "personId": "person_example_001",
  "careerStatus": "active",
  "provenance": { "kind": "synthetic_fixture", "sourceRecordId": "fixture-player-1" },
  "seasonProfiles": [{
    "seasonId": "season_example_2025",
    "effectiveFrom": "2025-07-01",
    "listedPositions": ["center"],
    "attributeSetId": "attributes_example_player_001_2025",
    "tendencySetId": "tendencies_example_player_001_2025"
  }]
}
```

Team, number, role, contract, injuries, and statistics are separate relationships.

### Franchise

```json
{
  "franchiseId": "franchise_example_001",
  "leagueId": "league_example_001",
  "lineageStatus": "active",
  "foundedOn": "1980-06-01",
  "provenance": { "kind": "synthetic_fixture", "sourceRecordId": "fixture-franchise-1" }
}
```

No timeless city, nickname, abbreviation, conference, arena, or roster is stored here.

### Team-season

```json
{
  "teamSeasonId": "teamseason_example_001_2025",
  "franchiseId": "franchise_example_001",
  "seasonId": "season_example_2025",
  "identity": {
    "market": "Example City",
    "name": "Comets",
    "abbreviation": "EXC",
    "brandingId": "branding_example_comets_2025"
  },
  "alignmentMemberships": [{ "groupId": "conference_example_east", "type": "conference" }],
  "venueAssignments": [{ "arenaId": "arena_example_001", "effectiveFrom": "2025-07-01", "effectiveTo": null }]
}
```

### Contract

```json
{
  "contractId": "contract_example_001",
  "leagueId": "league_example_001",
  "playerId": "player_example_001",
  "signingFranchiseId": "franchise_example_001",
  "executedOn": "2025-07-06",
  "effectiveFrom": "2025-07-06",
  "status": "active",
  "currency": "USD",
  "terms": [{
    "seasonId": "season_example_2025",
    "baseSalaryMinorUnits": 1250000000,
    "guaranteedMinorUnits": 1250000000,
    "optionType": "none"
  }],
  "sourceTransactionId": "transaction_example_signing_001"
}
```

### Game

```json
{
  "gameId": "game_example_2025_0001",
  "seasonId": "season_example_2025",
  "stageId": "stage_example_regular_season",
  "scheduledStart": "2025-10-21T19:00:00-05:00",
  "homeTeamSeasonId": "teamseason_example_001_2025",
  "awayTeamSeasonId": "teamseason_example_002_2025",
  "arenaId": "arena_example_001",
  "ruleSetId": "ruleset_example_2025_regular",
  "status": "scheduled",
  "resultId": null
}
```

### Season

```json
{
  "seasonId": "season_example_2025",
  "leagueId": "league_example_001",
  "label": "2025-26",
  "startsOn": "2025-07-01",
  "endsOn": "2026-06-30",
  "teamSeasonIds": ["teamseason_example_001_2025", "teamseason_example_002_2025"],
  "competitionStructureId": "competition_example_2025",
  "defaultRuleSetId": "ruleset_example_2025",
  "standingsRuleSetId": "standings_rules_example_2025",
  "statDefinitionSetId": "stats_example_2025"
}
```

## Testing strategy

- **Contracts:** golden schema fixtures, required/optional fields, stable serialization, unsupported-version rejection.
- **Identity:** duplicate/cross-type ID failures; names/numbers may change; external mappings never change canonical IDs.
- **Temporal/history:** franchise rebrand/relocation retains lineage; a player changes team, number, profile, role, and contract without changing `PlayerId`; team count and rules change without schema branching.
- **Migration:** one-step and multi-hop golden migrations; original preservation; deterministic output; structured failure for loss/unsupported versions.
- **Import/package:** reproducible raw→canonical fixture; checksums; ambiguous identity review; locatable validation issues.
- **Save boundary:** base package remains unchanged; worlds evolve independently; generated IDs do not collide; exact package dependencies are enforced.

## Remaining blocking decisions

Most Phase 1 blockers are resolved by ADR-006 through ADR-013. Before Implementation Slice 01:

1. Select the exact supported .NET target framework.
2. If deterministic imported IDs are generated in Slice 01, document the UUID algorithm/version, namespace UUIDs, source-key normalization, and collision policy first. Fixed fixture GUID constants may safely defer this.
3. Restore the missing foundation documents in the authoritative repository so implementation does not begin with incomplete permanent context.

The exact save container, backup/recovery mechanics, compatibility window, ambiguous lineage vocabulary, and real-data rights policy remain important gates for their later phases, but they do not block the synthetic first slice.

## Important but non-blocking choices

- Whether all person/role types ship in V1 or are merely reserved.
- Mid-season `TeamIdentityPeriod` support.
- How real future historical changes enter alternate timelines.
- Money/currency precision and later cap-accounting semantics.
- Which derived statistics/read models are persisted.
- Provider confidence/conflict taxonomy.
- First supported historical range and completeness bar.
- Detailed contract, transaction-leg, pick-protection, injury, and award schemas for later phases.

## Recommended implementation order

1. Resolve the remaining Slice 01 blockers and restore permanent foundation context.
2. Implement the deliberately small scope in `DATA_FOUNDATION_IMPLEMENTATION_01.md`.
3. Validate one two-season synthetic world that proves player and franchise continuity.
4. After that proof, add the two additional cross-era packages required by the broader V1 exit criteria.
5. Add provider-neutral external mappings and provenance incrementally; do not build a production importer yet.
6. Add real migration code only when a second schema exists; V1 needs version recognition and unsupported-version failure first.
7. Defer contracts, staff, arenas, drafts, transactions, injuries, awards, statistics, save persistence, and debug tooling until their next justified slices.

## Exit criteria

- Blocking choices are recorded in `docs/DECISIONS.md`.
- Three synthetic cross-era packages validate under one model.
- No durable relationship depends on a display name.
- Relocation/rebranding retains the intended franchise history.
- Player team, number, profile, role, and contract can vary without identity replacement.
- Package, provenance, typed-reference, and validation tests pass. Version recognition and unsupported-version behavior are tested; migration-chain tests begin when V2 exists.
- Unity, basketball simulation, and full real-world import remain uninitialized.

## Repository integrity note

The project owner has confirmed `C:\Users\Owner\Documents\ChatGPT\NBA` as authoritative. At review time, several permanent foundation files expected in that repository were absent. Restore them before implementation; the repository location itself is no longer an open question.
