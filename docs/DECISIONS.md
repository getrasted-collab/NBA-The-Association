# Decision Log

Record durable architecture and game-system decisions here. Do not silently rewrite accepted entries. A changed decision receives a new entry that supersedes the old one.

## Template

### ADR-NNN — Title

- **Status:** Proposed | Accepted | Superseded
- **Date:** YYYY-MM-DD
- **Decision:** What was chosen.
- **Reason:** Why it was chosen.
- **Alternatives considered:** Meaningful alternatives.
- **Consequences:** Benefits, costs, constraints, and follow-up work.

## Foundation decisions

### ADR-001 — Separate simulation from Unity and presentation

- **Status:** Accepted
- **Date:** 2026-10-05
- **Decision:** Core domain, league, and basketball simulation logic will run headlessly and avoid dependencies on Unity or UI APIs wherever practical. Unity will be a host and presentation adapter.
- **Reason:** Batch simulation, automated testing, deterministic debugging, portability, and maintainability are foundational requirements.
- **Alternatives considered:** Put gameplay logic directly in Unity components; rejected because it couples correctness to scenes and engine lifecycle.
- **Consequences:** Explicit application APIs and adapters are required. Unity-specific models may translate to/from core models.

### ADR-002 — Share one world model across game modes

- **Status:** Accepted
- **Date:** 2026-10-05
- **Decision:** Default, GM, and Player modes are perspectives over a common league and simulation world.
- **Reason:** All modes must inhabit the same coherent NBA universe.
- **Alternatives considered:** Separate mode-specific engines/world models; rejected due to inconsistency and duplication.
- **Consequences:** Mode-specific features use shared commands and state, adding only role-specific information.

### ADR-003 — Separate base data from save state

- **Status:** Accepted
- **Date:** 2026-10-05
- **Decision:** Versioned base data creates a starting world; playthrough mutations exist only in runtime/save state.
- **Reason:** Protect starting data and support multiple saves, history, provenance, and migrations.
- **Alternatives considered:** Mutate one shared database; rejected as unsafe.
- **Consequences:** New-game materialization, package identity, and save compatibility are explicit concerns.

### ADR-004 — Historical variability is a foundation constraint

- **Status:** Accepted
- **Date:** 2026-10-05
- **Decision:** Early models and tests must support variable league size, membership, identity, and effective rules.
- **Reason:** Modern-only assumptions would be expensive to remove later.
- **Alternatives considered:** Generalize only after building a modern league; rejected due to rewrite risk.
- **Consequences:** Synthetic fixtures must include historical structural variation.

### ADR-005 — Do not initialize Unity or the basketball engine during the foundation

- **Status:** Accepted
- **Date:** 2026-10-05
- **Decision:** Foundation work is limited to architecture, planning, data contracts, and supporting tests when implementation is authorized.
- **Reason:** Core boundaries and data decisions must settle before presentation or simulation complexity.
- **Alternatives considered:** Start with a Unity shell or possession simulator; rejected as premature.
- **Consequences:** Unity and basketball simulation remain explicitly out of scope.

## Phase 1 decisions

### ADR-006 — C#/.NET standalone core with NUnit and minimal dependencies

- **Status:** Accepted
- **Date:** 2026-10-06
- **Decision:** Use C# and .NET for the standalone core. Organize future responsibilities conceptually as Core, Data, Simulation, tests, and CLI tooling, but create only projects required by the current slice. Use NUnit unless a demonstrated compatibility problem requires a later change. Prefer the standard library and document every significant third-party dependency.
- **Reason:** This supports headless execution, testability, and eventual Unity integration while avoiding premature project proliferation.
- **Alternatives considered:** Unity-first assemblies, a different test framework, or dependency-heavy infrastructure; rejected for the initial foundation.
- **Consequences:** The exact supported .NET target must still be selected before scaffolding. The first slice should omit Simulation and CLI projects unless actually needed.

### ADR-007 — Strongly typed GUID identities and provider-neutral external mappings

