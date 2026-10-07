# Open Questions

Classifications:

- **BLOCKING** — must be answered before the named upcoming work can safely begin
- **IMPORTANT BUT NON-BLOCKING** — preserve a seam now; answer before its dependent system hardens
- **LATER** — can wait without undermining the current phase

Answered choices should be recorded in `DECISIONS.md`; do not infer answers merely to remove blockers.

## Resolved for Data Foundation

- **Resolved — ADR-006:** C#/.NET standalone core, NUnit, minimal dependencies, and smallest useful project structure.
- **Resolved — ADR-007:** Strongly typed GUID-backed IDs; provider-neutral external mappings; generated save entities may receive new GUIDs.
- **Resolved — ADR-008:** JSON is the V1 authoring/interchange format; domain models remain serialization-independent.
- **Resolved — ADR-009:** Immutable packages materialize in-memory worlds; saves follow a hybrid/self-contained direction without embedded presentation assets.
- **Resolved — ADR-010:** Explicit schema versions, ordered migrations, clear rejection of unsupported newer schemas, and no silent downgrade.
- **Resolved — ADR-011:** Franchise is enduring; TeamSeason owns season-specific identity/participation; ordinary relocation/rebranding preserves the franchise.
- **Resolved — ADR-012:** Historical unknowns remain explicit and provenance is separate from domain identity.
- **Resolved — ADR-013:** Phase 1 uses fictional synthetic data; full NBA import is deferred.
- **Resolved — ADR-016:** V2 package manifests, three-state historical values, package provenance, provider-neutral external mappings, explicit V1→V2 migration, and computed validation status.
- **Resolved by project owner:** `C:\Users\Owner\Documents\ChatGPT\NBA` is authoritative. The location question is closed.

## Architecture

- **Resolved — ADR-014:** Slice 01 targets `net10.0`.
- **Resolved — ADR-017:** Headless league/calendar workflows use a Core-only Application layer; `LeagueWorld` remains immutable and `LeagueSession` owns runtime date.
- **IMPORTANT BUT NON-BLOCKING:** What exact future assembly/package boundary will connect the standalone core to Unity? Needed before Unity work, not before logical modeling.
- **IMPORTANT BUT NON-BLOCKING:** What command/query and domain-event conventions are appropriate without overengineering?
- **IMPORTANT BUT NON-BLOCKING:** What deterministic RNG and stream-partitioning strategy will generated/simulated data use?
- **LATER:** Is ECS or another data-oriented execution model warranted after profiling?

## Identity and import

- **BLOCKING only before deterministic imported IDs are implemented:** Which UUID algorithm/version, namespace UUIDs, canonical source keys, normalization rules, and collision policy produce deterministic imported IDs? ADR-007 requires documentation first.
- **Resolved for V2 — ADR-016:** Source names are provider-neutral strings; entity types use the closed V2 vocabulary implemented by the package validator. A governed real-provider source registry remains non-blocking until real-data work.
- **IMPORTANT BUT NON-BLOCKING:** What uniqueness rules apply if a provider reuses identifiers or supplies multiple historical identifiers?
- **IMPORTANT BUT NON-BLOCKING:** What review workflow handles conflicting mappings, merges, and splits?
- **IMPORTANT BUT NON-BLOCKING:** What confidence/correction taxonomy accompanies real historical provenance?

## Data packages and schemas

- **Resolved — ADR-015:** V1 uses camelCase, schema integer `1`, canonical lowercase hyphenated GUID fixture strings, ISO 8601 dates/date-times, and null for explicitly unknown optional values.
- **Resolved for Slice 01 — ADR-015:** Executable validation is authoritative; formal JSON Schema is deferred until it adds independent value.
- **IMPORTANT BUT NON-BLOCKING:** Which derived indexes/read models are persisted versus rebuilt?
- **IMPORTANT BUT NON-BLOCKING:** Are unknown JSON fields preserved or rejected for each artifact type?
- **LATER:** At what scale should authoring, tooling, or runtime move from files to a database?

## Saves

