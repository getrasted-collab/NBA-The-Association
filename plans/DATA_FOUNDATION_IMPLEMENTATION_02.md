# Data Foundation — Implementation Slice 02

## Purpose

Extend the working Slice 01 foundation into the smallest reliable package/import boundary needed before any real NBA data is considered.

Slice 02 proves:

- provider-neutral external ID mappings;
- minimal source provenance;
- a useful V2 package manifest;
- explicit known/unknown/not-applicable historical values;
- a small parse → normalize → validate → package pipeline;
- cross-era synthetic packages;
- one explicit V1 → V2 migration;
- minimal command-line inspection, validation, external-ID tracing, and package comparison.

This is a plan only. Do not implement it until explicitly authorized.

## Guardrails

Do not add Unity, basketball simulation, ratings systems, advanced attributes/tendencies, real NBA data, production databases, contracts, transactions/trades, drafts, free agency, injuries, statistics, saves, GM Mode, Player Mode, or UI/presentation.

Continue targeting `net10.0`. Core remains independent from JSON, files, providers, and CLI concerns. Use fixed internal GUIDs in synthetic fixtures; deterministic imported-ID generation remains deferred until its namespace/algorithm is separately decided.

## Assessment of Implementation 01

Implementation 01 is a sound base:

- Core and Data dependencies point in the correct direction.
- Domain and JSON DTOs are separated.
- Typed GUID identities, immutable/read-only materialization, schema recognition, and structured validation are established.
- The two-season fixture proves player and franchise continuity.
- The baseline suite passes: 9 tests, 0 failures.

No rewrite is justified.

## Technical debt to correct first

These are bounded hardening changes, not a redesign:

1. **Malformed/null package sections:** `data` or collection properties explicitly set to `null` may currently cause a null-reference failure instead of a structured issue. V2 loading must return `manifest.malformed` or `data.malformed` without throwing. Preserve V1 behavior through the migration reader.
2. **Repeated linear reference lookups:** the V1 validator repeatedly uses `FirstOrDefault`. Build per-type dictionaries once per validation run. This improves clarity and permits mapping/reference checks; it is not a generalized indexing framework.
3. **Unvalidated game status text:** V1 DTO status is ignored during materialization. The V1 migration reader should require the only supported value (`scheduled`) or report a structured issue; V2 retains the same minimal status scope.
4. **Thin envelope validation:** V1 does not validate package version or required text fields. V2 manifest validation must cover its required metadata.
5. **Placeholder rating field:** `ExampleSkillRating` was only a nullability proof. It must not evolve into a ratings model. V2 removes it from the runtime profile. The V1 reader retains it only for compatibility; migration emits an explicit informational/loss note if a value existed.
6. **Canonical GUID export:** official fixtures are lowercase, but arbitrary valid uppercase DTO strings are preserved by serialization. V2 package writing should canonicalize GUID strings without mutating the source DTO.

Do not refactor the repeated typed-ID structs, introduce source generators, or generalize the validator into a rules engine in this slice.

## Package schema V2

V2 is the canonical output of Slice 02. V1 remains readable only through the explicit migration path.

Proposed envelope:

```json
{
  "manifest": {
    "schemaVersion": 2,
    "packageId": "00000000-0000-0000-0000-000000000001",
    "packageVersion": "2.0.0",
    "createdAtUtc": "2026-10-06T18:00:00Z",
    "eraLabel": "Synthetic cross-era fixture",
    "seasonIds": ["..."],
    "provenanceIds": ["..."],
    "containedCategories": ["leagues", "seasons", "franchises", "teamSeasons", "players", "games"]
  },
  "provenance": [],
  "externalIdMappings": [],
  "data": {}
}
```

### Manifest fields

