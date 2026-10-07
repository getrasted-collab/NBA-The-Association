# Roadmap

## Phase 0 — Foundation

Vision, architecture boundaries, data principles, roadmap, questions, decisions, and backlog. Complete.

## Phase 1 — Data Foundation (complete for current needs)

- Slice 01: typed identity, minimal league/season/franchise/team/player models, JSON envelope, validation, synthetic two-season fixture, and NUnit tests. **Implemented 2026-10-06.**
- Slice 02: explicit historical values, V2 manifests/provenance/external mappings, cross-record validation, synthetic import pipeline, explicit V1→V2 migration, cross-era fixtures, inspection/diff services, and a small CLI. **Implemented 2026-10-07.**
- Further import expansion is intentionally paused. Deterministic imported IDs and data-rights policy remain prerequisites before real-data work.

## Phase 2 — League, Calendar, and Schedule

Configurable seasons, participants, dates, schedule constraints/import, and advancement.

- Slice 01 planning: authoritative league date, immutable starting world versus mutable session, season participation, indexed schedule queries, deterministic advancement, boundaries, and structural schedule validation. **Planned 2026-10-07; awaiting approval.**

## Phase 3 — Player, Team, Roster, and Lineup Models

Expand availability, roster rules, lineups, and the minimum approved attribute/tendency vocabulary.

## Phase 4 — Basic Headless Basketball Simulation

Deterministic game state and minimal action/event vocabulary; no Unity dependency.

## Phase 5 — Statistics and Standings

Authoritative accounting, aggregation, standings, and reconciliation.

## Phase 6 — Save/Load and Compatibility

Hybrid/self-contained saves, atomic writes, recovery, migrations, and round-trip continuation.

## Phase 7 — First Basic Unity UI

Initialize Unity only after explicit approval and a working headless vertical slice.

## Phases 8–18

Contracts/transactions; draft/free agency; development/injuries; advanced basketball simulation; organizational AI; historical content breadth; advanced Default Mode; GM Mode; Player Mode; 3D broadcast presentation; long-term calibration, optimization, accessibility, and polish.
