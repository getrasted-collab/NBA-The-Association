# Architecture

## Dependency direction

```text
Future Unity / CLI / tools
          ↓
Application workflows
          ↓
Domain core
          ↑
Data and infrastructure adapters
```

The domain core must not reference Unity, UI, scene lifecycle, JSON, filesystems, or a database. Data/infrastructure maps external representations into validated domain state. Presentation issues commands and reads query models; it never owns league truth.

## Initial boundaries

- `NBATheAssociation.Core` — typed identities and domain records/invariants
- `NBATheAssociation.Data` — JSON DTOs, validation, and materialization
- `NBATheAssociation.Application` — headless runtime workflows and derived schedule queries; references Core only
- `NBATheAssociation.Tests` — NUnit regression and slice tests

Future Simulation, CLI, infrastructure, and Unity projects are created only when their approved slices need them.

## World composition

```text
Immutable versioned package
          ↓ validate/materialize
Independent in-memory world
          ↓ future commands
Versioned self-contained save state
```

Default, GM, and Player modes will operate on the same world. Rules, league structure, participants, and season identity are data-driven and must not assume the modern NBA.

## Execution principles

- Headless execution and automated tests are first-class.
- Explicit seeded randomness will be introduced only when stochastic systems exist.
- Storage DTOs and domain objects remain separate.
- Prefer explicit validators and mappings over reflection-heavy frameworks.
- Avoid microservices, ECS, event sourcing, and generic repository layers without measured need.