- **Status:** Accepted
- **Date:** 2026-10-06
- **Decision:** Durable entities use strongly typed IDs backed by UUID/GUID values serialized in canonical string form. Domain APIs use types such as `PlayerId`, `FranchiseId`, and `TeamSeasonId`, not untyped GUIDs. External mappings separately record source, entity type, external identifier, and internal entity ID. Immutable imported entities may use deterministic IDs, but their exact namespace/generation method must be documented before use. Save-generated entities may use newly generated GUIDs.
- **Reason:** Stable typed identity prevents accidental cross-entity references and decouples the domain from names and data providers.
- **Alternatives considered:** Names, database keys, provider IDs, untyped GUIDs, or random IDs for every import; rejected because they weaken stability or type safety.
- **Consequences:** A deterministic UUID algorithm/namespace remains a blocker before deterministic import is implemented. External mappings require uniqueness validation without making any provider authoritative.

### ADR-008 — JSON is the V1 authoring and interchange format

- **Status:** Accepted
- **Date:** 2026-10-06
- **Decision:** Use JSON for V1 canonical authoring/interchange and synthetic fixtures. Keep domain models independent of JSON DTOs and serialization attributes where practical.
- **Reason:** JSON is readable, inspectable, diffable, generatable, validatable, and easy to debug.
- **Alternatives considered:** YAML, database-first authoring, binary formats; deferred because they add ambiguity or tooling needs without helping the first proof.
- **Consequences:** JSON is not promised as the final high-performance runtime/save format. Persistence DTOs and mapping boundaries are required.

### ADR-009 — Immutable packages materialize in-memory worlds; saves are hybrid and self-contained

- **Status:** Accepted
- **Date:** 2026-10-06
- **Decision:** Validated immutable packages materialize an authoritative in-memory runtime world. Saves contain enough immutable starting-world information plus mutable state to reconstruct reliably even if the original package changes or disappears. Large presentation assets are referenced with fallback behavior and are not embedded per save.
- **Reason:** Save reliability and recoverability matter more than early size optimization.
- **Alternatives considered:** Package-reference-only saves and fully embedded presentation assets; rejected as fragile and wasteful respectively.
- **Consequences:** The exact save container, atomic-write/backup mechanics, and compatibility support window remain to be decided before Phase 6, not before the first data slice.

### ADR-010 — Explicit schema versions and ordered migrations

- **Status:** Accepted
- **Date:** 2026-10-06
- **Decision:** Every persistent package/save format declares an explicit schema version. Known old versions migrate through explicit, testable, ordered steps. Unsupported newer versions fail gracefully with a clear diagnostic and are never silently downgraded or interpreted as current.
- **Reason:** Long-lived data needs predictable compatibility and safe failure.
- **Alternatives considered:** Best-effort parsing, implicit current-version assumptions, and silent downgrade; rejected as corruption risks.
- **Consequences:** V1 needs a version envelope and unsupported-version tests. Actual migration code is added only when a second schema exists. The compatibility window and unknown-field preservation policy remain unresolved.

### ADR-011 — Franchise continuity and season-specific TeamSeason identity

- **Status:** Accepted
- **Date:** 2026-10-06
- **Decision:** `Franchise` is the enduring organizational/history identity. `TeamSeason` describes that franchise's identity and participation in one season, including name, market, abbreviation, branding reference, arena, alignment, and season configuration. Ordinary relocation/rebranding preserves `FranchiseId`. Future explicit lineage data may express expansion, contraction, reactivation, predecessor/successor relationships, and officially reassigned history.
- **Reason:** Historical identity must survive ordinary presentation and location changes without hard-coded modern assumptions.
- **Alternatives considered:** Treat each rebrand as a new franchise or put mutable current branding on Franchise; rejected because both corrupt historical queries.
- **Consequences:** V1 proves ordinary continuity only. Ambiguous/reassigned lineage is deliberately deferred rather than encoded as ad hoc exceptions.

### ADR-012 — Preserve historical unknowns and source provenance

- **Status:** Accepted
- **Date:** 2026-10-06
- **Decision:** Historical unknown/unavailable values remain distinct from known values. Modern defaults are applied only through an explicit, documented transformation. Imported data provenance is separate from identity and may include source, source ID, retrieval date, import version, transformations, and license/usage information.
- **Reason:** Silent imputation creates false historical certainty and makes corrections unauditable.
- **Alternatives considered:** Fill missing values with zero, empty values, or current-era defaults; rejected as misleading.
- **Consequences:** DTOs and validation must represent optional/unknown values intentionally. Provenance can begin minimally for synthetic data and expand with real imports.

### ADR-013 — Synthetic data precedes real NBA import

