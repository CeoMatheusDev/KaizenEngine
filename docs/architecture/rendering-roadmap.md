# Rendering Architecture and Technical Roadmap

> **Status:** Proposed engineering roadmap  
> **Target:** Desktop, mobile, and Web  
> **Visual target:** High  
> **Rendering strategy:** Kaizen-owned rendering architecture, integrated incrementally with the existing engine  
> **Baseline:** Godot-derived rendering infrastructure

## 1. Objective

Evolve KaizenEngine from its inherited rendering foundation toward a renderer whose architecture, policies, and feature development are owned by Kaizen, while retaining the engine's platform integration, resource model, editor workflows, and application compatibility.

"Custom renderer" means a Kaizen-designed rendering pipeline and renderer implementation. It does **not** mean writing GPU drivers, replacing Vulkan/Metal/OpenGL, or immediately discarding the existing RenderingDevice and platform abstractions.

The work must support three broad target classes:

- **Desktop:** the primary environment for feature development and high visual quality.
- **Mobile:** a first-class target with explicit GPU bandwidth, memory, power, and thermal budgets.
- **Web:** a constrained target using browser-supported graphics APIs. WebGL 2 is the conservative baseline for the current Web export path; WebGPU can be evaluated as a future backend when the engine's platform integration is ready.

Feature parity across these targets is not assumed. The renderer must expose capabilities and select appropriate paths per device.

## 2. Non-goals

The initial program will not:

- implement Vulkan, Metal, OpenGL, or browser GPU drivers;
- rewrite the entire rendering stack in one change;
- require compute shaders, storage buffers, or bindless resources on every target;
- make ray tracing a prerequisite for high visual quality;
- promise identical effects or image quality on desktop, mobile, and Web;
- optimize based on intuition without reproducible profiling;
- duplicate the inherited renderer indefinitely without a migration and ownership plan.

## 3. Current source baseline

The repository contains a Godot-derived rendering stack. The following paths are starting points for source investigation:

| Path | Role in the inherited architecture |
|---|---|
| `servers/rendering/` | Rendering server interfaces and orchestration. |
| `servers/rendering/rendering_server_default.cpp` | Default rendering-server implementation and integration entry point. |
| `servers/rendering/renderer_rd/` | RenderingDevice-based renderer infrastructure. |
| `servers/rendering/renderer_rd/renderer_scene_render_rd.h` and `.cpp` | RD scene-rendering implementation and shared scene-rendering behavior. |
| `servers/rendering/renderer_rd/forward_clustered/` | Forward+ renderer path. |
| `servers/rendering/renderer_rd/forward_mobile/` | Mobile-oriented RD renderer path. |
| `drivers/gles3/` | Compatibility renderer path. |
| `drivers/vulkan/` | Vulkan integration. |
| `platform/web/` | Web platform integration. |

This list is a navigation map, not a complete call graph. The first milestone must verify renderer selection, object construction, resource ownership, synchronization, shader compilation, and threading against the current source.

### 3.1 Important platform constraint

The current Web export path must be treated as a WebGL 2 Compatibility target unless source and build configuration prove otherwise. Do not assume Forward+ or the mobile RD renderer can be used for Web exports. WebGPU is a separate future investigation, not a baseline dependency.

## 4. Target architecture

The intended architecture separates scene-facing rendering policy from backend-specific execution.

```text
Scene / Camera / Environment / Materials / Lights
                         |
                 Kaizen Renderer API
                         |
        Visibility / Render Pipeline / Render Graph
                         |
        Resource Registry / Shader & Pipeline System
                         |
              Backend Capability Layer
             /             |             \
         Vulkan          Metal        OpenGL / WebGL
        Desktop        Apple/mobile    Compatibility
```

This is a conceptual boundary, not a proposal to create all of these directories immediately. The actual integration must follow the existing engine interfaces and avoid creating parallel ownership systems.

### 4.1 Scene-facing layer

