# Project Foundation / Development Setup Plan

## Scope and status language

This plan covers only:

1. Project Initialization
2. GitHub / Version Control
3. Folder Architecture

It does not authorize Unity creation, package installation, repository reorganization, Git LFS activation, real-data import, or production builds.

Status meanings:

- **DONE** — the current repository/project actually satisfies the item.
- **PARTIALLY DONE** — working implementation exists, but the item is not complete.
- **READY TO DO NOW** — prerequisites are known and the work is safe before Unity.
- **BLOCKED BY DECISION** — the owner must make or approve a product/technical choice first.
- **DEFER UNTIL UNITY** — perform while creating/configuring the Unity host.
- **DEFER UNTIL LATER** — deliberately unnecessary for the present foundation.

Percentages are completion estimates for the named setup area, not game-development progress.

## 1. Current-state audit

### Repository facts observed on 2026-10-07

- Public GitHub repository: `getrasted-collab/NBA-The-Association`.
- Remote URL: `https://github.com/getrasted-collab/NBA-The-Association.git`.
- GitHub default branch: `main`.
- Local `main` tracks `origin/main` and was clean at audit time.
- No `develop`, feature, release, or other remote branch exists.
- No tags exist.
- GitHub reports `main` is not branch-protected.
- Git LFS 3.7.1 is installed locally, but no patterns are tracked and no `.gitattributes` exists.
- A local Git author identity is configured; no repository-local pull-rebase, merge-fast-forward, or line-ending override was observed, so behavior currently falls back to broader Git defaults.
- A root `README.md` documents prerequisites, architecture, layout, restore/build/test, and CLI commands.
- The reviewed `.gitignore` excludes .NET output, common IDE state, Unity caches, build output, saves, artifacts, results, local secrets/signing material, coverage, captures, and local package caches without ignoring Unity `.meta` files.
- The standalone recovery procedure passed against GitHub commit `865f6e7d998717f0f1281f9343ff9e759d0f827f`, including an isolated no-cache NuGet restore.
- No Unity project exists, by prior architectural decision.

### Working standalone architecture

```text
src/
  NBATheAssociation.Core/
  NBATheAssociation.Application/
  NBATheAssociation.Data/
tests/
  NBATheAssociation.Tests/
tools/
  NBATheAssociation.Cli/
docs/
plans/
NBATheAssociation.slnx
```

The solution targets `net10.0`. Core owns immutable domain truth, Application owns headless workflows, Data owns packages/serialization/validation, CLI owns package inspection, and NUnit tests cover the implemented slices.

Completed technical slices:

- Data Foundation Slice 01
- Data Foundation Slice 02
- League/Calendar/Schedule Slice 01
- 33 passing automated tests at the most recent implementation report

This architecture must be preserved. Unity will be a consumer/host, not a replacement.

### Completion summary

| Area | Estimated completion | Meaning |
|---|---:|---|
| Project Initialization | 15% | Standalone product, architecture, onboarding, and reproducible setup exist; Unity-specific product/platform decisions and host do not. |
| GitHub / Version Control | 75% | Repository, main, workflow, policies, ignore rules, and isolated recovery are proven; independent backup, CI/protection, LFS activation, and releases remain. |
| Folder Architecture | 60% | Standalone layout and onboarding are sound; future Unity, source-assets, data, and build areas remain deliberately uncreated. |

## 2. Checklist status

### Project Initialization

| Item | Status | Evidence / next condition |
|---|---|---|
| Establish standalone C#/.NET foundation | **DONE** | Core, Application, Data, CLI, tests, solution, and reports exist. |
| Create Unity project | **DEFER UNTIL UNITY** | Roadmap intentionally waits for a sufficiently developed headless vertical slice. |
| Choose Unity version | **BLOCKED BY DECISION** | Select a supported LTS at bootstrap time after compatibility and package review. |
| Project name | **PARTIALLY DONE** | Product is “NBA The Association”; exact Unity project/folder display spelling should be approved. |
| Company name | **BLOCKED BY DECISION** | Legal/studio identity has not been supplied. Do not invent it. |
| Product name | **PARTIALLY DONE** | Working product name exists; final Unity Player Settings value needs approval. |
| Application identifier/package name | **BLOCKED BY DECISION** | Requires approved reverse-domain/company namespace and platform targets. |
| Target platforms | **BLOCKED BY DECISION** | No Windows/macOS/Linux/console target commitment has been approved. |
| Minimum hardware/OS direction | **BLOCKED BY DECISION** | Depends on target platforms, presentation scope, and performance research. |
| Resolution strategy | **DEFER UNTIL UNITY** | Define after primary platforms and UI technology are selected. |
| Aspect-ratio strategy | **DEFER UNTIL UNITY** | Plan for adaptive layouts, then verify against approved platform ratios. |
| Windowed/fullscreen strategy | **DEFER UNTIL UNITY** | Desktop behavior depends on platform decision. |
| Input system | **BLOCKED BY DECISION** | Recommend Unity Input System at bootstrap; confirm supported devices/platforms first. |
| Rendering pipeline | **BLOCKED BY DECISION** | URP versus HDRP is a consequential platform/performance choice. |
| Graphics APIs | **DEFER UNTIL UNITY** | Configure per target platform after pipeline/hardware choice. |
| Quality tiers | **DEFER UNTIL UNITY** | Define baseline tiers after representative scenes and performance budgets exist. |
| Frame-rate strategy | **BLOCKED BY DECISION** | Requires presentation/platform goals; do not assume 30/60/uncapped. |
| VSync strategy | **DEFER UNTIL UNITY** | Configure alongside frame pacing and display modes. |
| Localization strategy | **BLOCKED BY DECISION** | Architecture should be localization-ready, but launch languages and Unity package choice are unapproved. |
| Build settings | **DEFER UNTIL UNITY** | Scenes/platform configuration does not exist yet. |
| Development build settings | **DEFER UNTIL UNITY** | Establish after Unity project and first boot scene. |
| Scripting/backend decisions | **BLOCKED BY DECISION** | Unity compatibility with standalone assemblies and target backend must be proven; IL2CPP/Mono is platform-dependent. |
| Project-wide Unity conventions | **READY TO DO NOW** | This document defines boundaries and preliminary naming; a bootstrap checklist can later turn them into settings. |