- `schemaVersion`: required integer, exactly `2` for V2.
- `packageId`: required non-empty canonical GUID.
- `packageVersion`: required non-empty package-content version string. Do not build a package resolver or dependency solver.
- `createdAtUtc`: required `DateTimeOffset`, normalized to UTC on canonical output.
- `eraLabel`: optional descriptive text only; it does not drive rules.
- `seasonIds`: required set of seasons the package declares it supports. Every ID must reference a contained Season.
- `provenanceIds`: source records used by the package; every ID must resolve.
- `containedCategories`: required sorted list of categories actually present. Validation checks declared versus actual categories.

Do not persist an authoritative “validated” flag. Validation status becomes stale as soon as a file changes. `inspect` and `validate` report validation status computed for the bytes being inspected. A future signed/build receipt is outside this slice.

Do not implement package dependencies, remote registries, asset bundling, compression, signatures, or content-addressed storage.

## Provenance model

Provenance belongs in `NBATheAssociation.Data`, not Core domain identity.

Minimal V2 record:

```text
ProvenanceRecord
  Id                  canonical GUID, package-local stable identity
  SourceName          required provider/dataset name; free text in V2
  SourceIdentifier    optional dataset/file/record locator
  RetrievedAtUtc      optional DateTimeOffset
  ImportedAtUtc       required DateTimeOffset
  ImporterVersion     required non-empty string
  TransformationVersion optional string
  UsageNotes          optional string
```

Licensing/usage notes are descriptive in V2; they do not constitute legal approval. Do not build license policy evaluation.

Entity-level provenance links are limited to external mappings in Slice 02. Do not add provenance fields to every domain record. Package-level provenance IDs describe the package as a whole.

## External ID mappings

Mappings also belong in Data/package metadata, not in Core entities.

```text
ExternalIdMapping
  SourceName
  EntityType
  ExternalIdentifier
  InternalId
  ProvenanceId (optional)
```

### Entity type

Use an explicit V2 data enum/string vocabulary for entity types currently supported by the package: league, season, franchise, teamSeason, person, player, ruleSet, and game. Do not add future entity types speculatively.

The mapping validator resolves `InternalId` against the collection selected by `EntityType`, preserving typed domain identity at materialization boundaries. Do not introduce an untyped universal domain entity ID.

### Mapping rules

- `(sourceName, entityType, externalIdentifier)` is the external key, compared with a documented ordinal/case policy. V2 recommendation: source names use ordinal-ignore-case; external identifiers are ordinal/case-sensitive.
- Two identical records are `external_mapping.duplicate`.
- The same external key pointing to different internal IDs is `external_mapping.conflict`.
- Multiple sources may map to one internal ID.
- A source may map distinct external identifiers to one internal ID; flag as a warning for review, not an error.
- `InternalId` must exist in the declared entity category.
- Optional `ProvenanceId` must exist.
- External IDs never alter or generate internal identity in Slice 02.

Do not hard-code NBA API, Basketball Reference, or any real provider in production enums or domain records. Synthetic fixtures may use names such as `SyntheticArchiveA`.

## Historical value availability

V1 null cannot distinguish unknown from not applicable. Add a small Core value type:

```text
HistoricalValueState: Known | Unknown | NotApplicable

HistoricalValue<T>
  State
  Value (accessible/valid only when State == Known)
```

Required invariants:

- Known requires a value.
- Unknown and NotApplicable must not carry a value.
- No implicit conversion from `T` or null.
- The type remains independent from JSON.

V2 JSON representation:

```json
{ "state": "known", "value": 75 }
{ "state": "unknown" }
{ "state": "notApplicable" }
```

Apply this only where the existing fixture needs semantic distinction:

- `PlayerSeasonProfile.ListedPosition`
- `PlayerSeasonProfile.HeightInches`

Remove `ExampleSkillRating` from the V2 runtime/DTO model; do not replace it with another rating. Ordinary optional metadata such as `BrandingReference` remains nullable and does not use `HistoricalValue<T>`.

V1 migration mapping:

