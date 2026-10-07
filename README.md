# NBA The Association

NBA The Association is a long-term, headless-first basketball league, franchise, and career simulation project. The current repository contains the standalone C#/.NET domain, data, league-calendar workflow, validation, migration, inspection, and automated-test foundations. It does not yet contain Unity, basketball game simulation, saves, statistics, standings, or real NBA data.

## Current status

Completed implementation slices:

- Data Foundation 01 and 02
- League, Calendar, and Schedule 01
- 33 automated NUnit tests

Unity is intentionally deferred. A future Unity project will host presentation and input around the standalone world rather than own league or simulation truth.

## Architecture

```text
NBATheAssociation.Core
          ↑
NBATheAssociation.Application

NBATheAssociation.Data ── validates/materializes ──> Core LeagueWorld

future NBATheAssociation.Simulation
          ↑
future Unity host / presentation
```

- **Core** — typed identities and immutable domain facts.
- **Application** — headless league/calendar workflows; references Core only.
- **Data** — JSON packages, validation, migration, provenance, and materialization.
- **CLI** — package validation, inspection, external-ID tracing, and diff tools.
- **Future Simulation** — basketball behavior; not created yet.
- **Future Unity host** — presentation, scenes, UI, input, audio, and 3D; not created yet.

## Repository layout

```text
src/       standalone production projects
tests/     NUnit tests and fictional fixtures
tools/     development tools and CLI
docs/      permanent architecture, decisions, reports, and operating guidance
plans/     approved and historical implementation plans
```

## Prerequisites

- Git
- .NET SDK **10.0.x** (the repository was last verified with SDK 10.0.302)

Check installed versions:

```powershell
git --version
dotnet --version
```

## Restore, build, and test

Run from the repository root:

```powershell
dotnet restore NBATheAssociation.slnx
dotnet build NBATheAssociation.slnx --no-restore
dotnet test NBATheAssociation.slnx --no-build --no-restore
```

The documented baseline is **33 passing tests** with no failures or skips.

## CLI

Validate a committed synthetic V2 package:

```powershell
dotnet run --project tools/NBATheAssociation.Cli --no-restore -- validate tests/NBATheAssociation.Tests/Fixtures/v2-cross-era.json
```

Other commands:

```text
inspect <package>
trace-external <package> <source> <entityType> <externalId>
diff <leftPackage> <rightPackage>
```

Example inspection:

```powershell
dotnet run --project tools/NBATheAssociation.Cli --no-restore -- inspect tests/NBATheAssociation.Tests/Fixtures/league-calendar-short.json
```

## Project guidance

- Start with [AGENTS.md](AGENTS.md) for repository rules.
- See [Architecture](docs/ARCHITECTURE.md), [Decision Log](docs/DECISIONS.md), and [Roadmap](docs/ROADMAP.md).
- Follow [Git Workflow](docs/GIT_WORKFLOW.md) for branches, commits, assets, and never-commit rules.
- Use [Repository Recovery](docs/REPOSITORY_RECOVERY.md) for an isolated reconstruction test.
- Use [Independent Backup Strategy](docs/BACKUP_STRATEGY.md) for Git-native backups and future source-asset protection.

Do not introduce Unity, real NBA data, or a new gameplay slice without an explicitly approved plan.