Responsibilities:

- consume scene, camera, material, light, and environment data through engine-supported interfaces;
- build a frame description;
- perform visibility selection and draw-list construction;
- select rendering features according to renderer capabilities;
- preserve editor and runtime behavior.

This layer should not issue raw backend commands.

### 4.2 Render core

Responsibilities:

- frame lifecycle and pass scheduling;
- render-graph dependency validation;
- logical GPU resource tracking;
- shader and pipeline variant management;
- capability queries;
- diagnostics and profiling hooks.

The core should express what a frame needs to execute, not encode Vulkan-only assumptions into common code.

### 4.3 Backend layer

A backend translates the renderer's resource and pass operations into the selected graphics API. Backend-specific synchronization, memory allocation, shader compilation, and pipeline creation must remain behind explicit interfaces.

Candidate backends must be evaluated against the actual platform integration:

| Backend | Intended role | Status |
|---|---|---|
| Vulkan | High-performance desktop and supported mobile devices | Existing engine integration; Kaizen-owned renderer work is planned. |
| Metal | Apple platforms where the existing integration is available | Existing engine integration must be audited before reuse. |
| OpenGL / WebGL 2 | Compatibility and Web baseline | Existing compatibility path; Kaizen pipeline work is planned. |
| WebGPU | Possible future Web backend | Research only; not a current requirement. |

Do not implement an API backend merely because it appears in this table. First establish the renderer contract and verify the engine's existing device abstractions.

## 5. Core engineering subsystems

### 5.1 Capability model

Introduce or adapt a capability description that records actual device/backend support, such as:

- maximum texture dimensions and render-target limits;
- supported color and depth formats;
- sample counts;
- shader feature level and available stages;
- compute and storage-buffer support;
- texture compression formats;
- timestamp and profiling support;
- synchronization and presentation constraints.

Capabilities must be queried from the backend and consumed by feature selection. Avoid checks such as "if platform is mobile, disable feature X" when the actual device capability is available.

### 5.2 Resource lifetime and memory

The resource system must define:

- stable logical handles with generation validation where appropriate;
- ownership and reference rules;
- explicit CPU/GPU lifetime boundaries;
- deferred destruction for resources still referenced by in-flight frames;
- synchronization before reuse or destruction;
- bounded transient allocation and reuse;
- upload staging and asynchronous transfer where supported;
- cache invalidation on device loss or renderer restart.

A handle design must integrate with the engine's RID/resource conventions rather than creating an incompatible second resource identity system.

### 5.3 Render graph

A render graph should describe passes and resource dependencies declaratively.

Each pass should declare:

- resources read and written;
- attachment formats and sample counts;
- load/store behavior;
- viewport and resolution policy;
- execution constraints and required capabilities.

The graph can then validate read/write hazards, derive execution order, calculate resource lifetimes, and identify safe transient-resource reuse. Backend barriers and transitions must be generated from verified API rules.

Start with correctness and validation. Aliasing, pass merging, and aggressive scheduling are later optimizations.

### 5.4 Shader and pipeline system

The shader system should provide:

- explicit shader stages and variants;
- deterministic variant keys;
- reflection or validated metadata for resource bindings;
- pipeline cache keys that include relevant formats and fixed-function state;
- asynchronous compilation where safe;
- diagnostics that identify source shader, variant, backend, and compile failure;
- cache invalidation when shader or pipeline inputs change.

Avoid unbounded variant multiplication. Define a policy for feature combinations and measure compilation and cache behavior.

### 5.5 Visibility and submission

Implement visibility in stages:

1. frustum culling and correct bounds;
2. static and dynamic visibility handling;
3. draw-list sorting by pipeline/material/depth as appropriate;
4. instancing for compatible repeated geometry;
5. LOD and streaming policies;
6. occlusion culling only when its CPU/GPU cost is justified.

The first implementation should prioritize correctness and predictable frame cost over complex culling algorithms.

