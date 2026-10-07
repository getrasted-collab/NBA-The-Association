# Data Architecture

## Data layers

1. **Source/base:** immutable imported or authored material with provenance.
2. **Starting package:** validated, versioned canonical data for a new world.
3. **Runtime world:** authoritative in-memory domain state, independent from source objects.
4. **Save state:** future hybrid/self-contained persistence sufficient to reconstruct a world.
5. **Generated future data:** save-owned entities and facts that never mutate source packages.

## Identity

Durable entities use strongly typed GUID-backed IDs serialized as canonical lowercase hyphenated strings. Names, abbreviations, jersey numbers, row positions, and provider IDs are never primary keys. External IDs use separate provider-neutral mapping records.

`Franchise` is enduring organizational history. `TeamSeason` is the franchise's identity and participation in one season. Ordinary relocation/rebranding changes TeamSeason data while retaining FranchiseId.

Player identity is similarly stable while `PlayerSeasonProfile` and `RosterMembership` carry season/time-specific facts.

## Formats and compatibility

JSON is the V1 authoring/interchange format. Domain types remain independent from serialization. Every persistent package/save declares a schema version. Unsupported newer versions fail clearly; migrations are explicit and ordered once multiple versions exist.

Historical unknowns remain unknown. Defaults require an explicit documented transformation. Real-world imports eventually retain source, source ID, retrieval/import version, transformations, and usage/license metadata.

## Validation

Validation is structured and locatable. It covers parsing/schema recognition, duplicate identities, typed references, temporal consistency, domain constraints, and later accounting/package integrity. Phase 1 uses only synthetic fictional data.