### GitHub / Version Control

| Item | Status | Evidence / next condition |
|---|---|---|
| GitHub repository | **DONE** | Public repository exists and is connected. |
| Default branch | **DONE** | GitHub and local repository use `main`. |
| Main branch history/push | **DONE** | Implemented slices are committed and pushed. |
| `.gitignore` | **DONE** | Reviewed for the current standalone scope with future Unity caches retained; review again against the actual Unity project. |
| Git LFS installation | **DONE** | Installed locally. This is not the same as enabling it for the repository. |
| Git LFS policy | **DONE** | Selective normal-Git/LFS/external/generated policy is documented; activation waits for first qualifying asset. |
| Git LFS repository configuration | **DEFER UNTIL LATER** | No qualifying tracked binary assets exist. Do not add speculative patterns yet. |
| Branch strategy | **DONE** | `main` plus short-lived topic branches; no `develop` branch for now. |
| Development branch | **DEFER UNTIL LATER** | Not justified for a primarily solo project with no concurrent release train. |
| Feature/fix/data branch conventions | **DONE** | Conventions are documented; branches are created only for actual work. |
| Commit conventions | **DONE** | Simple type prefixes and logical-commit guidance are documented. |
| Pull-request conventions | **DONE** | Risk-based solo/contributor policy is documented. |
| Merge strategy | **DONE** | Squash versus preserved-history guidance is documented. |
| Main branch protection | **PARTIALLY DONE** | No protection currently exists; decide when contributors/CI justify enforcement. |
| Release/tagging strategy | **DONE** | Future policy is documented; no current release is implied. |
| Version-number strategy | **DONE** | Future SemVer-compatible stages are documented. |
| Backup strategy | **PARTIALLY DONE** | GitHub is one remote copy; independent backup and asset/data backup procedures do not exist. |
| Recovery procedure | **DONE** | Safe isolated procedure and failure handling are documented. |
| Fresh-clone standalone recovery test | **DONE** | GitHub clone, isolated restore, build, 33 tests, CLI, references, and clean state passed. |
| Fresh-clone Unity recovery test | **DEFER UNTIL UNITY** | Requires Unity project/version/packages/assets. |
| Never-commit policy | **DONE** | Explicit policy is documented in `GIT_WORKFLOW.md`. |
| Large-asset policy | **DONE** | Source/export/LFS/external/generated decision rules are documented. |
| Generated-file policy | **DONE** | Generated outputs remain ignored unless deliberately released outside normal source history. |
| Secrets policy | **DONE** | Never-commit and incident-response guidance is documented; ignore rules cover local secret files. |

### Folder Architecture

| Item | Status | Evidence / next condition |
|---|---|---|
| `src/` standalone projects | **DONE** | Working Core, Application, and Data projects are correctly separated. |
| `tests/` | **DONE** | NUnit project and deterministic fixtures exist. |
| `tools/` | **DONE** | CLI project exists. |
| `docs/` | **DONE** | Architecture, decisions, questions, roadmap, and reports exist. |
| `plans/` | **DONE** | Approved and historical implementation plans exist. |
| Root solution | **DONE** | `NBATheAssociation.slnx` builds all current projects. |
| Root README | **DONE** | Prerequisites, architecture, layout, exact commands, CLI, and baseline are documented and recovery-tested. |
| Future Simulation project location | **READY TO DO NOW** | Reserved conceptually under `src/`; do not create until simulation is approved. |
| `data/` | **DEFER UNTIL LATER** | Create only when canonical non-test packages or controlled authoring data exist. |
| `source-assets/` | **DEFER UNTIL UNITY** | Create with the first approved source-art workflow/assets, not empty taxonomy. |
| `unity/NBATheAssociation/` | **DEFER UNTIL UNITY** | Future host location; do not create now. |
| `builds/` | **DEFER UNTIL UNITY** | Output remains ignored; releases should generally use GitHub Release artifacts, not Git history. |
| Unity `Assets/` hierarchy | **DEFER UNTIL UNITY** | Design is defined below; create only used folders. |
| Third-party asset boundary | **DEFER UNTIL UNITY** | Use `Assets/ThirdParty/<ProviderOrPackage>` when an approved asset actually exists. |
| Source versus Unity-ready export separation | **READY TO DO NOW** | Policy is defined; physical folders wait for assets. |