### 5.6 Profiling and diagnostics

Expose at least:

- CPU frame time and render-thread time;
- GPU frame time where timestamp queries are supported;
- pass-level timing;
- draw and dispatch counts where applicable;
- pipeline and shader cache hit/miss counts;
- resource allocation and transient memory peaks;
- texture upload volume;
- synchronization stalls;
- resolution scaling and selected quality tier.

Profiling must degrade gracefully on APIs that do not expose GPU timestamps.

## 6. Rendering feature strategy

Implement a coherent forward-rendering baseline before advanced effects.

| Feature | Desktop high | Mobile | WebGL 2 baseline |
|---|---|---|---|
| Physically based materials | Core target | Core target with cost controls | Core target with shader constraints |
| Metallic/roughness workflow | Yes | Yes | Yes, constrained variants |
| Normal mapping | Yes | Yes | Yes |
| Directional lighting | Yes | Yes | Yes |
| Point and spot lights | Scalable | Strict per-device limits | Conservative limits |
| Directional shadows | High-quality options | Reduced resolution/cascade count | Conservative implementation |
| Image-based lighting | Yes | Yes, memory-aware | Yes, constrained resources |
| HDR and tone mapping | Yes | Device-dependent format path | Validate browser/API path |
| SSAO | Optional high tier | Optional low-cost tier | Optional, benchmark-gated |
| SSR | Optional | Usually disabled or reduced | Optional, benchmark-gated |
| TAA | Evaluate after motion-vector path | Evaluate carefully | Evaluate carefully |
| Volumetric fog | High tier | Reduced or disabled | Reduced or disabled |
| Global illumination | Later milestone | Baked/probe-first strategy | Baked/probe-first strategy |
| Ray tracing | Not required | Not required | Not required |

This table describes intended priorities, not current feature availability. Each feature requires implementation, platform validation, quality controls, and performance measurements before it can be marked supported.

## 7. Quality tiers

Use capability-driven quality tiers rather than equating a tier with a particular operating system.

- **Low:** minimal bandwidth and memory use; conservative shadows, lighting limits, and post-processing.
- **High:** intended default quality target; balanced shadows, materials, and selected post effects.
- **Ultra:** optional desktop tier for higher resolutions, shadow quality, and advanced effects.

The tier selects defaults. It must not override hard backend limits or cause unsupported shader/resource combinations.

## 8. Roadmap and exit criteria

### R0 — Rendering audit and baseline

**Goal:** understand the existing renderer before introducing a competing architecture.

Work:

- trace renderer selection from startup/settings to concrete renderer construction;
- map RenderingServer, scene renderer, RenderingDevice, and backend relationships;
- identify rendering-thread ownership and synchronization boundaries;
- trace RID and GPU-resource lifetime rules;
- map shader compilation and pipeline creation;
- establish reproducible desktop, mobile, and Web build/test configurations;
- capture baseline CPU/GPU timings and memory use.

**Exit criteria:**

- documented renderer construction and frame call flow;
- verified resource and threading model;
- list of reusable engine abstractions and required extension points;
- repeatable baseline scenes and measurements;
- explicit list of unsupported or unverified platform paths.

**Do not** begin a broad renderer rewrite before R0 is complete.

### R1 — Kaizen render core

**Goal:** establish the smallest useful Kaizen-owned rendering boundary.

Work:

- define the renderer-facing contract in terms compatible with engine lifecycle and threading;
- implement capability reporting;
- establish resource ownership and deferred destruction rules;
- build a minimal render-pass scheduling model;
- add diagnostics and profiling interfaces;
- add unit/integration tests for handle validity, resource lifetime, and pass dependencies.

**Exit criteria:**

- a minimal frame can be described and executed through the new boundary;
- resource lifetime is validated under multiple frames in flight;
- capability selection is testable;
- no duplicate resource ownership or renderer-selection path is introduced unintentionally.

