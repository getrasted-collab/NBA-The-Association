# Project Foundation B — Independent Backup and Restore Report

## Outcome

The repository now has a documented, repeatable Git-native backup and restore path independent of GitHub access. The mechanics passed a complete isolated local proof. No data was uploaded or copied to an external provider/device because the owner has not selected one, so the milestone is **partially complete** rather than fully complete.

Unity was not created, Git LFS was not enabled, and gameplay systems were not changed.

## Backup design

- Committed repository history: all-ref Git bundle plus SHA-256 sidecar checksum.
- Future irreplaceable source assets, LFS objects, and controlled/private data: separate encrypted versioned file backup with inventory and restore checks.
- Generated caches, package caches, intermediate exports, ordinary builds, test output, and disposable saves: excluded.
- GitHub remains the primary collaboration remote; the independent channel is disaster recovery, not a Git replacement.

`docs/BACKUP_STRATEGY.md` defines classification, destination requirements, encryption, frequency, retention, LFS/data handling, restoration, and verification. `tools/Backup-Repository.ps1` accepts an explicit destination, requires a clean repository, rejects filesystem-root and repository-internal destinations, avoids overwrite, verifies the bundle, and writes its checksum.

The root `.gitignore` excludes local `*.bundle` and `*.bundle.sha256` artifacts so disaster-recovery archives are not accidentally committed back into the repository they protect.

## Temporary restore proof

- Date: 2026-10-07
- Commit tested: `ca17a7fd69b9e7bdfba2e74f8aefafb84a542ff9`
- Method: created a bundle beneath a unique Windows temporary directory, verified its checksum and Git completeness, cloned it into a second temporary directory, validated the restored project, then removed the temporary proof data.
- Bundle refs: `main`, `origin/main`, `HEAD`, and reachable local refs were present; Git reported complete history.
- Restored `HEAD`: exact match.
- Restored tree before and after validation: clean.
- Restore: successful for all five projects.
- Build: successful with 0 warnings and 0 errors.
- Tests: **33 passed, 0 failed, 0 skipped**.
- CLI: V2 synthetic package validation returned `VALID` with exit code 0.
- Project list: Core, Application, Data, CLI, and Tests all present.
- Dependency check: Application referenced Core only.
- Hidden working-copy dependencies found: none.

The first execution exposed a PowerShell precedence defect in the overwrite guard before creating a bundle. It was fixed in commit `ca17a7f`, pushed, and the proof was restarted successfully from that exact revision.

## Not yet protected

- No permanent copy currently exists on an independent external device, NAS, cloud provider, or second private remote.
- Future LFS objects and source assets do not exist and therefore are not backed up.
- Controlled/private real-world data is not present; its authorized encrypted store remains future work.
- The bundle is not self-encrypting; the permanent destination or container must supply encryption.
- Uncommitted and ignored work is intentionally absent from Git bundles.

## Owner decision required

Select an independent off-machine destination and encryption method, identify who controls credentials/recovery keys, and approve copying the repository bundle there. Practical choices and tradeoffs are documented in `BACKUP_STRATEGY.md`. No external transfer occurred in this milestone.

## Foundation completion estimates

| Area | Completion | Remaining work |
|---|---:|---|
| Project Initialization | 15% | Unity/product/platform decisions and eventual host remain deliberately deferred. |
| GitHub / Version Control | 80% | Off-machine backup deployment, future CI/protection, actual LFS activation, and release operations remain. |
| Folder Architecture | 60% | Future Unity, source-asset, data, and build directories remain need-driven. |
| Foundation B backup milestone | 80% | Policy, tool, and local proof are complete; permanent independent destination and retrieval proof remain. |

## Recommended next foundation milestone

The immediate operational step is owner selection of an encrypted off-machine destination, followed by one real bundle upload/copy and an isolated retrieval/restore test. If that decision is intentionally deferred, the next planning-only foundation work is to resolve the pre-Unity product and platform decisions; Unity should still not be created automatically.