## 3. Project Initialization plan

### Architectural invariant

```text
NBATheAssociation.Core
          ↑
NBATheAssociation.Application
          ↑
future NBATheAssociation.Simulation
          ↑
future Unity host / presentation adapters
```

Data is an infrastructure boundary that materializes/validates Core worlds. A future composition root may reference Data, Application, Simulation, and Unity adapters, but no lower standalone assembly references Unity.

Unity must never become authoritative for league state, identity, schedules, simulation, contracts, AI, statistics, rules, or save-world truth. MonoBehaviours observe/query application state and issue explicit commands. Scene objects are disposable presentation state.

### Decision timing

#### A. Must decide before creating Unity

1. Exact Unity LTS editor version.
2. Initial target platform(s).
3. Company name and reverse-domain application identifier root.
4. Unity project folder/name and product display name.
5. Rendering pipeline (URP or HDRP; Built-in only with a documented reason).
6. Input-system package direction and initial device families.
7. Standalone assembly integration approach and Unity-compatible target framework/API surface.
8. Initial source-control/LFS rules for the first binary assets.

Recommended selection process—not an unapproved decision—is:

- choose the current production-supported Unity LTS at bootstrap time;
- create a disposable compatibility spike outside the authoritative project;
- prove Core/Application consumption without Unity references;
- verify required packages and intended renderer on the primary target;
- record exact editor revision in project settings and documentation.

The current `net10.0` assemblies must not simply be assumed compatible with Unity. Before bootstrap, decide whether standalone projects multi-target a Unity-compatible TFM, publish a compatible assembly boundary, or share source through explicitly controlled assembly definitions. Do not fork domain logic into Unity scripts.

#### B. Configure immediately after creation

1. Commit Unity `ProjectSettings/`, `Packages/manifest.json`, `Packages/packages-lock.json`, `Assets/`, and all corresponding `.meta` files.
2. Set company/product identifiers and initial platform.
3. Configure color space, renderer asset, initial graphics API list, and only the minimum quality tier needed for a baseline.
4. Enable/configure the approved input system and create one project-owned input-actions asset.
5. Establish one bootstrap/composition scene and one presentation sandbox scene; do not create a large scene hierarchy.
6. Configure development build defaults, script debugging policy, logging symbols, and a repeatable command/build profile where supported.
7. Set initial window/resolution behavior for the primary target and verify at 16:9, 16:10, ultrawide, and a constrained window where relevant.
8. Add assembly definitions separating Runtime, Editor, and tests.
9. Prove a Unity adapter can read a tiny Core/Application world without owning or mutating source truth incorrectly.
10. Run the Unity portion of the isolated recovery test.

#### C. Can safely wait until later

- final minimum hardware/OS matrix;
- complete low/medium/high/ultra quality tuning;
- advanced frame pacing and platform-specific VSync options;
- console/mobile graphics API decisions unless those platforms are selected initially;
- full localization content and launch-language list;
- addressable/remote content architecture;
- production build signing, store metadata, telemetry, crash reporting, and achievements;
- broad third-party packages;
- final accessibility and controller certification matrices;
- production CI/CD and release channels.

### Configuration principles

- **Resolution/aspect ratio:** build adaptive UI and camera safe areas; do not author critical UI for one fixed resolution. Exact supported ratios wait for platforms.
- **Window modes:** expose platform-appropriate windowed/borderless/fullscreen choices later; avoid hard-coded mode switches in views.
- **Input:** views consume semantic actions, not raw device keys. Game/domain commands remain device-independent.
- **Rendering:** renderer selection is a presentation decision and cannot leak into Core/Application/Simulation.
- **Quality:** use a small number of measured tiers, not duplicated settings for every scene.
- **Frame rate/VSync:** centralize presentation settings; simulation correctness must not depend on render frames or `Time.deltaTime`.
- **Localization:** all user-facing Unity strings should eventually use stable localization keys; domain identities/names remain data, not hard-coded UI text.
- **Builds:** development and release configurations must be explicit and reproducible. Generated builds are artifacts, not source.
- **Unity conventions:** no global “GameManager” containing unrelated systems, no authoritative state in scene singletons, and no database parsing in MonoBehaviours.

## 4. GitHub / Version Control plan

### Recommended branch model

Use **`main` plus short-lived topic branches**. Do not create `develop` now.

Reasoning:

- one active solo developer does not need two permanent integration branches;
- the existing practice already keeps tested commits on `main`;
- a long-lived `develop` branch adds merge drift without a concurrent release train;
- topic branches and PRs provide isolation when change risk warrants it;
- `develop` can be reconsidered only if multiple concurrent contributors/releases create a demonstrated need.