- non-null V1 position/height → Known;
- null V1 position/height → Unknown;
- no V1 field maps automatically to NotApplicable;
- non-null `ExampleSkillRating` is intentionally omitted with a migration note, not silently discarded.

## Import pipeline foundation

Implement one narrow pipeline in `NBATheAssociation.Data`:

```text
raw bytes/text
  → parser result
  → normalized V2 DataPackage DTO
  → validator result
  → canonical package output (only when valid)
```

### Minimal contracts

- `IRawPackageParser<TRawRecord>`: parses input and returns records plus structured import issues.
- `IPackageNormalizer<TRawRecord>`: maps parsed records into a V2 package DTO without validating cross-record invariants.
- `PackageImportPipeline<TRawRecord>`: orchestrates parse, normalize, validate, and canonical serialize; stops packaging on errors.
- Reuse `ValidationIssue` or a closely aligned issue shape with a stage field. Do not create an exception hierarchy.

Provide exactly one synthetic adapter, preferably for a deliberately simple raw JSON fixture whose shape differs from the canonical package. It should normalize source keys, dates, and historical-value states into V2 records.

Do not add plugin discovery, reflection registration, dependency injection containers, streaming/batch infrastructure, HTTP clients, scraping, authentication, database staging, fuzzy matching, or deterministic ID generation.

## Validation additions

Preserve Slice 01 validation and add stable codes for:

### Manifest and provenance

- `manifest.missing`
- `manifest.malformed`
- `manifest.package_version.missing`
- `manifest.created_at.invalid`
- `manifest.season_reference.invalid`
- `manifest.category_mismatch`
- `provenance.id.duplicate`
- `provenance.required_field.missing`
- `provenance.reference.invalid`

### External mappings

- `external_mapping.duplicate`
- `external_mapping.conflict`
- `external_mapping.entity_type.unsupported`
- `external_mapping.internal_reference.invalid`
- `external_mapping.provenance_reference.invalid`
- `external_mapping.multiple_ids_for_entity` (warning)

### Historical values

- `historical_value.state.invalid`
- `historical_value.known_value.missing`
- `historical_value.unexpected_value`

### Cross-record relationships

- `team_season.identity.duplicate`: more than one TeamSeason for the same Franchise and Season in V2. Mid-season identity periods remain deferred.
- `profile.identity.duplicate`: more than one PlayerSeasonProfile for the same Player and Season.
- `roster.overlap`: the same Player has overlapping inclusive membership intervals within one Season. An open-ended interval runs through season end for validation.
- Retain missing-reference and wrong-season checks from Slice 01.

Do not add roster-size, eligibility, salary, position, game-result, schedule-balance, or basketball-rule validation.

## Synthetic cross-era fixtures

Keep fixtures fictional and small.

### Fixture A — Legacy V1

Retain the existing `synthetic-world.json` unchanged as the migration input and regression fixture.

### Fixture B — V2 cross-era package

Migrate/represent the existing two-season world with:

- one franchise relocating and rebranding;
- one stable player moving teams;
- two rules configurations;
- Known and Unknown historical values;
- at least two provenance records;
- multiple external sources mapping to the same Player or Franchise;
- a complete manifest.

### Fixture C — V2 sparse historical package

A very small earlier-style fictional league demonstrating:

- fewer teams and no modern structural assumptions;
- a field explicitly NotApplicable;
- another field explicitly Unknown;
- a different rules configuration;
- valid provenance and external mappings.

Invalid cases should generally be produced by test builders/mutations rather than copied JSON files.

## Explicit V1 → V2 migration proof

Implement one class/function such as `V1ToV2PackageMigration`—not a migration framework.

Behavior:

1. Recognize and deserialize a valid V1 package using frozen V1 DTOs.
2. Create a V2 manifest from V1 envelope fields plus migration parameters for creation time and provenance.
3. Convert profile nulls to Unknown and non-nulls to Known.
4. Preserve all durable entity IDs and relationships.
5. Add no external mappings unless explicitly supplied to the migration call.
6. Emit a structured migration report containing source/target versions, notes/warnings, and counts.
7. Validate the resulting V2 package before returning success.
8. Never mutate the V1 DTO or input JSON.

