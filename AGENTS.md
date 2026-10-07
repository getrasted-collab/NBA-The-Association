# NBA The Association — Repository Instructions

## Project

NBA The Association is a long-term NBA league, franchise, career, and basketball simulation built with a standalone C#/.NET core and a future Unity host. Default, GM, and Player modes share one world.

## Working rules

- Read relevant files in `docs/` and `plans/` before major work.
- Inspect existing code and tests before editing.
- Implement only the approved slice; do not expand into later systems.
- Keep Core independent of Unity, JSON, storage, and UI concerns.
- Prefer small explicit types over global state, generic manager classes, and speculative abstractions.
- Use stable typed IDs; never use names or external-provider IDs as identity.
- Keep immutable starting data separate from runtime/save state.
- Do not hard-code modern league structure or silently substitute modern defaults for unknown historical data.
- Add and run proportionate automated tests. Preserve deterministic, headless execution.
- Record meaningful architecture changes in `docs/DECISIONS.md`; update questions, roadmap, backlog, and implementation reports.
- After completing and verifying repository changes, commit them and push the current branch to the configured GitHub remote. Report any push failure clearly.
- Do not rewrite working systems without a documented reason.
- Do not initialize Unity or implement basketball simulation unless explicitly authorized.

## Current scope

Data Foundation Slices 01 and 02 are complete. Phase 2 League/Calendar/Schedule Slice 01 is being planned in `plans/LEAGUE_CALENDAR_IMPLEMENTATION_01.md`; do not implement it until explicitly approved. Basketball simulation, contracts, transactions, drafts, injuries, statistics, saves, AI, game modes, UI, Unity, and real NBA imports remain out of scope.