Topic names:

```text
feature/<short-kebab-name>
fix/<short-kebab-name>
refactor/<short-kebab-name>
data/<short-kebab-name>
docs/<short-kebab-name>
release/<version>          # only when release stabilization exists
```

Delete merged topic branches. Do not create empty branches for future ideas.

### When direct `main` work is acceptable

For solo development, small low-risk documentation, tests, or tightly scoped fixes may be committed directly after verification. The standing repository rule still requires commit and push after completed changes.

Use a topic branch and preferably a PR for:

- large refactors;
- persistent schema changes and migrations;
- simulation-engine behavior changes;
- save compatibility changes;
- broad dependency/package upgrades;
- large binary/source-asset additions;
- release preparation;
- changes from outside contributors;
- work where a side-by-side review materially reduces risk.

### Commit style

Use a simple Conventional-Commit-like prefix without requiring complex scopes:

```text
feat: add schedule rescheduling model
fix: preserve local game date across offsets
test: cover historical doubleheaders
docs: define Unity bootstrap decisions
refactor: isolate package validation lookup
chore: update build tooling
data: add synthetic calendar fixture
```

Rules:

- one small logical purpose per commit;
- imperative, descriptive subject;
- architecture decisions and schema changes include matching docs/tests in the same logical change or an immediately associated commit;
- migrations identify source and target schema versions;
- large assets state source, license/authorization, size, and whether LFS is used;
- never mix unrelated formatting or reorganization into feature commits;
- do not preserve obsolete/broken replacements indefinitely “just in case”; remove only after reference checks and tests.

Existing commit subjects need not be rewritten. Apply this guidance forward.

### Pull requests and merging

A PR should state purpose, scope exclusions, tests, schema/data impact, asset/license impact, risks, and follow-up work. Use draft PRs for long-running risky work. Do not require a PR for every tiny solo commit.

Recommended merge behavior:

- squash a reviewed topic branch when its intermediate commits are noisy;
- preserve a small, intentional commit series when migration/review history adds value;
- avoid merge commits solely to mirror Git Flow;
- require green build/tests before merging once CI exists;
- do not force-push shared reviewed branches without coordination.

Branch protection is not urgent for a single owner without CI. Reconsider when contributors or CI are added; at that point require PR review/status checks for high-risk paths rather than creating ceremony prematurely.

### Version and tag policy

Use SemVer-compatible versions when distributable artifacts begin:

- `0.y.z` — pre-release development where compatibility may change;
- `1.0.0+` — public compatibility commitment only when actually ready.

Tags/releases:

```text
checkpoint/YYYY-MM-DD-name   # optional annotated engineering milestone; use sparingly
v0.1.0-internal.1            # internal playable build
v0.2.0-alpha.1               # alpha
v0.5.0-beta.1                # beta
v1.0.0                       # release
```

Use annotated tags created from tested commits. GitHub Releases hold release notes and deliberate build artifacts. Do not tag every commit and do not call current foundations an alpha/playable release.

Package/save schema versions remain separate from game build versions. A game release can support multiple schema versions; do not conflate them.

### Git LFS and large assets

Git LFS is a selective storage mechanism, not a default for every binary extension.

#### Normal Git

- C# and text source;
- JSON/YAML/TOML/Markdown and small configuration/data;
- Unity `.meta` files;
- small optimized textures/icons/audio where normal diffs/history cost remains reasonable;
- small deterministic test fixtures;
- scripts and manifests needed to regenerate assets.

#### Candidate for Git LFS after first real asset review

- `.blend` source files;
- `.psd`/`.psb` source artwork;
- large lossless textures or layered image sources;
- large `.fbx` or other binary model exports that must be versioned;
- large WAV/FLAC source audio;
- other non-diffable binary source files whose history would materially bloat Git.

Do not blindly track an entire extension if most files are tiny. Before enabling a pattern:

1. verify GitHub LFS quota/cost for the account;
2. identify actual file sizes and change frequency;
3. decide whether the source and export both truly need version history;
4. add reviewed `.gitattributes` patterns before the first commit of those assets;
5. test clone/LFS pull in an isolated directory.

#### Outside the repository

- unauthorized/licensed assets that cannot be redistributed;
- private raw datasets or protected reference archives;
- huge intermediate renders/caches;
- purchased asset source files whose license forbids repository distribution;
- local working references without shipping/source-control value.

Store them in an authorized asset vault with a manifest/instructions containing no secret links or credentials. A future bootstrap script may download only redistributable/versioned dependencies.

#### Generated/downloaded by tools

- reproducible derived data;
- caches and imported-library derivatives;
- temporary model/texture exports when the authoritative source and deterministic export procedure are sufficient;
- package caches;
- development builds and test results.

Generated output is committed only when it is a required, reviewed source input for Unity and cannot be reproduced reliably or economically.

### Unity `.gitignore` policy

