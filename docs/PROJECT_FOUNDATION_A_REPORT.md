# Project Foundation A Report

## Outcome

The standalone repository onboarding and reconstruction milestone is complete. The project can be cloned from GitHub into an isolated directory, restore all dependencies without relying on the normal user NuGet package cache, build every current project, pass the complete test suite, and validate a committed package through the CLI.

Unity was not created, Git LFS was not enabled, and no working project was reorganized.

## Implemented

- Root `README.md` with accurate status, architecture, repository layout, prerequisites, exact restore/build/test commands, CLI examples, and the 33-test baseline.
- `docs/GIT_WORKFLOW.md` covering `main`, topic branches, direct solo commits, PR risk thresholds, commit prefixes, merge guidance, future versions/tags, selective LFS, never-commit rules, Unity `.meta` handling, and backup direction.
- Reviewed `.gitignore` additions for local secrets/signing material, coverage, Unity captures/recordings, crash dumps, and repository-local package caches.
- `docs/REPOSITORY_RECOVERY.md` with safe isolated procedures, failure handling, future Unity extension, and the verified result.
- Updated foundation plan statuses, roadmap, open questions, and backlog using actual verification rather than documentation-only claims.

## Git workflow decisions

- `main` remains the permanent integration branch.
- No `develop` branch is used now.
- Short-lived `feature/`, `fix/`, `refactor/`, `data/`, and `docs/` branches are used when isolation/review adds value.
- Small verified solo changes may go directly to `main`.
- PRs are preferred for large refactors, schema/migrations, simulation, save compatibility, broad dependency upgrades, large binary additions, outside contributions, and releases.
- Commit prefixes are `feat:`, `fix:`, `test:`, `docs:`, `refactor:`, `chore:`, and `data:`.
- Branch protection remains disabled as directed.

## Git LFS and large assets

Git LFS remains disabled and no `.gitattributes` was created. The policy distinguishes normal text/small assets, reviewed large binary LFS candidates, restricted/excessive external assets, and generated/downloaded outputs. Future candidates include `.blend`, large `.psd`/`.psb`, models, textures, audio, and video, but patterns will be added only when an actual file, quota, rights, and clone behavior are reviewed.

## Never-commit policy

The policy explicitly excludes credentials, keys, tokens, passwords, secrets, signing material, machine caches, Unity caches, temporary exports, unauthorized copyrighted assets, restricted datasets, ordinary generated builds, saves, test/coverage results, captures, and dumps. Unity `.meta` files are explicitly not ignored; a committed Unity asset and its matching `.meta` file must both be committed once Unity exists.

## `.gitignore` changes

Existing .NET, IDE, Unity cache, build, save, artifact, and result exclusions were preserved. Narrow additions cover:

- `.env` variants except a sanitized `.env.example`;
- `secrets/`, `.pfx`, and `.p12`;
- coverage outputs;
- Unity `MemoryCaptures/` and `Recordings/`;
- crash dumps;
- repository-local `.nuget/` caches.

No broad asset/data wildcard or Unity `.meta` exclusion was added.

## Recovery test

### Commit tested

`865f6e7d998717f0f1281f9343ff9e759d0f827f`

### Procedure

1. Created a unique directory beneath the Windows user temporary directory, outside the authoritative repository.
2. Cloned `https://github.com/getrasted-collab/NBA-The-Association.git`.
3. Confirmed clone `HEAD` exactly matched the expected commit.
4. Confirmed the fresh clone was clean.
5. Recorded Git 2.55.0.windows.1 and .NET SDK 10.0.302.
6. Restored `NBATheAssociation.slnx`.
7. Built the entire solution.
8. Ran the complete NUnit suite.
9. Ran CLI `validate` against `v2-cross-era.json`.
10. Listed solution projects and Application project references.
11. Confirmed the clone remained clean.
12. Repeated restore with `--force --no-cache` and a newly created isolated `NUGET_PACKAGES` directory, then rebuilt and reran all tests.
13. Recorded results and safely removed only the isolated temporary directory.

### Results

- Clone SHA: exact match
- Fresh and final Git status: clean
- Restore: successful for all projects
- Build: successful, **0 warnings, 0 errors**
- Tests: **33 passed, 0 failed, 0 skipped**
- CLI smoke test: exit code 0, output `VALID`
- Projects present: Core, Application, Data, CLI, Tests
- Application dependencies: **Core only**
- Isolated no-cache NuGet restore: passed; 825 package files populated into the temporary package directory
- Hidden working-copy files required: none
- Large/secret/local files discovered: none

The documented .NET SDK, Git/network access, and public NuGet dependencies remain legitimate environment prerequisites. No Unity, LFS, external asset, private dataset, or local configuration dependency was required.

## What could not be verified

- Unity recovery, because no Unity project exists by design.
- LFS restore, because LFS is not enabled and no qualifying assets exist.
- Independent non-GitHub backup restoration, because the owner has not selected a backup destination/policy.
- Production releases/builds, which do not yet exist.

## Updated completion estimates

| Area | Before | After | Remaining gap |
|---|---:|---:|---|
| Project Initialization | 10% | 15% | Unity/platform/product decisions and host configuration |
| GitHub / Version Control | 40% | 75% | independent backup, later CI/protection, LFS activation when justified, releases |
| Folder Architecture | 55% | 60% | future Unity/source-assets/data/build areas when real content exists |

## Remaining owner decisions

- independent encrypted backup destination, owner, schedule, retention, and restore cadence;
- whether/when CI and `main` protection become worthwhile;
- future GitHub LFS quota and authorized external asset vault;
- all Unity bootstrap decisions already listed in `OPEN_QUESTIONS.md`.

## Recommended next foundation milestone

Select an independent backup destination and policy, then perform and document a non-GitHub backup/restore proof. Unity creation, LFS activation, asset pipeline implementation, and Unity recovery should remain deferred until their explicit prerequisites are approved.