### R2 — Desktop forward renderer

**Goal:** produce a complete, measurable forward-rendering path on a primary desktop backend.

Work:

- camera and visibility path;
- mesh and material submission;
- depth and color passes;
- physically based metallic/roughness shading;
- directional, point, and spot lighting;
- shadow maps and cascades;
- image-based lighting;
- HDR output path where supported;
- tone mapping and color-space validation.

Start with the backend best supported by the audited engine integration. Do not implement multiple backends simultaneously unless the shared contract is already stable.

**Exit criteria:**

- representative 3D scenes render correctly;
- material and lighting reference tests pass;
- shader/pipeline caching works;
- frame timings and memory use are recorded;
- editor and runtime integration is functional.

### R3 — Mobile renderer path

**Goal:** adapt the core pipeline to constrained GPUs and thermal envelopes.

Work:

- establish device capability profiles;
- reduce render-target bandwidth and overdraw;
- limit light counts and shadow cost;
- add resolution scaling and LOD policy;
- validate texture compression and upload behavior;
- measure sustained performance, not only short benchmark peaks.

**Exit criteria:**

- tested on representative low-, mid-, and high-tier devices;
- memory and thermal behavior measured;
- quality fallback is deterministic;
- no unsupported desktop-only feature is required for a valid frame.

### R4 — Web renderer path

**Goal:** provide a dependable Web baseline.

Work:

- verify the current Web build and browser graphics path;
- target WebGL 2-compatible resource, shader, and pass behavior;
- define conservative limits for lights, shadows, render targets, and post-processing;
- test browser context loss and restoration;
- measure startup time, shader compilation, memory, and frame pacing;
- investigate WebGPU separately as a future capability.

**Exit criteria:**

- documented browser and graphics requirements;
- a representative 3D scene runs through the supported Web path;
- context-loss handling is tested;
- unsupported features fail gracefully or select a fallback.

### R5 — Advanced rendering

**Goal:** add effects only after the baseline is stable and profiled.

Suggested order:

1. SSAO;
2. improved shadow filtering and cascades;
3. screen-space reflections;
4. temporal anti-aliasing, after motion vectors and history management are correct;
5. volumetric fog;
6. global illumination, beginning with baked/probe-based options and evaluating dynamic methods separately.

Every feature must have a quality toggle, capability gate, fallback, correctness test, and measured cost.

**Exit criteria:**

- effects are independently switchable;
- no hidden dependency on unsupported backend features;
- temporal resources are invalidated correctly on camera cuts/resizes;
- measurable visual benefit justifies GPU and memory cost.

### R6 — Editor integration

**Goal:** make the renderer maintainable and usable by content creators.

Work:

- material and shader inspection;
- shader compilation diagnostics;
- render-pipeline/pass inspection;
- resource and memory diagnostics;
- viewport quality controls;
- GPU timing visualization where supported;
- debug views for normals, depth, roughness, overdraw, and shadow cascades.

**Exit criteria:**

- renderer state can be inspected without a debugger;
- shader errors are actionable;
- debug views are consistent with runtime output.

### R7 — Optimization and hardening

**Goal:** improve measured performance and production reliability.

Work:

- pipeline cache persistence and invalidation;
- resource pooling and transient allocation reuse;
- instancing and draw submission optimization;
- culling and LOD refinement;
- asynchronous resource upload;
- shader variant reduction;
- frame pacing and synchronization analysis;
- regression benchmarks per backend and quality tier.

**Exit criteria:**

- repeatable benchmark suite;
- no unexplained regressions in frame time or memory;
- stress tests cover resource churn and renderer restart;
- platform-specific fallbacks are documented.

## 9. Performance methodology

Do not treat the following values as guaranteed hardware targets. They are initial frame-budget references to guide profiling:

| Profile | Frame budget |
|---|---:|
| 60 FPS | 16.67 ms |
| 30 FPS | 33.33 ms |
| 120 FPS | 8.33 ms |