- **IMPORTANT BUT NON-BLOCKING for Phase 1; BLOCKING before save implementation:** What physical save container and internal serialization are used?
- **IMPORTANT BUT NON-BLOCKING for Phase 1; BLOCKING before save implementation:** What exact starting-world subset is embedded, and how are asset references/fallbacks represented?
- **IMPORTANT BUT NON-BLOCKING for Phase 1; BLOCKING before save implementation:** What atomic-write, backup, integrity, corruption-recovery, and interrupted-migration policy applies?
- **IMPORTANT BUT NON-BLOCKING:** What backward-compatibility window is promised, and are unknown fields preserved?
- **IMPORTANT BUT NON-BLOCKING:** Which diagnostics/replay state is stored and what size targets apply?
- **LATER:** Are cloud, cross-platform, branching saves, and user mods supported?

## Historical structure

- **Resolved for Phase 2 Slice 01 — ADR-017:** The first runtime is season-oriented, stops at the inclusive Season end, and does not introduce SeasonPhase or automatic rollover.
- **IMPORTANT BUT NON-BLOCKING:** What lineage relation vocabulary eventually represents expansion, contraction, reactivation, predecessor/successor, and officially reassigned history? ADR-011 limits V1 to ordinary continuity.
- **IMPORTANT BUT NON-BLOCKING:** Are mid-season city/name/venue changes required, or is one identity per TeamSeason sufficient initially?
- **IMPORTANT BUT NON-BLOCKING:** When a save crosses known historical changes, which occur automatically, optionally, or never?
- **IMPORTANT BUT NON-BLOCKING:** How are incomplete ratings/tendencies inferred and disclosed when an explicit transformation is requested?
- **IMPORTANT BUT NON-BLOCKING:** How are era-relative metrics and statistical-definition changes compared?
- **LATER:** What is the first supported historical range and accuracy bar?

## Attributes and tendencies

- **IMPORTANT BUT NON-BLOCKING:** What separates skill, current ability, potential, role, physical state, and contextual performance? Slice 01 uses only a deliberately small profile.
- **IMPORTANT BUT NON-BLOCKING:** What authoring/runtime scales will ratings use?
- **IMPORTANT BUT NON-BLOCKING:** How are measurements, handedness, athletic traits, skills, processing, and versatility modeled without duplication?
- **IMPORTANT BUT NON-BLOCKING:** Are tendencies preferences, conditional policies, learned distributions, or a combination?
- **IMPORTANT BUT NON-BLOCKING:** How do tendency, scheme, role, lineup, matchup, and context combine?
- **IMPORTANT BUT NON-BLOCKING:** How are tendencies calibrated and edited safely?
- **LATER:** How visible/uncertain are attributes, and how do tendencies evolve?

## Basketball simulation

- **LATER:** What is the first simulation time/event granularity and action vocabulary?
- **LATER:** What is the first spatial abstraction?
- **LATER:** How are decisions separated from physical/skill resolution and coaching intent?
- **IMPORTANT BUT NON-BLOCKING:** Which game invariants and diagnostic trace levels are mandatory? Authoritative results must remain distinct from optional traces.
- **IMPORTANT BUT NON-BLOCKING:** Which seasons and metrics form initial calibration baselines?
- **LATER:** How will 3D presentation represent events without false precision?

## Deferred entity systems

- **IMPORTANT BUT NON-BLOCKING:** What money/currency and cap-accounting representation will contracts use?
- **IMPORTANT BUT NON-BLOCKING:** How are multi-leg transactions modeled atomically?
- **IMPORTANT BUT NON-BLOCKING:** How are conditional/protected draft assets represented?
- **IMPORTANT BUT NON-BLOCKING:** What injury and availability vocabularies are required?
- **IMPORTANT BUT NON-BLOCKING:** How are award definitions, renames, voting, and shared results represented?
- **LATER:** Which staff roles and employment structures are needed first?

These do not block Slice 01 because those entities are excluded.

## Data rights and provenance

- **IMPORTANT BUT NON-BLOCKING for synthetic Phase 1; BLOCKING before real-data work:** Which NBA/player/team/arena/branding data and assets may be stored, committed, distributed, and shipped?
- **IMPORTANT BUT NON-BLOCKING:** What minimum license/usage metadata is mandatory when status is unknown?
- **IMPORTANT BUT NON-BLOCKING:** What retention policy applies to raw source records and corrections?