The current ignore file already covers `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `Build/`, and `Builds/`. At Unity bootstrap, review and add only applicable paths such as crash reports, memory captures, recorders, coverage output, local IDE integrations, and platform build products.

Commit:

- `Assets/**` source/imported project assets intended for the project;
- every Unity `.meta` file corresponding to a committed asset or folder;
- `Packages/manifest.json` and `Packages/packages-lock.json`;
- `ProjectSettings/**` except specifically documented machine-local generated state.

Never globally ignore `*.meta`. Missing `.meta` files break GUID references and reproducibility.

### What must never be committed

- credentials, passwords, tokens, API keys, signing keys, private certificates, or populated secret files;
- `.env` and local secret-store exports (a sanitized `.env.example` may be committed later);
- machine-specific IDE/user state;
- Unity `Library`, `Temp`, `Logs`, generated `obj`, crash/memory capture caches, and package caches;
- generated builds unless deliberately attached to a release outside normal source history;
- temporary exports, renders, autosaves, and editor recovery files;
- local save games, test results, coverage output, and profiling captures unless a small reviewed fixture is intentionally required;
- unauthorized/protected NBA imagery, logos, likeness assets, fonts, audio, footage, or purchased third-party sources;
- raw real-world datasets that are not authorized or practical to distribute;
- private source references and personal data;
- duplicate source and export binaries without an explicit authoritative-source/use justification.

If a secret is committed, removal from the latest commit is insufficient: revoke/rotate it immediately, assess history exposure, and then clean history only with a coordinated recovery plan.

### Backup strategy

GitHub is the primary remote source-control copy, not the complete backup system.

Future minimum:

1. local working copy;
2. GitHub remote;
3. independent scheduled repository mirror/bundle or second authorized remote;
4. separately backed-up LFS objects;
5. authorized source-asset storage backup;
6. controlled data-package backup;
7. later, save/build release backup and checksums.

Document owner, frequency, retention, encryption, and restore test for each store before irreplaceable assets or real datasets enter the project.

## 5. Long-term repository architecture

Recommended structure:

```text
NBA-The-Association/
├── src/
│   ├── NBATheAssociation.Core/
│   ├── NBATheAssociation.Application/
│   ├── NBATheAssociation.Data/
│   └── NBATheAssociation.Simulation/        # create only when approved
├── tests/
│   ├── NBATheAssociation.Tests/             # current; split only when scale justifies
│   └── ...                                  # future focused test projects
├── tools/
│   ├── NBATheAssociation.Cli/
│   └── ...                                  # future editors/validators
├── data/                                    # future canonical authorized packages/metadata
├── docs/
├── plans/
├── source-assets/                           # future authoritative creative sources
│   ├── blender/
│   ├── images/
│   ├── audio/
│   └── references/                          # only redistributable references
├── unity/
│   └── NBATheAssociation/                   # future Unity project
├── builds/                                  # ignored local output, not source history
├── AGENTS.md
├── NBATheAssociation.slnx
└── README.md
```

Only create a directory when its first real, approved content exists. Git does not preserve empty directories, and placeholder taxonomies create churn.

### Responsibility map

| Concern | Authoritative location | Must not live in |
|---|---|---|
| Durable identity, league facts, rules | `src/NBATheAssociation.Core` | MonoBehaviours, scenes, mode-specific UI |
| Headless use cases/runtime session | `src/NBATheAssociation.Application` | scene managers, Data DTOs |
| Package parsing, validation, migration | `src/NBATheAssociation.Data` | MonoBehaviours, UI views |
| Basketball simulation | future `src/NBATheAssociation.Simulation` | animation controllers, Unity frame loop as truth |
| Unity composition/presentation | future `unity/NBATheAssociation` | standalone domain assemblies |
| Developer inspection/editing | `tools/` | shipping domain entities unless runtime capability is required |
| Automated verification | `tests/` and future Unity test folders | production scenes as the sole test harness |
| Creative source files | `source-assets/` or authorized external vault | Unity cache/import output |
| Unity-ready assets | future Unity `Assets/_Project/...` | standalone Core/Data |
| Generated builds | ignored `builds/` / GitHub Releases | normal Git source history |

There is one `Player` domain model, not separate Default/GM/Player implementations. Mode-specific Unity screens may present different actions, but trades, schedules, contracts, AI, and player identity stay in shared standalone systems.

## 6. Future Unity folder architecture

Create only used folders, beginning with this shape:

```text
unity/NBATheAssociation/
├── Assets/
│   ├── _Project/
│   │   ├── Runtime/
│   │   │   ├── Composition/       # bootstrap and dependency wiring
│   │   │   ├── Adapters/          # Core/Application/Simulation ↔ Unity
│   │   │   └── Presentation/      # Unity-facing coordinators, not league truth
│   │   ├── UI/
│   │   │   ├── Shared/
│   │   │   ├── DefaultMode/
│   │   │   ├── GMMode/
│   │   │   └── PlayerMode/
│   │   ├── Scenes/
│   │   ├── Prefabs/
│   │   ├── Art/
│   │   │   ├── 2D/
│   │   │   ├── 3D/
│   │   │   ├── Materials/
│   │   │   ├── Textures/
│   │   │   └── Shaders/
│   │   ├── Animation/
│   │   ├── Audio/
│   │   ├── Fonts/
│   │   ├── Configuration/         # Unity/presentation configuration only
│   │   ├── Editor/
│   │   └── Tests/
│   └── ThirdParty/
├── Packages/
└── ProjectSettings/
```

Do not create `Scripts/Core`, `Scripts/Players`, `Scripts/Teams`, `Scripts/League`, `Scripts/Trades`, or three copies of mode logic. Those names would falsely suggest that Unity owns systems already belonging to standalone assemblies.

`DefaultMode`, `GMMode`, and `PlayerMode` subfolders are acceptable under UI/presentation when they contain views, view models/adapters, icons, layouts, or navigation unique to that perspective. Shared world logic remains outside Unity.

## 7. Source-asset workflow

### Blender / 3D

```text
source-assets/blender/<area>/<asset>.blend
  → reviewed export preset/tool
  → Unity-ready FBX/glTF or direct approved import
  → unity/.../Assets/_Project/Art/3D/<area>/
  → Unity importer settings + .meta
  → material/prefab/presentation asset
```

The `.blend` is authoritative when retained. Commit the exported model only when Unity needs it and reproducible direct import/export is not sufficient. Record scale, axes, units, naming, rig version, export preset, and source/export relationship. Avoid committing render caches and duplicate iterations.

### Images/UI/textures

```text
layered source (.psd/.psb or other approved source)
  → controlled export
  → optimized PNG/TGA/other Unity-ready texture
  → Unity importer settings + .meta
  → sprite/material/UI asset
```

Keep layered sources under `source-assets/images/` or an authorized asset vault. Unity receives optimized exports, not every working draft. Fonts require explicit redistribution/embedding rights.

### Audio

Keep approved high-quality masters separate from Unity-ready compressed files. Record source, license, sample rate, loudness target, edit/export version, and looping requirements. Large masters are LFS/external-vault candidates; Unity-ready output is committed only when needed by the project.

### References

Commit references only when redistribution is authorized and they are genuinely required. Otherwise store a metadata note/citation or use an authorized external reference library. Never treat internet availability as permission to commit an asset.

## 8. Naming conventions

Prefer descriptive names and stable domain vocabulary over prefixes for their own sake.

| Artifact | Convention | Example |
|---|---|---|
| Repository folders | lowercase kebab-case for new non-.NET top-level folders | `source-assets/` |
| C# projects | `NBATheAssociation.<Responsibility>` | `NBATheAssociation.Simulation` |
| Namespaces | match project and logical folder, PascalCase | `NBATheAssociation.Application` |
| C# files/types | PascalCase; normally one primary public type per file | `LeagueSession.cs` |
| Interfaces | `I` prefix only for actual abstraction boundaries | `IPackageNormalizer` |
| Tests | behavior-focused names; fixture class by system | `Advance_to_date_rejects_backward` |
| JSON test fixtures | lowercase kebab-case with purpose | `league-calendar-short.json` |
| Canonical data packages | lowercase kebab-case plus explicit version/era where useful | `synthetic-2035-v2.json` |
| Unity scenes | PascalCase descriptive purpose; no numbering unless ordered flow requires it | `Bootstrap.unity`, `PresentationSandbox.unity` |
| Prefabs | PascalCase noun or role | `TeamHeader.prefab` |
| Materials | PascalCase plus meaningful variant | `ArenaFloorGloss.mat` |
| Textures | descriptive PascalCase with map role suffix when useful | `ArenaFloor_Normal.png` |
| Animations | PascalCase action/subject | `TrophyReveal.anim` |
| UI assets | descriptive component/state | `PrimaryButton_Hover.png` |
| Blender files | lowercase-kebab asset name and meaningful variant/version only when needed | `league-trophy-source.blend` |
| Layered artwork | lowercase-kebab source name | `broadcast-scorebug-source.psd` |

Avoid opaque Hungarian-style prefixes, initials only the creator understands, dates as the only version control, and names such as `New`, `FinalFinal`, `Manager`, or `System` without a precise responsibility.

## 9. Migration table

No broad move is justified.

| Current path/state | Recommended path/state | Classification | When | Why |
|---|---|---|---|---|
| `src/NBATheAssociation.Core` | same | **DO NOT MOVE** | — | Correct durable-domain boundary. |
| `src/NBATheAssociation.Application` | same | **DO NOT MOVE** | — | Correct headless workflow boundary. |
| `src/NBATheAssociation.Data` | same | **DO NOT MOVE** | — | Correct persistence/import boundary. |
| `tests/NBATheAssociation.Tests` | same initially | **MOVE ONLY IF NEEDED** | Split only when test scale/runtime warrants focused projects. | Avoid project proliferation. |
| `tools/NBATheAssociation.Cli` | same | **DO NOT MOVE** | — | Correct developer-tool boundary. |
| test JSON under `tests/.../Fixtures` | same | **DO NOT MOVE** | — | These are test inputs, not production data. |
| no `README.md` | root `README.md` | **MOVE NOW** (create, not move) | Next safe foundation task. | Enables clone/build/test onboarding and recovery. |
| no `data/` | root `data/` | **MOVE ONLY IF NEEDED** | First approved canonical non-test dataset/package. | Avoid empty folder and mixing test data with product data. |
| no `source-assets/` | root `source-assets/` | **MOVE WHEN UNITY IS CREATED** | First approved source asset. | Establish source/export boundary with actual assets and LFS decision. |
| no Unity project | `unity/NBATheAssociation/` | **MOVE WHEN UNITY IS CREATED** | After prerequisite decisions and headless milestone. | Keeps host separate from standalone systems. |
| local generated build folders | ignored `builds/` or external artifacts | **DO NOT MOVE into Git** | When builds exist. | Builds are outputs, not source. |

## 10. Repository recovery strategy

Never delete the only working copy to test recovery.

### Standalone recovery test — can run now

Use a newly created temporary directory or another machine:

1. Record current commit SHA and required .NET SDK version.
2. Clone `https://github.com/getrasted-collab/NBA-The-Association.git` into the isolated location.
3. Confirm checkout is the recorded commit and working tree is clean.
4. Run `dotnet --info` and verify a compatible .NET 10 SDK.
5. Run `dotnet restore NBATheAssociation.slnx`.
6. Run `dotnet build NBATheAssociation.slnx --no-restore`.
7. Run `dotnet test NBATheAssociation.slnx --no-build --no-restore`.
8. Run a CLI smoke test against a committed synthetic fixture.
9. Confirm no untracked/generated files are required for success.
10. Delete only the isolated test clone after recording results.

Completion requires documented commands, SDK prerequisite, commit tested, build/test counts, CLI result, and any missing setup repaired in source control.

### Unity recovery extension — after Unity exists

1. Install the exact recorded Unity editor revision through an authorized method.
2. Clone into a fresh directory and fetch required LFS objects.
3. Restore authorized external assets through documented procedures.
4. Open the project and allow package/import restoration.
5. Confirm no committed asset GUID/reference is missing.
6. Run standalone and Unity tests.
7. Open bootstrap and presentation sandbox scenes without errors.
8. Produce the documented development build for the primary target.
9. Verify the build launches and can consume a standalone world through the approved adapter.

This milestone validates repository recovery, not backups alone. It should be repeated after major Unity/package/build-pipeline changes.

## 11. Dependency-ordered milestones

### Foundation A — Repository onboarding and Git standards — **DONE**

- **Prerequisites:** current repository and owner acceptance of this plan.
- **Tasks:** create root README; record .NET prerequisite/build/test/CLI commands; adopt commit/branch/PR guidance; refine `.gitignore` only with reviewed applicable patterns; document secret policy.
- **Completion:** a new contributor can understand layout and run the standalone build from documentation.
- **Verification:** README command walkthrough in current checkout.
- **Can be done now:** yes.
- **Codex autonomous:** yes for README/ignore/documentation; no for enabling account-level rules without owner direction.
- **Owner decision:** whether to enable branch protection now; recommendation is wait until CI/contributors.

### Foundation B — Standalone recovery proof — **DONE**

- **Prerequisites:** Foundation A README commands.
- **Tasks:** fresh isolated clone, restore, build, 33+ tests, CLI smoke test; document result.
- **Completion:** repository reconstructs standalone environment without the working copy.
- **Verification:** clean-clone log and commit SHA.
- **Can be done now:** yes.
- **Codex autonomous:** yes, using a separate temporary directory and non-destructive cleanup.
- **Owner decision:** none unless network/account access fails.

### Foundation C — Unity product decisions

- **Prerequisites:** primary presentation/platform goals and current Unity compatibility research at execution time.
- **Tasks:** approve editor LTS, platforms, company/product identifiers, renderer, input direction, frame-rate direction, and standalone-assembly compatibility approach.
- **Completion:** every “must decide before creation” item has an accepted ADR/checklist value.
- **Verification:** decision review against target hardware and a disposable compatibility spike.
- **Can be done now:** partially; product decisions require the owner and version/package facts should be refreshed near bootstrap.
- **Codex autonomous:** research/recommendations and spike execution when authorized; cannot invent product/legal identity or platform priorities.
- **Owner decision:** required.

### Foundation D — Asset and LFS readiness

- **Prerequisites:** first approved real source asset and known GitHub LFS quota/rights.
- **Tasks:** classify source/export/external/generated status; add narrow `.gitattributes`; create only needed `source-assets` folders; document export and license metadata.
- **Completion:** first binary asset round-trips through clone without bloating normal Git or violating rights.
- **Verification:** isolated LFS clone/pull and Unity/source-tool open test.
- **Can be done now:** policy only; activation should wait.
- **Codex autonomous:** repository configuration after explicit asset decision; owner must confirm rights/quota.
- **Owner decision:** required for asset rights/storage.

### Foundation E — Unity project creation

- **Prerequisites:** Foundation C decisions, roadmap authorization, and LFS policy for included assets.
- **Tasks:** create `unity/NBATheAssociation`, exact editor settings, packages, renderer, input, assembly definitions, bootstrap/sandbox scenes, and initial adapter boundary.
- **Completion:** committed Unity source/settings open cleanly and consume a standalone sample world without owning domain truth.
- **Verification:** Unity compile/tests, Console clean baseline, dependency review.
- **Can be done now:** no; deliberately deferred.
- **Codex autonomous:** can execute after explicit authorization and environment readiness; owner decisions remain prerequisites.
- **Owner decision:** explicit creation authorization.

### Foundation F — Unity baseline and recovery proof

- **Prerequisites:** committed Unity project and documented editor version/external assets.
- **Tasks:** configure baseline resolution/window/build behavior; run fresh-clone Unity restore; create a development build.
- **Completion:** clean clone opens without missing assets and produces a launching development build.
- **Verification:** isolated clone/import/test/build log and artifact checksum.
- **Can be done now:** no.
- **Codex autonomous:** largely after authorization; platform signing/store steps may require owner credentials handled outside Git.
- **Owner decision:** target platform/build acceptance.

### Foundation G — Release governance

- **Prerequisites:** first internal playable build.
- **Tasks:** choose concrete initial version, annotated tag, release notes template, artifact retention, checksums, and branch protection/CI needs.
- **Completion:** repeatable internal release from a tested commit.
- **Verification:** fresh installation/build artifact smoke test and recoverable tag.
- **Can be done now:** no practical need.
- **Codex autonomous:** mechanics after owner chooses release milestone.
- **Owner decision:** release readiness and distribution audience.

## 12. Decisions needed from the owner

### Blocking before Unity creation

1. Primary initial target platform(s).
2. Studio/company name and reverse-domain identifier root.
3. Exact product display name versus repository/project code name.
4. Renderer priority: visual ceiling versus hardware breadth/performance, leading to URP/HDRP choice.
5. Initial input devices and whether keyboard/mouse plus controller are required from day one.
6. Frame-rate/performance direction for the primary target.
7. Localization ambition for first playable/release.
8. Timing threshold for Unity creation: after which headless milestone is the host justified?

### Important but not blocking current standalone work

1. Whether to enable branch protection before CI/contributors. Recommendation: not yet.
2. Independent backup destination/owner and retention schedule.
3. GitHub LFS budget and authorized asset-vault choice before large binaries.
4. Whether internal checkpoint tags are useful before the first playable. Recommendation: use sparingly.

## 13. Tasks Codex can complete now

Without Unity or new product decisions, Codex can safely:

1. add a lightweight PR template only if the owner wants PRs now;
2. inspect GitHub settings and recommend future branch protection/CI without enabling them;
3. create an independent backup operations checklist once the owner selects its destination;
4. repeat the isolated recovery test after major dependency/tooling changes.

Do not create speculative branches, LFS patterns, empty asset trees, or Unity settings as substitutes for real work.

## 14. Tasks that must wait for Unity or later

### Wait for Unity authorization

- Unity project creation and editor version pinning;
- renderer/input/package installation;
- Unity Assets/Packages/ProjectSettings hierarchy;
- `.meta` verification;
- assembly definitions and adapter proof;
- scenes, prefabs, presentation configuration;
- Unity fresh-clone/import/build recovery test;
- source-asset folders and LFS activation when actual approved assets arrive.

### Wait until later need

- production CI/CD, signing, stores, release branches, and protected release workflows;
- final hardware/OS matrix and quality tiers;
- complete localization content;
- production builds in GitHub Releases;
- real-data storage/import distribution;
- multi-contributor branch enforcement;
- splitting current tests/tools/projects without scale pressure.

## 15. Definition of Project Foundation Complete

For these three scoped areas, Project Foundation is complete when:

1. the root README reproduces the standalone restore/build/test/tool workflow;
2. Git/branch/commit/PR/secrets/generated/LFS/asset policies are accepted and followed;
3. an isolated standalone clone/recovery test passes and is documented;
4. an independent repository/source-asset backup and restore procedure exists;
5. the long-term folder boundaries are documented without unnecessary reorganization;
6. all pre-Unity product/technical decisions are accepted in the decision log;
7. Unity is created only after roadmap authorization at `unity/NBATheAssociation/` using the pinned editor;
8. Unity settings, packages, source assets, and `.meta` files are correctly committed while caches/builds remain excluded;
9. Unity consumes standalone domain/application/simulation boundaries without duplicating league truth;
10. a fresh isolated clone restores packages/assets, passes standalone and Unity tests, opens without missing committed assets, and produces a development build;
11. no credential, unauthorized asset, protected raw data, or generated cache/build has entered source history.

Until Unity is authorized, completing items 1–5 constitutes the **Standalone Repository Foundation Complete** milestone; it does not falsely claim Unity foundation completion.

## Recommended next Project Foundation task

Choose an independent encrypted backup destination, owner, frequency, and retention policy, then document and test a repository backup/restore separate from GitHub. Do not enable LFS or create Unity merely to advance checklist status.