Do not add a registry, graph search, downgrade, multi-hop orchestration, or V3 placeholder. The package loader may dispatch V1 to this one migration and load V2 natively.

## Debugging and inspection CLI

Add one project:

```text
tools/NBATheAssociation.Cli/
```

Use only standard command-line argument parsing; do not add a command framework.

Commands:

- `validate <package>` — prints structured issues and exits nonzero for validation errors.
- `inspect <package>` — prints schema/package metadata, coverage, provenance sources, categories, and entity counts.
- `trace-external <package> <source> <entityType> <externalId>` — prints the internal target and provenance, or a clear not-found result.
- `diff <leftPackage> <rightPackage>` — prints the minimal comparison described below.

Output may be plain deterministic text. JSON CLI output, interactive editing, color frameworks, and shell completion are deferred.

## Minimal package comparison

Package comparison provides meaningful value now because imports and migrations need review. Include a narrow `PackageDiff` in Data and expose it through CLI.

Compare only:

- manifest package/schema/version/coverage/category changes;
- provenance IDs added/removed;
- external mapping keys added/removed/retargeted;
- entity IDs added/removed per contained category;
- counts for each change group.

Do not perform arbitrary JSON diffs, field-by-field semantic comparison, rename detection, or historical reconciliation.

## Expected project/file changes

### Existing projects

`NBATheAssociation.Core`:

- add `HistoricalValue<T>` and state enum;
- update PlayerSeasonProfile to use it for position/height;
- remove the placeholder skill field from V2 runtime state.

`NBATheAssociation.Data`:

- preserve frozen V1 DTO/reader for migration;
- add V2 manifest, provenance, external mapping, historical-value DTOs;
- add V2 serialization/materialization and hardened validation;
- add one explicit V1→V2 migration;
- add minimal import-pipeline contracts/orchestrator;
- add package inspection/diff services.

`NBATheAssociation.Tests`:

- retain Slice 01 regression coverage;
- add V2, migration, import, mapping/provenance, historical-value, overlap, CLI-service, and diff tests;
- add two V2 fixtures and one raw synthetic input.

### New project

`NBATheAssociation.Cli` referencing Data only (and Core transitively). No new NuGet dependency is expected.

Likely paths:

```text
src/NBATheAssociation.Core/HistoricalValue.cs
src/NBATheAssociation.Data/V1/
src/NBATheAssociation.Data/V2/
src/NBATheAssociation.Data/Import/
src/NBATheAssociation.Data/Migrations/V1ToV2PackageMigration.cs
src/NBATheAssociation.Data/Inspection/
tools/NBATheAssociation.Cli/
tests/NBATheAssociation.Tests/Fixtures/v2-cross-era.json
tests/NBATheAssociation.Tests/Fixtures/v2-sparse-historical.json
tests/NBATheAssociation.Tests/Fixtures/raw-synthetic-source.json
```

Do not split Data into more assemblies during Slice 02.

## Required tests

### Slice 01 regression

- Existing V1 fixture remains readable/migratable.
- Typed IDs, identity continuity, invalid references/ranges, immutability, and unsupported-newer-schema behavior remain covered.

### Historical values

- Known requires and preserves a value.
- Unknown and NotApplicable carry no value and remain distinct after round trip.
- Invalid state/value combinations fail validation.
- V1 null migrates to Unknown; no value is inferred.

### Manifest and provenance

- Valid V2 manifest loads.
- Missing/malformed package ID/version/date/coverage/categories fail structurally.
- Missing/duplicate provenance IDs and invalid references fail.
- Contained-category declarations match actual package contents.

### External mappings

