# Independent Backup Strategy

## Purpose and scope

GitHub remains the primary collaboration remote, but it is not the only recovery path. This strategy protects NBA The Association against repository or account loss, damaged Git history, loss of a working computer, and future irreplaceable project assets. It does not replace normal Git commits, GitHub, or the separate fresh-clone recovery procedure.

The current repository is small and text-oriented. The approved V1 mechanism is therefore:

1. a verified Git bundle containing all committed refs; and
2. a separate, encrypted file-backup channel when non-Git source assets, LFS objects, or controlled data actually exist.

A Git bundle does not include uncommitted or ignored files. Work must be committed before it is protected by the repository backup.

## Backup classification

### A. Reconstructable from GitHub

These are currently committed and recoverable from the primary remote:

- complete reachable Git history, branches, and future tags;
- standalone source code and project files;
- documentation and plans;
- development tools;
- NUnit tests and synthetic fixtures;
- small, authorized data packages committed as project inputs.

They are still included in the independent Git-native backup so GitHub is not a single point of failure.

### B. Must also exist in an independent backup

- the complete Git repository history, branches, and tags;
- future irreplaceable Blender, Photoshop/image, audio, and other source masters;
- committed Unity source assets and their `.meta` files once Unity exists;
- future Git LFS objects required to reconstruct a checkout;
- authorized controlled/private project data that cannot live in the public repository;
- release manifests, checksums, and deliberately retained milestone builds when those exist.

### C. Generated and normally not backed up

- `bin/`, `obj/`, Unity `Library/`, `Temp/`, `Logs/`, and `UserSettings/`;
- package caches, IDE caches, test results, coverage output, and profiler captures;
- reproducible builds and intermediate asset exports;
- temporary files and disposable local test saves.

Important user saves may later receive their own save-data backup policy. They are not part of the source-repository backup.

### D. Future external or large assets

- `.blend`, large `.psd`/`.psb`, audio masters, video, large textures, and model sources;
- licensed assets that cannot legally be stored in the public repository;
- restricted real-world datasets and controlled correction sources;
- future LFS content and large release artifacts.

These require an authorized asset inventory, rights metadata where relevant, encrypted versioned storage, and a restore manifest. Generated/downloadable derivatives should be recreated instead of duplicated unless reproduction is impractical.

## Git-native backup format

Use `git bundle create <file> --all` from a clean repository. The bundle is a single portable Git file containing every currently reachable local ref, including branches and tags. Validate it immediately with `git bundle verify` and store a SHA-256 checksum beside it.

The repository provides `tools/Backup-Repository.ps1` to perform those steps. It requires an explicit destination, refuses a dirty repository, refuses repository-internal and filesystem-root destinations, never overwrites an existing bundle, and embeds no credentials or provider paths.

The bundle is not encrypted by itself. Encryption must be supplied by the approved destination, an encrypted volume, or a reviewed encrypted archive/container.

## Destination decision

No permanent independent destination is approved yet, so project data must not be copied off this machine until the owner chooses one.

Practical options:

| Option | Strengths | Tradeoffs |
|---|---|---|
| Encrypted cloud storage | Off-site, accessible after local disaster, often versioned | Provider/account dependency, recurring cost, privacy and recovery-key management |
| External SSD/HDD | Simple, fast, can remain offline | Can fail or be lost; not off-site if stored beside the computer |
| NAS | Convenient automation and local history | Hardware/administration cost; still needs an off-site copy |
| Second private Git remote | Excellent independent Git-history redundancy | Does not automatically protect LFS, controlled data, or external source assets |

The selected destination must be physically or administratively independent of GitHub and the working computer, encrypted at rest, accessible to the project owner, and documented without committing credentials. A two-destination combination such as encrypted cloud plus periodically disconnected external storage is preferred when valuable source assets arrive.

## Frequency and retention

For the current solo repository:

- create an independent Git bundle weekly while active and after every major milestone, migration, or release tag;
- retain the four most recent weekly bundles, six monthly bundles, and milestone/release bundles;
- verify the newest bundle at creation and perform a full isolated restore at least monthly and after changing backup tooling;
- back up future irreplaceable source-asset work after each meaningful work session, with daily/versioned snapshots while active;
- review storage health, credentials/recovery keys, and restoration documentation every six months.

The permanent destination may provide its own versioning, but retention must not depend on an unverified sync folder that can propagate accidental deletion.

## Future Git LFS and controlled data

Git LFS is not enabled. Before enabling it, the project must document how all required LFS objects are fetched, independently copied, inventoried, checksummed, and restored. A Git bundle alone is not an LFS backup.

Controlled/private datasets and licensed assets must use access-controlled encrypted storage separate from the public repository. Backups must preserve source/license metadata and transformation manifests without placing secrets, private download links, or unauthorized material in Git. Recovery tests should use authorized test data or metadata-only proofs when access restrictions apply.

## Restore and verification

Use a new temporary directory, never the authoritative checkout:

1. Verify the bundle and checksum.
2. List bundle heads and confirm expected branches/tags exist.
3. Clone the bundle into a separate restore directory.
4. Confirm the restored `HEAD` equals the recorded source commit and the tree is clean.
5. Run `dotnet restore`, `dotnet build`, and `dotnet test`.
6. Run the CLI validation smoke test against the committed fictional V2 package.
7. Confirm all five projects exist and Application references Core only.
8. Record results, then remove only the verified temporary test directories.

When source assets/LFS/controlled data exist, also restore them from their independent channel, verify checksums and inventory, and prove the documented development/build workflow can consume them.

## Current proof status

The local mechanism was proven on 2026-10-07 against commit `ca17a7fd69b9e7bdfba2e74f8aefafb84a542ff9`. A SHA-256-checked bundle restored the complete repository into a separate temporary directory; the restored checkout built, passed all 33 tests, returned `VALID` from the CLI smoke test, contained all five expected projects, preserved the Core-only Application dependency, and remained clean. Temporary proof data was not retained as a project backup.

This validates bundle creation and reconstruction, but it does **not** make the backup independent of this machine. The permanent off-machine destination remains an owner decision and must be configured and tested before independent backup is fully complete.