## AI and game modes

- **IMPORTANT BUT NON-BLOCKING:** How are direction, ownership priorities, staff beliefs, uncertainty, and risk represented without becoming immutable franchise identity?
- **IMPORTANT BUT NON-BLOCKING:** Can a save change perspective or control multiple actors? Save ownership must not assume exactly one controlled team.
- **IMPORTANT BUT NON-BLOCKING:** What information, automation, and permissions differ by mode?
- **IMPORTANT BUT NON-BLOCKING:** Is multiplayer/shared control a goal affecting future determinism/API design?
- **LATER:** AI techniques, exploit prevention, explanations, and pre-NBA Player Mode scope.

## Unity, UI, and presentation

- **BLOCKING before Unity creation:** Which exact Unity LTS editor revision is approved after a current compatibility/package review?
- **BLOCKING before Unity creation:** What are the initial target platforms and minimum hardware/OS direction?
- **BLOCKING before Unity creation:** What company/studio name, product display name, and reverse-domain application identifier should Unity use?
- **BLOCKING before Unity creation:** Which rendering pipeline best balances the intended visual ceiling with approved platform breadth—URP or HDRP?
- **BLOCKING before Unity creation:** Which input devices must be first-class at bootstrap, and is Unity Input System approved?
- **BLOCKING before Unity creation:** How will the `net10.0` standalone projects expose a Unity-compatible assembly/API boundary without duplicating domain logic?
- **IMPORTANT BUT NON-BLOCKING:** What frame-rate, VSync, window-mode, resolution, and aspect-ratio targets apply to the primary platform?
- **IMPORTANT BUT NON-BLOCKING:** Which languages/localization package and key-authoring workflow are required for the first playable?
- **IMPORTANT BUT NON-BLOCKING:** Which completed headless milestone authorizes Unity bootstrap?
- **LATER:** UI binding/read models, navigation, accessibility, localization, and modding.
- **IMPORTANT BUT NON-BLOCKING:** Provenance, uncertainty, historical context, and causality must remain queryable.
- **LATER:** 3D assets, event visualization, visual/audio identity, and performance budgets.

## Repository operations

- **Resolved for the standalone foundation:** `main` plus risk-based short-lived topic branches, simple commit prefixes, selective future LFS, never-commit rules, and isolated GitHub recovery are documented and verified.
- **Resolved for local backup mechanics — ADR-018:** Use a verified all-ref Git bundle plus SHA-256 checksum for committed repository history; future source assets/LFS/controlled data use a separate encrypted versioned channel.
- **IMPORTANT BUT NON-BLOCKING:** Should `main` branch protection wait for CI/contributors as recommended, or be enabled earlier?
- **BLOCKING for completion of Foundation B, non-blocking for current code:** Which independent off-machine destination and encryption method should be used, and who owns its credentials/recovery key? Recommended cadence and retention are defined in `BACKUP_STRATEGY.md`.
- **IMPORTANT BUT NON-BLOCKING until large assets arrive:** What GitHub LFS quota/budget and authorized external asset vault are available?
- **LATER:** When does contributor/release concurrency justify a long-lived integration branch? Current recommendation is no `develop` branch.

## League calendar and time

- **Resolved — ADR-017:** The authoritative league date is `DateOnly`; Game tipoffs remain `DateTimeOffset`; schedule date uses the civil date at the stored offset.
- **IMPORTANT BUT NON-BLOCKING:** When explicit competition phases are introduced, which cross-era phase vocabulary and boundary rules are required?
- **IMPORTANT BUT NON-BLOCKING:** What offseason/rollover command creates or selects the next Season without assuming modern NBA workflows?
- **LATER:** Do venue time-zone identifiers and historical daylight-saving rules need to supplement authored numeric offsets?
- **LATER:** How are postponements, cancellations, and rescheduled tipoffs represented once Game state becomes mutable?

## Remaining blockers after Phase 2 Implementation Slice 01

None for the completed synthetic league/calendar slice. Deterministic imported-ID generation and real-data rights remain blocking only before real-data import work. Season phases and rollover require decisions before their own future slices, not before using the current session. The next slice must be planned and approved before coding.