- Multiple sources can map to one internal entity.
- Duplicate external keys fail.
- Conflicting external targets fail.
- Invalid entity type/internal ID/provenance reference fails.
- Trace lookup returns the correct typed category, internal ID, and provenance.

### Cross-record validation

- Duplicate Franchise+Season TeamSeason identity fails.
- Duplicate Player+Season profile identity fails.
- Overlapping memberships for one player in one season fail.
- Non-overlapping transfer memberships pass.
- Existing wrong-season checks still pass/fail appropriately.

### Import pipeline

- Synthetic raw input parses and normalizes into the expected V2 package.
- Parse errors stop before normalization/package output.
- Normalization/validation errors return structured issues.
- Valid import produces canonical JSON and does not mutate raw/parsed input.

### Migration

- Valid V1 migrates to valid V2.
- IDs and relationships are unchanged.
- historical null/value mapping follows the declared rules.
- placeholder skill omission is reported when applicable.
- migration is deterministic when supplied the same creation time/provenance inputs.
- original V1 JSON/DTO is unchanged.
- V2 round trip succeeds; unsupported schema newer than 2 fails clearly.

### Inspection and diff

- inspect returns correct metadata/counts.
- validate exposes stable issue codes.
- external trace returns found/not-found results.
- diff reports additions, removals, and mapping retargets without false changes for reordered JSON.
- CLI exit codes are tested through command-handler methods; process-spawning tests are unnecessary initially.

## Implementation sequence

1. Run the Slice 01 baseline and preserve its fixture/tests.
2. Harden malformed/null handling, status validation, canonical output, and lookup indexes.
3. Add `HistoricalValue<T>` and update the V2 PlayerSeasonProfile only.
4. Freeze V1 DTOs/reader under a V1 namespace; define the V2 manifest/DTOs.
5. Implement V2 provenance and external mappings plus validation.
6. Implement duplicate identity and roster-overlap validation.
7. Add the two valid V2 fixtures and focused invalid mutations.
8. Implement the explicit V1→V2 migration and report.
9. Add the narrow import pipeline and synthetic raw adapter/fixture.
10. Add package inspection, trace, and minimal ID-level diff services.
11. Add the CLI as a thin adapter over those services.
12. Run all tests, review dependency/scope boundaries, update decisions/questions/roadmap/backlog, and write the Slice 02 report.

## Definition of done

- Slice 01 regression suite remains green.
- V2 packages have validated manifests, provenance, mappings, and explicit historical-value states.
- Two V2 cross-era fixtures validate and use no real NBA data.
- One valid V1 package migrates explicitly and reproducibly to valid V2.
- Synthetic raw data passes through parse → normalize → validate → package.
- CLI can validate, inspect, trace, and minimally diff packages.
- No new third-party runtime dependencies are introduced.
- Core remains free of JSON/provider/CLI concerns.
- No deferred game, management, save, Unity, or presentation system is added.

## Premature or excluded proposals

- Real NBA/API/Basketball Reference adapters
- Deterministic imported-ID generation
- Provider plugin discovery or dependency injection framework
- Database staging or production database selection
- Remote package registry, dependency resolution, signing, compression, or asset packaging
- Full provenance at every field/entity
- License-policy enforcement
- Generic migration framework, downgrade, or V3 scaffolding
- Generic validation/rules engine
- Field-level semantic package diff
- Interactive/editor UI
- Player ratings, attribute/tendency systems, or basketball validation

## Remaining blockers

No blocker prevents planning or synthetic implementation after approval.

Before implementation, the following choices should be accepted as part of this plan or amended:

1. V2 uses the manifest structure above and treats runtime validation status as computed, not persisted.
2. Historical values use the explicit three-state wrapper only for semantically historical fields.
3. External-source names are provider-neutral strings with the stated comparison rules.
4. V1 migration intentionally removes the placeholder skill field and reports that omission.
5. A small standard-library CLI and ID-level diff are in scope.

Real-data licensing and deterministic imported-ID generation remain blockers only for later real import work.

