# Data Foundation Implementation 02 Report

## Outcome

Implementation Slice 02 is complete. The standalone .NET foundation now loads, validates, migrates, inspects, compares, and traces synthetic V2 data packages while preserving all Slice 01 behavior. No Unity, basketball, real-data, management, statistics, or presentation systems were added.

## Implemented

- Core `HistoricalValue<T>` with explicit `Known`, `Unknown`, and `NotApplicable` states. Values are accessible only when known.
- V2 package envelope and manifest with package identity/version, UTC creation time, era label, season coverage, provenance references, and contained categories.
- Minimal provenance records and provider-neutral external ID mappings, kept outside Core identity.
- Canonical V2 JSON serialization, deserialization, materialization, schema recognition, and structured diagnostics.
- Stronger cross-record validation for manifests, provenance, mappings, historical values, duplicate season identities/profiles, roster overlaps, games, references, and coverage.
- One explicit V1→V2 migration that preserves durable IDs, converts V1 null historical fields to `Unknown`, and reports omission of the V1 placeholder skill field.
- A narrow parse → normalize → validate → canonical-package pipeline with one synthetic raw JSON adapter.
- Package summary inspection, external-ID tracing, and an ID-level/package-metadata diff.
- A dependency-free CLI with `validate`, `inspect`, `trace-external`, and `diff` commands.
- Cross-era and sparse-historical fictional V2 fixtures plus a synthetic raw-input fixture.

## Resulting structure

```text
src/NBATheAssociation.Core/
  DomainModels.cs
  HistoricalValue.cs
  Ids.cs
  LeagueWorld.cs
src/NBATheAssociation.Data/
  PackageDtos.cs                  # frozen V1 DTOs
  DataPackageSerializer.cs        # V1 reader/writer
  DataPackageValidator.cs
  DataPackageMaterializer.cs
  V2PackageDtos.cs
  V2PackageSerializer.cs
  V2PackageValidator.cs
  V2PackageMaterializer.cs
  V1ToV2PackageMigration.cs
  ImportPipeline.cs
  PackageInspection.cs
tools/NBATheAssociation.Cli/
tests/NBATheAssociation.Tests/
  DataFoundationTests.cs
  DataFoundationV2Tests.cs
  Fixtures/
```

Files remain directly within the existing small projects; creating speculative subassemblies or folder frameworks was unnecessary.

## V2 package architecture

The package contains four explicit areas:

1. `manifest` describes schema and package identity, version, creation time, declared seasons, provenance, and categories.
2. `provenance` describes the synthetic source/import context without affecting domain identity.
3. `externalIdMappings` maps provider-neutral external keys to typed entity categories and internal GUID identities.
4. `data` contains the existing league-world records plus V2 historical-value profile DTOs.

Validated DTOs materialize an immutable/read-only `LeagueWorld`. Serialization canonicalizes GUID text, UTC timestamps, collection order, and metadata order without mutating the source DTO.

## Import pipeline

```text
raw JSON text
  → SyntheticRawJsonParser
  → SyntheticPackageNormalizer
  → V2PackageValidator
  → V2PackageSerializer canonical JSON
```

Each stage returns structured issues. Parse or normalize errors stop later stages; canonical output is produced only for a valid package. The adapter is deliberately synthetic and narrow, with no provider discovery, network, fuzzy matching, database staging, or deterministic ID generation.

## Migration result

`V1ToV2PackageMigration` accepts frozen V1 DTOs plus explicit creation/provenance inputs. It preserves entity IDs and relationships, maps non-null position/height to `Known`, maps null to `Unknown`, adds no inferred `NotApplicable` values or external mappings, reports placeholder-skill omission, validates its V2 result, and leaves source JSON/DTOs unchanged. Equal inputs produce equal canonical output.

## CLI capabilities

- `validate <package>` prints stable issues and returns nonzero for invalid data.
- `inspect <package>` prints schema/package metadata, season coverage, sources, categories, and entity counts.
- `trace-external <package> <source> <entityType> <externalId>` resolves the internal ID and provenance or reports not found.
- `diff <left> <right>` reports manifest changes, provenance/entity ID additions/removals, and mapping additions/removals/retargets.

The CLI uses only standard argument handling and references Data; it adds no command framework.

## Validation added

- Manifest presence/shape, package GUID/version/date, exact season coverage, and category declarations.
- Provenance uniqueness, required fields, and references.
- External mapping duplicates, conflicts, supported entity categories, internal/provenance references, and review warnings for multiple source IDs.
- Historical state vocabulary and state/value invariants.
- Unique Franchise+Season TeamSeason and Player+Season profile identities.
- Inclusive same-season roster overlap detection and season date bounds.
- Existing entity references, scheduled-game status, participant season, and game/season bounds.
- Unsupported schemas fail clearly rather than being interpreted or downgraded.

## Tests and verification

- Full solution build: successful.
- Complete NUnit suite: **19 passed, 0 failed, 0 skipped** on `net10.0`.
- CLI `inspect` smoke test: successful against the V2 cross-era fixture.
- Slice 01's 9 regression tests remain included and passing.

The new tests cover both V2 fixtures, state invariants, manifests/provenance, mapping rules/tracing, duplicate identities, roster overlaps, migration determinism and immutability, V2 round trips/schema rejection, import stage failures/success, reorder-insensitive diffing, inspection, and CLI exit codes.

## Regressions found and fixed

- Local validator overloads for string/integer historical DTOs conflicted at compilation; they were made explicit helpers.
- Restore metadata initially pointed at another machine's NuGet cache; a forced solution restore regenerated local assets. No dependency version changed.
- Historical integer unknowns could expose a default value; Core now throws on value access unless the state is `Known`.

## Deviations

No architectural deviation from the approved plan was required. Files were kept flat inside the small Data project instead of creating the plan's illustrative `V1/`, `V2/`, and `Import/` folders; this avoids organizational complexity without changing boundaries. The synthetic normalizer intentionally embeds a canonical package template inside a different raw-record envelope and normalizes its creation time; it proves stage isolation but is not a real-source adapter.

## Remaining technical debt and risks

- The synthetic adapter is intentionally minimal; a future controlled source needs explicit field-level normalization rather than template embedding.
- V1 DTOs remain in their original namespace for regression compatibility instead of being physically moved.
- Validation is an explicit service, not a generic rule framework; this is desirable now but should be monitored as categories grow.
- Deterministic imported IDs remain forbidden until namespace, algorithm, normalization, and collision policy are decided.
- Real-data licensing, usage constraints, corrections, and source governance remain unresolved.
- Mid-season identities, ambiguous franchise lineage, field-level provenance, unknown-field preservation, and database choices remain deferred.

## Recommended next slice

Plan Slice 03 before implementation. The recommended decision is either:

1. a small authoring/import-hardening slice that replaces the template-based synthetic adapter with explicit field-level normalization and decides deterministic IDs only if real-data experiments are next; or
2. pause Data Foundation at this proven boundary and plan Phase 2 league/calendar/schedule workflows using synthetic V2 packages.

Do not begin real NBA import until both deterministic identity generation and data-rights policy are explicitly resolved.
