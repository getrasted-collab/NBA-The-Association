# Git and GitHub Workflow

## Branch model

`main` is the permanent integration branch. The project does not use a `develop` branch because it is currently primarily solo-developed and has no parallel release train. Reconsider that only if contributor or release concurrency creates a demonstrated need.

Use short-lived topic branches when isolation or review materially reduces risk:

```text
feature/<short-kebab-name>
fix/<short-kebab-name>
refactor/<short-kebab-name>
data/<short-kebab-name>
docs/<short-kebab-name>
```

Delete topic branches after merge. Do not create empty branches for hypothetical future work.

### Direct commits to main

For solo work, direct commits to `main` are acceptable for small, low-risk documentation updates, tests, and tightly scoped fixes after proportionate verification. Every completed repository change must be committed and pushed; failures must be reported clearly.

Prefer a topic branch and pull request for:

- large refactors;
- persistent schema changes or migrations;
- simulation-engine changes;
- save compatibility changes;
- broad dependency upgrades;
- large binary/source-asset additions;
- release preparation;
- outside contributions;
- any change where side-by-side review materially reduces risk.

Branch protection is not enabled yet. Reconsider it with CI or additional contributors rather than adding ceremony before enforceable checks exist.

## Commits

Use a simple descriptive prefix:

```text
feat: add a user-facing capability
fix: correct behavior
test: add or repair verification
docs: change documentation only
refactor: change structure without intended behavior change
chore: update tooling or repository maintenance
data: add or change an approved dataset or fixture
```

Guidance:

- Keep commits small and logically coherent.
- Use an imperative, specific subject.
- Include related tests and architecture documentation with meaningful behavior/schema changes.
- Identify source and target schema versions in migration commits.
- State source, authorization/license, size, and LFS decision for large assets.
- Avoid unrelated formatting or file moves in feature commits.
- When replacing obsolete code, verify references/tests and remove the superseded system rather than leaving broken duplicates indefinitely.

Existing historical commit messages do not need rewriting.

## Pull requests and merges

A pull request should summarize:

- purpose and scope exclusions;
- verification performed;
- schema/data/save compatibility impact;
- dependencies and asset/license impact;
- risks and deferred follow-up.

Use draft PRs for long-running risky work. A PR is not required for every tiny solo commit.

Prefer squash merge when a topic branch contains noisy intermediate commits. Preserve a small intentional commit series when migration or review history is valuable. Avoid Git Flow merge complexity without a demonstrated release need. Once CI exists, required checks should include the full build and test suite.

## Future releases and tags

There is no production release process yet. When distributable builds exist, use SemVer-compatible versions:

- `0.y.z` for pre-release development;
- `1.0.0` only when a public compatibility commitment is justified.

Suggested annotated tags:

```text
checkpoint/YYYY-MM-DD-name   optional engineering milestone; use sparingly
v0.1.0-internal.1            internal playable build
v0.2.0-alpha.1               alpha
v0.5.0-beta.1                beta
v1.0.0                       release
```

Create tags only from tested commits. GitHub Releases should carry release notes and deliberate build artifacts. Package/save schema versions remain separate from game versions.

## Git LFS and large assets

Git LFS is installed in the current environment but is **not enabled for this repository**. No `.gitattributes` patterns should be added until a real qualifying asset is reviewed.

### Normal Git

Use normal Git for:

- C#, text configuration, Markdown, and scripts;
- small JSON data and deterministic test fixtures;
- Unity `.meta` files;
- small optimized images/audio whose size and history are reasonable;
- manifests and export tooling needed to reproduce assets.

### Potential Git LFS candidates

Review actual size, change frequency, quota, and rights before tracking:

- `.blend` source files;
- large `.psd` or `.psb` layered artwork;
- large FBX/model binaries;
- large lossless textures;
- large WAV/FLAC or other source audio;
- video and cinematic assets;
- other large, non-diffable binary sources that must be versioned.

Do not automatically track every file of one extension. Add narrow `.gitattributes` rules before the first qualifying asset enters history, then verify an isolated clone and LFS pull.

### External asset storage

Keep these outside Git/Git LFS in an authorized asset vault:

- purchased or licensed sources that cannot be redistributed through the repository;
- unauthorized NBA imagery, logos, likeness assets, fonts, audio, or footage;
- restricted/private raw datasets;
- huge working references, intermediate renders, and caches;
- assets exceeding practical LFS quota or retention requirements.

Document how authorized contributors restore required external assets without committing credentials or secret download links.

### Generated or downloaded assets

Caches, package downloads, temporary exports, development builds, test results, and reproducible derivatives remain untracked. Commit a generated/exported asset only when Unity actually requires it as a reviewed source input and it cannot be reproduced reliably or economically.

## Never-commit policy

Never commit:

- API keys, access tokens, passwords, credentials, signing keys, certificates, or local secrets;
- populated `.env` files or secret-store exports;
- machine-specific editor/IDE state and temporary caches;
- Unity `Library/`, `Temp/`, `Logs/`, `UserSettings/`, package caches, memory captures, or recordings;
- temporary Blender/image/audio exports and autosaves;
- unauthorized or restricted copyrighted assets;
- restricted raw datasets or private personal data;
- generated builds unless intentionally distributed as a release artifact outside ordinary source history;
- local saves, test results, coverage files, crash dumps, or profiler captures unless a small reviewed fixture is explicitly required.

If a secret is exposed, immediately revoke/rotate it and assess history exposure. Merely deleting the current file is not remediation.

## Unity source-control rules

When Unity exists, commit:

- approved project assets under `Assets/`;
- the matching `.meta` file for every committed asset and folder;
- `Packages/manifest.json` and `Packages/packages-lock.json`;
- reviewed `ProjectSettings/`.

Never ignore all `*.meta` files. Unity GUID references depend on them.

Do not commit generated Unity caches or local builds. Review `.gitignore` against the actual Unity version/layout during bootstrap instead of copying an unexamined template.

## Backup and recovery

GitHub is the primary source-control remote, not a complete backup system. Irreplaceable source assets, future LFS objects, controlled data, and releases require an independent backup destination and tested restoration policy.

Follow [REPOSITORY_RECOVERY.md](REPOSITORY_RECOVERY.md) for the safe isolated recovery test. Never delete the only working copy to prove recoverability.

Follow [BACKUP_STRATEGY.md](BACKUP_STRATEGY.md) for the independent Git-bundle format, retention, restore verification, and the separate future treatment of LFS/source assets. No permanent destination has been selected, and no project data may be uploaded to a third-party backup provider without owner approval.