Measure CPU and GPU independently. A frame that meets its CPU budget but misses its GPU budget is not optimized; neither is a fast GPU frame blocked by render-thread submission.

For every benchmark, record:

- CPU, render-thread, and GPU frame times;
- resolution and render scale;
- scene complexity and visible object count;
- draw calls and pipeline changes;
- shader compilation and cache warm-up state;
- VRAM/GPU memory and process memory;
- texture upload and streaming activity;
- backend, driver, device, OS, and build configuration;
- quality tier and enabled effects.

Use repeatable scenes and warm/cold runs. Report median and high-percentile frame times rather than relying only on average FPS.

## 10. Testing strategy

### Correctness

- reference scenes for material response, lighting, shadows, and color output;
- image comparisons with documented tolerance;
- resource lifetime and synchronization stress tests;
- shader compile tests per supported backend;
- resize, device restart, and context-loss tests where applicable.

### Performance

- fixed benchmark scenes;
- cold and warm shader/pipeline cache runs;
- low-, mid-, and high-capability device profiles;
- long-running mobile thermal tests;
- Web startup and steady-state measurements.

### Compatibility

- editor viewport and runtime rendering;
- project load/save and scene reload;
- export template behavior;
- feature fallback when capabilities are absent.

## 11. Suggested source organization

The following is a **conceptual** organization for Kaizen-owned code. It must not be created wholesale before R0 determines the correct integration points. Prefer extending existing engine boundaries over duplicating them.

```text
servers/rendering/
  kaizen/
    core/             # frame lifecycle, capabilities, shared contracts
    graph/            # pass declarations, dependency validation
    resources/        # logical resource tracking and lifetime policy
    shaders/          # variants, reflection metadata, pipeline keys
    visibility/       # culling and draw-list construction
    materials/        # Kaizen-owned material policies
    lighting/         # lighting model and light selection
    shadows/          # shadow policy and algorithms
    post_processing/  # optional post-processing effects
    profiling/        # counters, timings, diagnostics
    backends/         # only if a separate backend boundary is justified
```

Do not create a second Vulkan/Metal/OpenGL driver layer. Backend code should reuse the engine's supported graphics API integration wherever practical.

## 12. Risks and mitigations

| Risk | Mitigation |
|---|---|
| Duplicating renderer and resource ownership | Complete R0 call-flow and lifetime audit before R1. |
| Breaking editor/runtime integration | Keep engine lifecycle and RenderingServer contracts explicit; test both modes. |
| Desktop assumptions leaking into mobile/Web | Capability-driven feature selection and platform CI/build checks. |
| Unbounded shader variants | Deterministic keys, variant budgets, cache metrics, and compile tests. |
| GPU resource use-after-free | Deferred destruction tied to actual in-flight work and synchronization. |
| Feature scope growing before baseline quality | Complete a coherent forward renderer before advanced effects. |
| Optimizing the wrong bottleneck | Capture repeatable CPU/GPU and memory baselines before optimization. |
| Mistaking inherited features for Kaizen work | Label inherited, adapted, Kaizen-specific, and planned work in code and docs. |

## 13. Immediate implementation order

The next engineering task is **R0**, not implementation of a new renderer directory.

Recommended audit order:

1. `servers/rendering/rendering_server_default.cpp` — identify initialization, renderer selection, and dispatch.
2. `servers/rendering/renderer_rd/renderer_scene_render_rd.h` and `.cpp` — inspect scene-renderer responsibilities and lifecycle.
3. `servers/rendering/renderer_rd/forward_clustered/` — trace the desktop Forward+ path.
4. `servers/rendering/renderer_rd/forward_mobile/` — compare mobile-specific constraints and execution.
5. `drivers/gles3/` — establish the Compatibility path and Web relevance.
6. `drivers/vulkan/` and platform integration — identify what is already abstracted and reusable.
7. Shader compilation, pipeline caching, RID/resource ownership, and render-thread synchronization — trace their actual call sites before defining new contracts.

