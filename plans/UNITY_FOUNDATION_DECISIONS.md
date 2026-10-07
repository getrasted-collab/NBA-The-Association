# Unity Foundation Decisions

**Status:** Proposed for owner approval; planning only  
**Research date:** 2026-10-07

This plan resolves the recommended pre-creation direction. It does not authorize installing Unity, creating the authoritative Unity project, adding packages, or changing gameplay/domain code.

## 1. Recommended decision set

| Topic | Recommendation | State |
|---|---|---|
| Editor | Unity 6.3 LTS; pin exact safe patch, currently `6000.3.25f1` | Owner approval required |
| Initial platform | Windows x64 desktop | Owner approval required |
| Project identity | `unity/NBATheAssociation/`; product `NBA The Association` | Safe default |
| Renderer/template | HDRP / High Definition 3D | Owner approval required |
| Input | Unity Input System; keyboard, mouse, controller | Safe default |
| Presentation | 60 FPS target; simulation independent of rendering | Safe default |
| UI | UI Toolkit first; narrow evidence-based uGUI exceptions | Safe default, validate in spike |
| Integration | Multi-target project-owned libraries and consume .NET Standard 2.1 managed plug-ins | Compatibility spike required |

Architectural invariant:

```text
Core ← Application ← future Simulation ← Unity composition/presentation
Data ── validates/materializes ──> Core
```

Unity is a host. It must not duplicate or own authoritative NBA world state.

## 2. Unity version

Use the **Unity 6.3 LTS family**, supported through December 2027, rather than Unity 6.0 LTS, whose support ends in October 2026, or a shorter-lived Update release. Pin **6000.3.25f1** for the disposable spike and bootstrap if it remains the current safe 6.3 patch at execution time. Unity released that patch on 2026-09-24.

Commit `ProjectVersion.txt` when the real project exists. Do not auto-upgrade. Patch within 6.3 only after release-note/security review, backup, topic-branch compile/tests, and a development build. Evaluate a later LTS in a disposable upgraded clone at a planned milestone.

Risks include package/template regressions, a support horizon shorter than the game's development, and Unity's managed API profile differing from .NET 10. Refresh advisories and package compatibility immediately before installation.