- **Status:** Accepted
- **Date:** 2026-10-06
- **Decision:** Phase 1 uses small fictional fixtures to prove identity, temporal relationships, serialization, validation, and immutability. A full real NBA dataset will enter later through controlled import pipelines.
- **Reason:** Architecture can be tested without licensing risk, data noise, or premature scale.
- **Alternatives considered:** Begin with a complete real-world import; rejected as a distraction from structural proof.
- **Consequences:** Real-data licensing remains unresolved but does not block the synthetic first slice.

### ADR-014 — Target .NET 10 for the initial standalone projects

- **Status:** Accepted
- **Date:** 2026-10-06
- **Decision:** Target `net10.0` for Phase 1 standalone projects.
- **Reason:** This is a new codebase, .NET SDK 10.0.302 is installed in the authoritative development environment, and no legacy target constraint exists yet.
- **Alternatives considered:** `net5.0`, which is also installed but obsolete for a new long-lived foundation; deferring scaffolding further.
- **Consequences:** Future Unity integration must use a compatible boundary or multi-targeting strategy if its supported runtime differs. The Core domain remains free of platform-specific APIs to keep that option open.

### ADR-015 — V1 JSON conventions and executable validation

- **Status:** Accepted
- **Date:** 2026-10-06
- **Decision:** V1 JSON uses camelCase properties, integer schema version `1`, lowercase hyphenated (`D`) GUID output in canonical fixtures, ISO 8601 `DateOnly`/`DateTimeOffset` values, and null for explicitly unknown optional facts. Executable Data-layer validation is the V1 contract; formal JSON Schema files are deferred until they add value beyond it.
- **Reason:** These conventions are sufficient for a small readable fixture and avoid maintaining two validation systems during the first proof.
- **Alternatives considered:** Formal JSON Schema immediately, stringly formatted dates without framework support, or silently defaulted missing values.
- **Consequences:** The serializer/domain boundary remains explicit. A later schema artifact must match the executable rules rather than redefine them.

### ADR-016 — V2 packages expose historical availability, provenance, and external mappings

- **Status:** Accepted
- **Date:** 2026-10-07
- **Decision:** V2 packages use a required manifest with schema/package identity, version, UTC creation time, season coverage, provenance references, and contained categories. Historical profile fields use `Known`, `Unknown`, or `NotApplicable`. Provenance and provider-neutral external mappings remain Data-layer metadata, not Core identity. Validation status is computed rather than persisted. Compatibility is proven by one explicit V1→V2 migration, not a generic migration framework.
- **Reason:** Historical uncertainty, source traceability, and provider independence must be represented before controlled imports, while Core must remain storage- and provider-independent.
- **Alternatives considered:** Provider IDs on domain entities, null-only historical fields, a persisted validation flag, and a generic migration/package platform; rejected as lossy, stale, or premature.
- **Consequences:** V1 remains a frozen migration/regression input. V2 is the current canonical package output. Deterministic imported-ID generation, real-source governance, field-level provenance, package registries, and broader migration orchestration remain deferred.

## Phase 2 decisions

### ADR-017 — Civil league date and season-oriented application session

- **Status:** Accepted
- **Date:** 2026-10-07
- **Decision:** `DateOnly` is the authoritative simulated league date. Game tipoffs remain `DateTimeOffset`; a Game's schedule date is the civil date represented at its stored offset, not its UTC calendar date. Immutable `LeagueWorld` remains starting truth, while a season-oriented `LeagueSession` in the Core-only `NBATheAssociation.Application` assembly owns mutable runtime date and derived schedule indexes. The first session stops at its target Season's inclusive end. Automatic season rollover and explicit `SeasonPhase` records are deferred.
- **Reason:** Day-based league progression must be deterministic and independent from wall-clock/Unity concerns, while tipoffs still need a precise instant and authored local offset. A separate application layer prevents mutable workflow state from contaminating Core facts or Data persistence.
- **Alternatives considered:** Use `DateTimeOffset` as the league clock, assign games by UTC date, mutate `LeagueWorld`, place workflows in Data/Core, automatically roll into later seasons, or introduce preseason/regular/postseason/offseason records immediately. These were rejected as ambiguous, incorrectly coupled, or premature for the first calendar slice.
- **Consequences:** V1 does not recalculate venue time zones or daylight-saving rules; authored tipoff offsets are authoritative. Sessions can start before opening but cannot advance beyond Season end. Future offseason/rollover work must explicitly extend this boundary rather than relying on implicit transitions.