Deliverables from this audit should be a source-backed call graph, a resource-lifetime map, a backend capability matrix, and a benchmark baseline. Those artifacts become the implementation specification for R1.

An initial source-level audit is available in [Rendering audit (R0)](rendering-audit.md). It covers renderer selection, initialization and ownership, the threading model, resource-lifetime entry points, the shader pipeline, and the existing `RenderingDeviceGraph`.

Follow-up documents complete the source-level part of R0:

- [Rendering resource lifetime](rendering-resource-lifetime.md) — the resource-lifetime map required by R1: RID → renderer storage → `RenderingDevice` object → deferred disposal on frame-ring reuse, including the fence wait and disposal order.
- [Resource lifetime diagnostics](resource-diagnostics.md) — implementation status for the first R1 deliverable: generation handles, `Alive`/`PendingRelease`/`Released` accounting, runtime churn evidence, and the remaining gaps before pipeline eviction.
- [Pipeline object lifetime and eviction](pipeline-lifetime.md) — the P2 registry core: per-queue reference counting, LRU budget, completion-serial retirement, registry and device-level tests, opt-in deferred destruction in the Vulkan driver, and the remaining creation-side eviction work.
- [Pipeline cache and shader variants](rendering-pipeline-cache.md) — variant-table keys, device PSO cache persistence per backend, and the invalidation rules R1 must preserve.
- [Web export runtime](rendering-web-runtime.md) — the Web path is pinned to the Compatibility renderer and cannot use `RenderingDevice`; it also lacks any shader or pipeline cache, which constrains how a future renderer can target the browser.
- [Metal rendering driver](rendering-metal-driver.md) — the Metal backend already implements the RD contract (argument buffers, heaps, barriers, MetalFX) but has no cross-run PSO cache and bakes shader libraries only when exporting from an arm64 Mac.
- [Benchmark scene specification](rendering-benchmark-scenes.md) — the implementable definition of scenes S1–S7: fixed content and parameters, frame-indexed camera contract, quality presets, cell matrix, sampling protocol, harness contract, and the validity gate that decides whether a measured cell may be published.

Runtime baselines remain open. The measurement plan — instruments, scene set, reference configurations, procedure, and report template — is specified in [Rendering benchmark methodology](rendering-benchmarks.md), and the scene set is defined in [Benchmark scene specification](rendering-benchmark-scenes.md). The S1–S4 and S6 scenes, the S7 manual editor procedure, and the sampling harness are implemented in `benchmarks/rd_3d/` (see [benchmark projects](../../benchmarks/README.md)). Five D1 sessions have been validated and published: the S1 400-box cell in [`benchmarks/results/2026-11-19_s1_d1_q0/`](../../benchmarks/results/2026-11-19_s1_d1_q0/README.md), S2, S3a, S3b and six S4 light cells in [`benchmarks/results/2026-11-19_d1/`](../../benchmarks/results/2026-11-19_d1/README.md), and S6 at 1, 4 and 16 quads for post presets P0–P5 in the three `2026-11-19_s6_d1_*` sessions. D2 (Mobile) and D3 (Compatibility) template batches are also published: [`benchmarks/results/2026-11-19_1252_d2_mobile/`](../../benchmarks/results/2026-11-19_1252_d2_mobile/README.md) (7 valid cells, 6 not-applicable) and [`benchmarks/results/2026-11-19_1329_d3_compat/`](../../benchmarks/results/2026-11-19_1329_d3_compat/README.md) (9 valid cells, 3 not-applicable). S5 (2D/Canvas) is now authored in `benchmarks/rd_2d/` and measured: 12 cells in [`benchmarks/results/2026-11-19_1414_s5_2d/`](../../benchmarks/results/2026-11-19_1414_s5_2d/README.md). Every other number is unmeasured, and no other run has passed the gate.