Sources: [Unity 6 support policy](https://unity.com/releases/unity-6/support), [6000.3.25f1 notes](https://unity.com/releases/editor/whats-new/6000.3.25f1), [archive](https://unity.com/releases/editor/archive).

## 3. Initial platform

Use **Windows x64** for development and the first internal playable. Prefer DX12 for HDRP, retain/test DX11 fallback, and postpone the shipping Mono-versus-IL2CPP choice. Do not declare final consumer minimum hardware/OS until representative scenes are profiled.

macOS, Linux, and consoles remain possible. Mobile, Web, XR, and handheld targets are not bootstrap requirements. Preserve portability through semantic input, adaptive UI, platform/storage adapters, and Unity-free Core/Application/Simulation—not by maintaining multiple render pipelines.

## 4. Project identity and location

- Unity project path/folder: `unity/NBATheAssociation/`
- Product display name: `NBA The Association`
- Unity-owned root namespace: `NBATheAssociation.Unity`

This co-locates recovery/versioning while leaving `src/`, `tests/`, and `tools/` intact. Never move standalone projects under `Assets`.

Owner must provide a durable legal/studio/company name and reverse-domain root. Examples only: `com.example.nbatheassociation` or `com.studioname.nbatheassociation`. Do not infer a legal identity from the GitHub username.

## 5. Render pipeline and template

### URP versus HDRP

URP offers wider platform/hardware reach, faster baseline iteration, and a lower GPU floor. HDRP costs more in shader/import complexity and hardware, but supplies the physical lighting, advanced materials, skin/hair/eye rendering, volumetrics, and high-end post-processing aligned with realistic arenas, player closeups, and broadcast presentation. Switching later is costly because materials, shaders, lighting, and effects differ.

**Recommendation: HDRP.** The approved vision is premium realistic 3D presentation on desktop, not mobile breadth. Keep it responsible by starting with two small scenes, one conservative quality profile, no ray tracing, and profiling each expensive feature. Do not maintain URP in parallel. If broad low-end reach is more important than the visual ceiling, the owner must choose URP before creation.

Create the eventual project with the **High Definition 3D** template matching the pinned editor. Unity describes URP as cross-platform/scalable and HDRP as high-fidelity for high-end platforms: [render-pipeline guidance](https://docs.unity3d.com/6000.0/Documentation/Manual/choose-a-render-pipeline.html).

## 6. Input

Use the official **Input System**, not legacy `UnityEngine.Input`. Bootstrap supports keyboard, mouse/pointer/wheel, and common Windows controllers. Define intent rather than raw keys:

```text
UI/Navigate  UI/Submit  UI/Cancel  UI/Menu
UI/TabLeft   UI/TabRight  UI/ContextAction
UI/Point     UI/Click   UI/Scroll
```

Bindings, rebinding UX, repeat rules, and accessibility alternatives wait for the input/UI slice. Domain commands never reference Unity input types. Unity recommends Input System for new projects and UI Toolkit supports its navigation/pointer events: [input guidance](https://docs.unity3d.com/6000.0/Documentation/Manual/Input.html), [runtime UI input](https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-Runtime-Event-System.html).

## 7. Frame rate and time

- Target stable **60 FPS** presentation.
- Default VSync on; cap at 60 when VSync is off initially.
- Add 120/144/high-refresh or uncapped modes only after animation/input profiling.
- UI/presentation may use Unity frame or unscaled time.
- League/basketball simulation advances only through explicit deterministic steps, commands, clocks, and seeds—never `Update`, `FixedUpdate`, `Time.deltaTime`, or displayed FPS.
- Fast/headless simulation may execute many steps without rendering.

## 8. Resolution and aspect ratio

Use 1920×1080 as a **reference viewport**, not a fixed canvas. UI must use responsive/flex layouts, min/max widths, scalable type, virtualized/scrollable data, readable maximum content width, and safe margins. Keep 3D camera composition separate from UI. Support windowed and borderless fullscreen initially; exclusive fullscreen can wait.

Bootstrap tests must cover 1920×1080, 2560×1440, 4K, 16:10, 2560×1080 and 3440×1440 ultrawide, plus a 1280×720 resizable-window stress case (not yet a promised minimum). Verify text reflow, modal bounds, controller focus, pointer targets, resize/focus transitions, and 3D safe composition.

## 9. Unity ↔ standalone integration

### Compatibility constraint

The current assemblies target `net10.0`. Unity 6.3's portable managed plug-in profile is **.NET Standard 2.1**, and Unity documents .NET Core-targeted plug-ins as unsupported. Current .NET 10 DLLs cannot simply be copied into Unity. Core/Application also use `DateOnly`, which is not in the .NET Standard 2.1 reference surface.

Source: [Unity .NET profile support](https://docs.unity3d.com/6000.0/Documentation/Manual/dotnet-profile-support.html).

### Decision

Reject manual source copies, duplicate Unity domain classes, direct `net10.0` DLL references, and a third-party NuGet importer. Prefer:

1. retain `net10.0` for standalone development/tests;
2. multi-target Core, Application, and future Simulation to `netstandard2.1` after a spike identifies required changes;
3. build compatible DLLs with `dotnet` and copy/sync them automatically into one project-owned Unity managed-plug-in location;
4. reference them from a separate Unity adapter assembly;
5. keep Data outside the first proof and add only a narrow materialization/composition adapter when actually needed;
6. keep standalone NUnit tests authoritative and add Unity EditMode tests only for loading/adaptation.

The sync must be reproducible from a clean clone and detect stale/missing DLLs. The recovery spike will decide whether generated DLLs are committed or rebuilt before opening Unity; their C# source remains authoritative.

If `DateOnly` blocks multi-targeting as expected, prefer a small project-owned immutable civil-date value type in Core with Data-layer conversion. This preserves ADR-017's day-only semantics without a fake `System.DateOnly`, broad polyfill dependency, or duplicate model. That change requires its own approved plan/tests/ADR clarification and is **not** implemented here.

### Disposable compatibility spike

Before the real project:

1. use the pinned editor in a temporary directory outside the repository;
2. create a minimal disposable HDRP project;
3. attempt multi-targeting on a topic branch/worktree;
4. build/reference Core and Application as .NET Standard 2.1 managed plug-ins;
5. in an EditMode test construct/read a synthetic `LeagueWorld`, create a `LeagueSession`, query calendar/schedule state, and prove two sessions are independent;
6. compile Editor and create a Windows development build;
7. confirm Core/Application contain no Unity, JSON, filesystem, or presentation dependency;
8. record API/compiler/stripping findings and remove only the disposable project.

The authoritative Unity project is blocked until this passes.

## 10. Packages

Commit exact package versions in `manifest.json` and `packages-lock.json`; no floating Git package references.

**Bootstrap:** HDRP/template dependencies, Input System, Unity Test Framework/EditMode support, and the editor's built-in runtime UI Toolkit. Do not add a preview UI package.

**First need:** Cinemachine 3 with the first intentional animated camera; Localization at the first real UI vertical slice; uGUI/TextMeshPro assets only for a proven uGUI surface; Timeline for authored sequences; Addressables for measured content-delivery needs; Burst/Jobs/Collections for measured workloads outside portability boundaries.

**Not now:** multiplayer, analytics/ads/cloud/economy, DOTS/ECS, third-party DI/event buses/reactive/tween/save/database/NuGet frameworks, unapproved platform SDKs, or demo/asset packs.

## 11. UI technology

Use **UI Toolkit first** for navigation shells, dashboards, rosters, tables, settings, modals, and developer tools. Its retained tree, UXML/USS separation, flex layout, reusable controls, and list virtualization suit this data-heavy game. Use uGUI only where a measured surface—such as specialized world-space UI—proves materially better. Never implement screens twice.

The first UI vertical slice must prove large virtualized tables, sorting/filtering, controller focus/recovery, mouse/keyboard use, responsive aspect ratios, animated transitions over HDRP 3D, localization readiness, and allocation/layout performance. Unity recommends UI Toolkit for new UI work: [UI Toolkit](https://docs.unity3d.com/6000.0/Documentation/Manual/UIElements.html).

## 12. Graphics baseline

- Linear color space.
- HDRP/HDR baseline from the template.
- One conservative `Desktop` quality profile; no premature Low/Medium/High/Ultra.
- Evaluate HDRP TAA for 3D while checking UI sharpness and motion ghosting.
- Prefer DX12 and test DX11 fallback.
- Do not enable ray tracing at bootstrap.
- Shadows, volumetrics, reflections, post-processing, render scale, dynamic resolution/upscaling, crowds, and texture budgets wait for representative-scene profiling.

Record test hardware and CPU/GPU frame timings once `PresentationSandbox` exists.

## 13. Localization readiness

Install Unity Localization with the **first real UI vertical slice**, not bootstrap. Until then, prohibit durable user-facing strings scattered through view scripts; use a presentation text boundary/stable keys, never display domain enums/IDs directly, preserve world names as data, and design for text expansion and locale-aware number/date formatting. Launch languages and RTL scope can wait.

## 14. Unity coding boundaries

Unity may own composition, adapters, MonoBehaviours, scenes, cameras, animation, materials, shaders, VFX, audio, UI, input, localization presentation, platform settings, and asset references.

Unity may not own authoritative players, teams, franchises, league/calendar/schedule, rules, contracts, trades, drafts, injuries, statistics, AI, simulation results, save schemas/state, or separate mode copies of shared systems.

Rules:

- no giant `GameManager`, `LeagueManager`, or `PlayerManager`;
- scene objects are replaceable views/adapters, never world truth;
- one small explicit composition root; no service locator or speculative DI framework;
- lower assemblies never reference Unity;
- views issue explicit application commands and render snapshots/read models;
- Editor tooling remains in Editor-only assemblies.

## 15. Future bootstrap sequence

1. Obtain owner approvals and company/identifier values.
2. Refresh 6000.3 security/patch/package status and install the exact editor plus Windows support.
3. Run the disposable compatibility spike; stop on failure.
4. Plan/implement any required standalone compatibility change separately and rerun all .NET tests.
5. Create High Definition 3D at `unity/NBATheAssociation/`.
6. Set identity, Windows x64, Linear color, HDRP, DX12 preference/DX11 fallback, and .NET Standard 2.1 compatibility.
7. Retain/install only approved bootstrap packages and lock versions.
8. Configure one semantic Input System action asset.
9. Create only used Runtime, Editor, test, and adapter assembly boundaries/folders; commit every asset with `.meta`.
10. Create minimal `Bootstrap` and `PresentationSandbox` scenes.
11. Add only the proven Core/Application adapter; no gameplay or production UI.
12. Configure 60 FPS, window/borderless baseline, and responsive panel settings.
13. Run standalone tests, Unity compilation/EditMode tests, and a Windows development build.
14. Review Console/dependencies/exclusions and absence of duplicate domain models.
15. Commit/review/merge, then prove fresh clone → .NET restore/build/sync → exact Unity open/package restore/tests/build.
16. Record a bootstrap/recovery report before feature work.

## 16. Owner decisions

### Recommended defaults — safe to approve as a bundle

- Unity 6.3 LTS/current safe 6000.3 patch; Windows x64; HDRP/High Definition 3D.
- Folder/product names above; Input System; keyboard/mouse/controller.
- 60 FPS/VSync baseline with frame-independent simulation.
- Responsive 1080p reference plus 1440p/4K/16:10/ultrawide/window tests.
- UI Toolkit-first; multi-target managed plug-in integration.
- Linear color, one conservative quality baseline, no bootstrap ray tracing.
- Localization package at first UI slice.

### Owner must choose

1. Durable company/studio name.
2. Reverse-domain identifier root.
3. Approve HDRP's high-end desktop focus (recommended) versus URP's broader reach.
4. Explicitly authorize the disposable compatibility spike when ready.

### Can wait

Other platforms, consumer minimum hardware, final quality tiers, shipping backend, high-refresh/exclusive-fullscreen/HDR output/upscaling/ray tracing, Cinemachine/Localization/Timeline/Addressables until used, launch languages, platform services, signing, and CI/CD.

## 17. Exit and next step

Planning is ready for approval. Foundation C is not complete until the owner accepts the decisions and the compatibility spike passes.

After approval, the exact next step is **the disposable Unity 6.3 LTS compatibility spike outside the repository**, not creation of `unity/NBATheAssociation/`. It can safely begin once the owner approves Unity/Windows/HDRP/integration direction, the exact patch is refreshed, and temporary editor/disk prerequisites are available.
