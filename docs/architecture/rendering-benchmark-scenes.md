# Benchmark Scene Specification (R0 follow-up)

> **Status:** Specification — **S1–S4 and S6 authored and executed once in a tools build; S5 not written; S7 is a manual procedure; no cell has passed the validity gate** (see the [run log](rendering-benchmark-run-log.md))
> **Verified against revision:** `f67b0df` (2026-11-18)
> **Related:** [Benchmark methodology](rendering-benchmarks.md), [Rendering audit (R0)](rendering-audit.md), [Rendering roadmap](rendering-roadmap.md), [Web export runtime](rendering-web-runtime.md), [Metal rendering driver](rendering-metal-driver.md)

This document turns the scene set S1–S7 from the [benchmark methodology](rendering-benchmarks.md#4-benchmark-scene-set) into an implementable specification: fixed content, fixed camera paths, fixed project settings, parameter sweeps, and acceptance criteria. It contains **no performance numbers**. Every count below is a proposed parameter value, not a measurement.

The goal is comparability. A result is only meaningful if the frame content, resolution, quality settings, and camera position at frame *n* are identical across configurations. That requires the scene to be deterministic and independent of frame rate.

## 1. Global rules

1. **One project per scene group.** S1–S4 and S6 share a 3D project; S5 is a 2D project; S7 reuses the 3D project in the editor. Each project pins every rendering setting in `project.kaizen`; nothing may depend on engine defaults, because defaults change between commits.
2. **Content is generated, not imported.** Meshes and textures are produced at startup by a deterministic generator (fixed seed, integer hashing, no `randf()`), so the repository stays asset-free and the same content is used on every backend. Generation cost is first-frame only and excluded from sampling (§5).
3. **Animation is frame-indexed, not time-indexed.** Camera and object movement advance by rendered frame index, never by `delta`. A 300-frame loop therefore renders the same 300 images on a 30 FPS run and a 300 FPS run.
4. **No simulation.** No physics bodies, no navigation, no particles, no animation players, no scripts beyond the camera driver and the measurement harness.
5. **Resolution is fixed per configuration.** 1920×1080 for D1–D3, W1–W2, M1–M2. A1 uses the device panel resolution. B1 records the CSS canvas size and `devicePixelRatio`. Any deviation is recorded in the report.
6. **`--headless` is forbidden** for any published number (`main/main.cpp:573`): it bypasses presentation, so frame pacing is not representative.
7. **Editor and template runs are separate tables.** The editor forces rendering onto the main thread (`main/main.cpp:2793-2796`), so S7 can never be compared with template results.
8. **Each cell records a content hash**: SHA-256 over commit, project settings, scene parameters, and preset. A cell without a hash is not comparable.

## 2. Shared baseline

### 2.1 Project settings

Pinned for every 3D scene unless the scene says otherwise:

| Setting | Value | Why |
|---|---|---|
| `rendering/renderer/rendering_method` | per configuration | `forward_plus`, `mobile`, or `gl_compatibility` |
| `rendering/rendering_device/driver` | per configuration | `vulkan`, `d3d12`, or `metal`; ignored by Compatibility |
| `display/window/size/viewport_width` / `viewport_height` | `1920` / `1080` | fixed sampling surface |
| `display/window/vsync/vsync_mode` | `0` (disabled) | driver-level vsync must also be disabled manually; `--disable-vsync` cannot override it (`main/main.cpp:629`) |
| `rendering/anti_aliasing/quality/msaa_3d` | `0` | baseline is no MSAA (`servers/rendering/rendering_server.cpp:3777`) |
| `rendering/anti_aliasing/quality/msaa_2d` | `0` | same |
| `rendering/scaling_3d/mode` | `0` (bilinear) | no FSR/MetalFX in the baseline |
| `rendering/scaling_3d/scale` | `1.0` | native render resolution |
| `rendering/textures/default_filters/anisotropic_filtering_level` | `0` | remove filtering variance from S1–S3 |
| `rendering/lights_and_shadows/directional_shadow/size` | `4096` (`2048` with the Mobile renderer) | atlas cost must be stated |
| `rendering/lights_and_shadows/directional_shadow/soft_shadow_filter_quality` | `2` (`0` with the Mobile renderer) | same |
| `rendering/driver/depth_prepass/enable` | `true` | note that Apple/Mali/Adreno/PowerVR vendors disable it automatically (`rendering_server.cpp:3749-3750`) |

Notes that affect interpretation:

- The depth prepass is **on by default but skipped on Apple GPUs and most mobile GPUs** (`rendering/driver/depth_prepass/disable_for_vendors` defaults to `PowerVR,Mali,Adreno,Apple`, `rendering_server.cpp:3750`). A Metal-vs-Vulkan comparison therefore differs by more than the API; the report must state the effective prepass state.
- Metal has no persistent pipeline cache (see [Metal rendering driver](rendering-metal-driver.md#4-pipeline-state-cache-remains-disabled)), so its cold-run cells are not comparable to Vulkan cold runs; report them separately and never average them together.

### 2.2 Camera contract

- FOV 75°, near 0.05 m, far 500 m unless the scene overrides it.
- Motion is a closed loop of **300 frames** defined by 3–4 keyframes with linear interpolation of both position and look-at target. Keyframe `L-1` connects back to keyframe `0`. No easing, no smoothing, no randomness.
- Position at frame *n* is `t = (n % 300) / 300 * L`, `i = floor(t)`, `lerp(key[i], key[(i+1) % L], t - i)`, computed in double precision.
- The sample window must be a multiple of 300 frames so that every run covers exactly the same set of camera positions.
- A scene that needs a static camera simply uses two identical keyframes.

### 2.3 Quality presets

Two presets are measured for every 3D scene, plus scene-specific post effects in S6:

| Setting | Q0 (baseline) | Q1 (high) |
|---|---|---|
| MSAA 3D | 0 (disabled) | 4× (`msaa_3d = 2`) |
| SSAO | off | Medium |
| SSIL | off | Medium |
| Glow | off | on, intensity 0.8 |
| Volumetric fog | off | off (S6 only, Forward+ only) |
| Directional shadow size | 4096 | 4096 |
| Soft shadow quality | 2 | 4 |
| Depth prepass | `true` (engine may disable it per vendor) | `true` (same) |

Q0 answers "what does the renderer cost with the least work"; Q1 answers "what does a typical high-quality frame cost". Both are required; a report with only one preset is incomplete.

When the Mobile renderer is active, its `.mobile` project-setting overrides take precedence over the shared value (`rendering/lights_and_shadows/directional_shadow/size.mobile`, `.../soft_shadow_filter_quality.mobile`, shown in `servers/rendering/rendering_server.cpp:3706-3708`). The report records the **effective** value, not the requested one.

### 2.4 Render scale experiments

`rendering/scaling_3d` is **not** part of S1–S7. Upscaling quality and cost (FSR 1.0, FSR 2.2, MetalFX spatial/temporal) is a separate experiment because it changes the internal resolution and needs its own image-quality criteria. On macOS the mode list adds MetalFX spatial and temporal (`rendering_server.cpp:3791-3793`); a Metal run must record which mode was selected.

## 3. Scene definitions

Every scene below lists: purpose, content, parameters and default, camera, lighting/environment, acceptance criteria, and explicit non-goals.

### S1 — Static 3D

> Implemented in `benchmarks/rd_3d/` (`scenes/s1_static.tscn`). See [benchmark projects](../benchmarks/README.md) for how to run it. Executed once in a tools build on RADV/Forward+; that run is invalid by design, and the observations are in the [run log](rendering-benchmark-run-log.md).

**Purpose:** baseline geometry and material throughput with the least possible per-frame variety. This is the reference point every other scene is normalized against. On Forward+ it is a throughput baseline, not a per-draw probe: identical surfaces are merged into instanced draws automatically (see the acceptance criteria).

**Content**

- Ground plane: 400×400 m, 2 triangles, same material as the boxes.
- Boxes: grid of `N` boxes, 2×2×2 m, spacing 8 m, one `BoxMesh` shared across all instances but **separate `MeshInstance3D` nodes** (no MultiMesh — that is S2). Each box: 12 triangles.
- Material: one `StandardMaterial3D`, albedo from a generated 1024² checker texture with mipmaps, roughness 0.6, metallic 0.0. No normal map, no emission.

**Parameters**

| Parameter | Default | Sweep |
|---|---|---|
| `box_count` | 400 (20×20) | 100, 400, 1600 |

**Camera:** orbit around the field center at radius `2.2·S` and height `0.9·S`, where `S ≈ 4·sqrt(box_count)` is the field half-extent; the camera looks at the field center for the whole 300-frame loop, so all boxes remain in frustum and draw calls are not reduced by culling. Far plane `4·S + 200`.

**Lighting/environment:** one `DirectionalLight3D`, energy 1.0, **shadows off**; environment background = solid color `(0.05, 0.05, 0.07)`, ambient light from color, energy 0.3; no fog, no glow, no SSAO.

**Acceptance criteria**

- `raster/total_primitives_drawn` ≈ `12 × box_count + 2` (±1%). Primitive count does not depend on how the renderer batches.
- `raster/total_draw_calls` is **renderer-dependent and must be recorded, not asserted as a fixed number**:
  - Forward+ sorts the opaque list by material/shader/geometry key and merges **adjacent** identical surfaces into one instanced draw (`RenderElementInfo::repeat`, `MAX_REPEATS = 2^20 − 1` in `forward_clustered/render_forward_clustered.h`). Batching is real but partial: only neighbours in the sorted list merge, and the sort order depends on the camera. Measured on RADV/Forward+ with one material and one shared `BoxMesh`: 2, 5, 4, 5, 6 and 7 draw calls for 1, 64, 65, 128, 256 and 400 boxes. The count is not monotonic and not predictable in advance, which is exactly why the criterion is stability, not a target.
  - Mobile reports one draw call per instance (`forward_mobile/render_forward_mobile.cpp` sets the counter from `instances->size()`), so the same scene reports ≈ `box_count + 1`.
  - The acceptance criterion is **stability across repeated runs of the same configuration**, not a target count. A draw-call count that changes between two runs of one configuration invalidates the cell.
- No increase in `pipeline/compilations_*` during the sampling window. These counters are cumulative since startup, so the check is on the per-frame delta, never on the absolute value.
- The frame at index 299 is bit-identical across runs of the same configuration (screenshot hash), proving determinism.

**Non-goals:** shadows, transparency, post-processing, per-draw overhead (Forward+ batches this scene), culling stress.

---

### S2 — Instancing

> Implemented in `benchmarks/rd_3d/` (`scenes/s2_instancing.tscn`); executed once in a tools build, no publishable cell yet.

**Purpose:** draw-call scaling. Separates per-draw overhead from per-primitive cost.

**Content**

- One `SphereMesh` (`radial_segments = 32`, `rings = 16`, ≈ 1k triangles) in a single `MultiMesh` with `transform_format = 1` (3D).
- Instances placed on a square grid with fixed spacing 2.5 m, so the field half-extent is `S ≈ 1.25·sqrt(N)`; positions offset by a deterministic integer hash (≤ 10% of spacing) to avoid a perfectly regular silhouette.
- `visible_instance_count = -1`.

**Parameters**

| Parameter | Default | Sweep |
|---|---|---|
| `instance_count` | 4096 (64×64) | 1024, 4096, 16384, 65536 |

**Camera:** orbit around the field center at radius `2.2·S` and height `0.9·S`, looking at the center for the whole 300-frame loop, so every instance stays in frustum and the primitive count is not affected by culling. Far plane `4·S + 200`. Camera distance and far plane therefore change with the sweep and must be recorded.

**Lighting/environment:** identical to S1.

**Acceptance criteria**

- Draw calls stay **constant within ±1** across the whole `instance_count` sweep. A scene whose draw calls grow with instance count is a MultiMesh configuration bug and must be rejected.
- `raster/total_primitives_drawn` scales linearly with `instance_count` (±1%).
- No increase in `pipeline/compilations_*` during the sampling window.

**Optional variant S2b:** per-instance color enabled (adds color data to the instance buffer, no new PSO). Measured only if instanced color cost is a question.

**Non-goals:** culling behavior, LOD, occlusion.

---

### S3 — Material variety

**Purpose:** separate *uniform* variety from *shader/PSO* variety. These are the two failure modes behind "many materials are slow", and they have different fixes.

**Content:** the same sphere mesh as S2, placed as 64 separate `MeshInstance3D` nodes in an 8×8 grid, spacing 3 m. Each node gets its own material.

**S3a — uniform variety (default sub-scene)**

| Parameter | Default | Sweep |
|---|---|---|
| `material_count` | 64 | 1, 16, 64 |

All materials share one feature set; only albedo color, roughness, and metallic differ. Expected behavior: no additional PSOs after the first frame; cost grows only from per-material uniform/set updates and draw calls.

**S3b — feature variety**

Eight feature combinations, cycled across the 64 nodes:

1. plain PBR
2. + normal map (generated 512²)
3. + ORM texture (generated 512²)
4. + emission
5. + rim
6. + clearcoat
7. + anisotropy
8. + alpha transparency

**Camera:** static at (0, 8, 25), look-at (0, 3, 0), with a ±10 m lateral sweep over 300 frames.

**Lighting/environment:** one directional light, shadows off.

**Acceptance criteria**

- S3a: no increase in `pipeline/compilations_*` during sampling, even at `material_count = 64`.
- S3b: the number of PSO compilations in the **first 120 frames** is recorded exactly, and is stable across repeated runs of the same configuration. The report states which `pipeline/compilations_*` counter moved (`canvas`, `mesh`, `surface`, `draw`, `specialization`).
- Do **not** assume which feature maps to which PSO. Read the monitors; SPIR-V is generated per variant and the pipeline key is derived per pass.

**Non-goals:** lighting cost, texture bandwidth (textures are 512² and fit in cache).

**Implemented** in `benchmarks/rd_3d/scripts/scene_s3.gd`; `--bench-variant a|b`
selects the sub-scene, `--bench-count` sets `material_count` for S3a.

- The 64 nodes share one `SphereMesh` (32×16) and get a per-node
  `material_override`; the mesh itself carries no material.
- S3a materials are `StandardMaterial3D` with identical features. Albedo hue is
  `fmod(index × 0.618…, 1)`, saturation, roughness, and metallic come from the
  integer hash in `bench_scene_common.gd`, so material *n* is identical on every
  run and platform.
- S3b generates one 512² tangent-space normal map and one 512² packed ORM
  texture on the CPU at startup and cycles the eight features across the nodes.
  Feature 3 uses `ORMMaterial3D`: the packed `orm_texture` property is only
  registered by that class (`scene/resources/material.cpp:3654`, `:936` of
  `material.h`); `StandardMaterial3D` would read separate roughness/metallic
  textures instead.
- Adjacent-batch merging applies here too: on Forward+, `material_count = 1`
  merges all 64 identical spheres into one instanced draw, while 16 or 64
  distinct materials cannot merge. The acceptance criterion is the PSO count,
  not a fixed draw-call number.

---

### S4 — Lighting and shadows

**Purpose:** light-count scaling and shadow cost.

**Content**

- Ground 400×400 m plus 64 boxes (8×8, 4 m tall) using the S1 material.
- One `DirectionalLight3D` with shadows **on** in every cell; `directional_shadow_max_distance = 200`.
- `spot_count` spot lights (range 30 m, energy 4.0) and `omni_count` omni lights (range 20 m, energy 2.0), positioned deterministically around the field. Shadows on lights are off in the baseline and on for at most four spots in the shadow variant.

**Parameters**

| Parameter | Default | Sweep |
|---|---|---|
| `spot_count` / `omni_count` | 8 / 0 | (0,0), (8,0), (0,32), (8,32), (32,64) |
| light shadows | off | on for 4 spots |

**Camera:** orbit radius 60 m, height 25 m, look-at the field center; 300-frame loop.

**Lighting/environment:** background color, ambient energy 0.1, no fog, no GI.

**Acceptance criteria**

- Draw calls must not change with light count in a clustered renderer; record whether they do. A change indicates a fallback path and must be documented, not hidden.
- Directional shadow atlas size and soft-shadow quality are stated per cell.
- No SDFGI, VoxelGI, or volumetric fog in this scene; those are separate features with their own convergence behavior.

**Non-goals:** GI quality, volumetric lighting, decals, reflection probes.

**Implemented** in `benchmarks/rd_3d/scripts/scene_s4.gd`;
`--bench-spots` and `--bench-omnis` set the light counts and
`--bench-variant shadows` enables shadows on four spots.

- Geometry: `PlaneMesh` 400×400 for the ground and 64 `BoxMesh` nodes of
  2×4×2 m in an 8×8 grid at 6 m spacing, all using the S1 checker material. The
  spacing and box footprint are authored choices; the specification fixes only
  the count, the 4 m height, and the material.
- The directional light always has shadows on, with
  `directional_shadow_max_distance = 200`.
- Spot lights sit on a 20 m ring at 12 m height, aimed at the field centre;
  range 30 m, energy 4.0, 50° cone. Omni lights sit on a 30 m ring at
  alternating 6 m and 9 m heights; range 20 m, energy 2.0. Both rings are
  placed by `index × TAU / count`, so the layout is identical on every run.
- The shadow variant enables `shadow_enabled` on the first
  `min(4, spot_count)` spots and records that number in `scene.parameters`.
- Ambient energy is 0.1, background colour, no fog, no GI.

The harness records `draw_calls` per frame. Collect the (0,0), (8,0), (0,32),
(8,32), and (32,64) cells of one configuration and compare them: in a clustered
renderer the count should not change with light count. A change is a finding to
document, not something to hide.

---

### S5 — 2D / Canvas

**Purpose:** canvas renderer cost, and the scene most likely to behave identically on the Compatibility/Web backend because it uses no 3D features.

**Content**

- 2D scene, no `Camera3D`.
- `sprite_count` `Sprite2D` nodes on a grid, all using one generated 256×256 texture and the default canvas material (batching path).
- `label_count` `Label` nodes, fixed string `"Kaizen benchmark 0123456789"`, font size 16.
- `layer_count` `CanvasLayer` nodes; the first contains the sprites, the others contain a subset of labels.

**Parameters**

| Parameter | Default | Sweep |
|---|---|---|
| `sprite_count` | 2000 | 500, 2000, 8000 |
| `label_count` | 200 | 0, 200 |
| `layer_count` | 1 | 1, 5 |

**Camera/animation:** the root `CanvasLayer` offset pans horizontally over a 300-frame loop; sprites are not rotated.

**Acceptance criteria**

- Draw calls are recorded exactly; with batching they should not grow linearly with `sprite_count`. If they do, record it as a batching limitation rather than adjusting the scene.
- `video/texture_mem` stays constant across the sweep (one shared texture).
- This scene is the primary B1 (browser) cell: it avoids 3D features that Compatibility may emulate differently.

**Non-goals:** 3D content, custom canvas shaders, text shaping stress (the string is fixed ASCII).

---

### S6 — Transparency and post-processing

**Purpose:** overdraw and post-effect cost, which dominate many real scenes and are invisible in S1–S4.

**Content**

- Base: ground plus 32 opaque boxes (S1 material), one directional light, shadows off.
- `quad_count` transparent quads, 40×40 m, alpha 0.35, perpendicular to the view axis and facing the camera, stacked from z = 10 m to z = 25 m in 1 m steps. Each covers most of the viewport, so `quad_count` is a direct overdraw multiplier (1×, 4×, 16×).
- One `WorldEnvironment`; only the toggles listed below change between cells.

**Parameters**

| Parameter | Default | Sweep |
|---|---|---|
| `quad_count` | 4 | 1, 4, 16 |
| post preset | `P0` | `P0`–`P5` |

Post presets:

| ID | Glow | SSAO | SSIL | Volumetric fog |
|---|---|---|---|---|
| P0 | off | off | off | off |
| P1 | on | off | off | off |
| P2 | off | on | off | off |
| P3 | off | on | on | off |
| P4 | on | on | on | off |
| P5 | on | on | on | on (Forward+ only) |

Fixed values: glow intensity 0.8, bloom 0.1, HDR threshold 1.0; SSAO radius 1.0, intensity 2.0, power 1.5, detail 0.5, quality Medium (`rendering/environment/ssao/quality = 2`); SSIL quality Medium (`rendering/environment/ssil/quality = 2`); volumetric fog density 0.01, length 64, detail spread 1.5.

**Camera:** static at (0, 8, 40), look-at (0, 2, 0), two identical keyframes.

**Acceptance criteria**

- Each post cell is verified to actually change the image: one reference screenshot per (preset, configuration) is diffed against P0 and the mean absolute difference is recorded. A difference of **exactly zero** is invalid: it means the effect did not run. A small non-zero difference is recorded as measured, not judged: on the reference run SSAO changed 0.35% of the pixels by one level while still costing about 1 ms, because it modulates only the ambient term.
- Volumetric fog, SSAO, and SSIL are Forward+ features. The Mobile backend stubs
  the SSAO/SSIL quality setters and reports no volumetric support, so a preset
  that asks for them there is published as **"not applicable"**, never as "0 ms".
  A screenshot is still archived.
- The harness turns an unsupported request into `valid = false` with a reason
  naming the effect, so such a run cannot be published by accident. That is a
  classification of the cell, not a verdict that the effect failed: "invalid" in
  the report means "do not publish this number", and the collector records the
  cell as not applicable.
- Transparent quads must be sorted stably; if the order changes between runs, the frame hash check will fail and the scene must be fixed.

**Non-goals:** TAA, upscaling, depth of field, motion blur. Those are separate image-quality experiments with their own criteria.

**Implemented** in `benchmarks/rd_3d/scripts/scene_s6.gd`; `--bench-count`
sets `quad_count` and `--bench-preset P0..P5` applies the post preset.

- Base geometry: the 400×400 ground and 32 `BoxMesh` nodes of 2×2×2 m in an 8×4
  grid at 6 m spacing, using the S1 checker material. Count and material are
  fixed by the specification; the grid layout is an authored choice.
- Quads: one shared `QuadMesh` of 40×40 m, `Transparency = Alpha`, albedo 0.35,
  unshaded, culling disabled. Each quad is centred on the camera's view axis and
  oriented so its +Z face points at the camera, stacked at `z = 10 + index` m.
  They share one mesh and one material, so the only thing that changes with
  `quad_count` is overdraw.
- Environment: the shared solid background and ambient energy 0.3. SSAO and
  SSIL quality are pinned in `project.kaizen` to `2` (Medium), and the report
  records those project settings.
- `scene.parameters.post` states the preset, which toggles were applied, and a
  `support` map for glow, SSAO, SSIL, and volumetric fog derived from
  `RenderingServer.get_current_rendering_method()`. The scene exposes
  `unmet_requirements()`, and the harness records any unsupported request as an
  invalid reason, so the cell is reported as **not applicable**, not "0 ms".
- `--bench-screenshot <path>` writes the frame-299 image as PNG.
  `--bench-reference <png>` compares the captured frame against that image and
  writes `image_diff.mean_abs_diff` over every RGBA byte (0–255), plus the
  maximum byte difference and the image size. The comparison runs once, after
  sampling. `--bench-min-diff <f>` marks the run invalid when the mean falls
  below `f`, which is how "the effect silently did not run" is caught.
- The diff covers the whole frame, including the opaque base, so it answers
  "did this preset change the image", not "is the effect good".

---

### S7 — Editor viewport

**Purpose:** the editor's own rendering path, which differs from a game by thread model, viewport composition, and overlays.

**Content:** the S1 project at `benchmarks/rd_3d`, `box_count = 400`, opened in
the editor with `scenes/s1_static.tscn` as the current scene. This is a manual
procedure, not a harness scene: the steps are in
[benchmark projects](../benchmarks/README.md).

**Cells**

| Cell | Layout |
|---|---|
| S7a | single 3D viewport, maximized |
| S7b | four 3D viewports (2×2), same scene |

**Procedure notes**

- Run the editor binary, not a template: `kaizen --editor --path <project>`.
- `--print-fps` prints `Editor FPS:` in editor runs (`main/main.cpp:5174-5178`); those numbers are never merged with template numbers.
- The editor forces `separate_thread_render = 0` (`main/main.cpp:2793-2796`), so the render thread model differs from any template run; this must be stated in the report.
- Record whether the Scene dock preview, the inspector, and the grid are visible; the 3D editor grid and gizmos are part of the measured frame and cannot be excluded.
- Use the editor's frame profiler for per-pass GPU attribution; `--gpu-profile` applies to non-editor output.

**Acceptance criteria**

- Screenshot archived with the viewport layout; viewport size in pixels recorded.
- Same commit, same project, same asset state as the template S1 cell; otherwise the pair is not comparable.
- Editor results are reported as "editor", never as engine performance.

**Non-goals:** scripting editor, importing assets, opening multiple scenes.

## 4. Cell matrix

Each cell is one (scene, parameters, configuration, preset) combination. Minimum set for a first baseline:

| Scene | Parameter cells | Presets | Configurations | Cells |
|---|---|---|---|---|
| S1 | 3 `box_count` | Q0, Q1 | D1, D2, D3, W1, W2, M1, A1, B1* | 45 |
| S2 | 4 `instance_count` | Q0, Q1 | same | 60 |
| S3a | 3 `material_count` | Q0 | same | 24 |
| S3b | 1 feature set | Q0 | same | 8 |
| S4 | 5 light cells + shadow variant | Q0, Q1 | D1, D2, M1 | 36 |
| S5 | 3×2×2 combinations | Q0 | D1, D3, A1, B1 | 48 |
| S6 | 3 overdraw × 6 presets | — | D1, D2**, M1, W2 | 60 |
| S7 | 2 layouts | Q0 | D1, M1 | 4 |

** S6 adds six cells on D2, the Mobile backend, with P0 and P1 only. That
backend stubs SSAO and SSIL and reports no volumetric support, so P2–P5 there
are "not applicable": the harness marks them invalid and the collector records
the cell as not applicable. Running these cells is how the applicability rule
itself is tested instead of assumed.

\* B1 uses only the Compatibility renderer; Q1 sub-cases that require an unsupported feature are reported as "not applicable" rather than silently omitted. S1 and S2 totals already exclude the three/four B1 Q1 cells.

This matrix is deliberately larger than a first pass can execute. The recommended order is: **S1 on D1 and M1 at Q0**, then S2, then S5/B1, then the rest. Early cells validate the harness and the scene contract; there is no value in measuring 200 cells with a broken camera path.

## 5. Sampling protocol

1. **Generation:** build scene content once, before frame 0. First-frame generation time is recorded but excluded from steady-state statistics.
2. **Warm-up:** a fixed 900 frames (3 camera loops), rounded up to a multiple of 300. The start is fixed rather than adaptive so that every run samples the same frame indices; an adaptive start would make runs incomparable. Compilations are expected to stop before frame 900; if any occur within the last 120 warm-up frames, the cell is invalid.
3. **Sampling:** exactly 3600 frames (12 loops of the 300-frame camera path), starting at frame 900. If the machine cannot sustain 60 FPS, still sample 3600 frames and report the longer wall time.
4. **Per-frame record:** frame index, frame time (µs, monotonic clock), `Performance` snapshot, and viewport CPU/GPU render time when available.
5. **Repetitions:** 5 runs per cell; report median, p95, p99, min, max, and the between-run spread. Raw per-run values are published alongside the summary.
6. **Cold run:** one additional run per cell with the pipeline cache deleted (`user://vulkan/pipelines.*.cache`), reporting time-to-first-steady-frame and the number of PSO compilations. On Metal this always behaves like a cold run because no cache is written.
7. **Frame-hash check:** the screenshot at frame 299 is hashed; all repetitions of a cell must produce the same hash.

Hitches are defined as frames whose time exceeds twice the run median. The count and the worst frame are reported; a run with hitches inside the sampling window is still valid but is flagged.

## 6. Harness contract (implemented, executed once in a tools build)

The harness is a GDScript autoload that runs only in template builds (`OS.has_feature("template")`), so it cannot accidentally be measured from the editor. A tools build can be used for scene development with `--bench-allow-editor`, which marks the report invalid. The project starts on an empty bootstrap scene; the harness loads the scene named by `--bench-scene` from its internal registry.

Invocation:

```text
kaizen --path benchmarks/rd_3d -- --bench-scene S1 --bench-count 400 --bench-preset Q0 \
       --bench-config D1 --bench-frames 3600 --bench-warmup 900 \
       --bench-out /abs/path/S1_D1_Q0.json
```

`--bench-variant a|b` selects the S3 sub-scene (`shadows` for S4); `--bench-spots` and `--bench-omnis` set the S4 light counts.

User arguments after `--` are read with `OS.get_cmdline_user_args()`.

Per-frame data comes from:

- `Time.get_ticks_usec()` for wall-clock frame time.
- `Performance.get_monitor(Performance.TIME_PROCESS)` and `TIME_PHYSICS_PROCESS`.
- `Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME)`, `RENDER_TOTAL_OBJECTS_IN_FRAME`, `RENDER_TOTAL_PRIMITIVES_IN_FRAME`.
- `Performance.get_monitor(Performance.RENDER_TEXTURE_MEM_USED)`, `RENDER_BUFFER_MEM_USED`, `RENDER_VIDEO_MEM_USED`.
- `Performance.get_monitor(Performance.PIPELINE_COMPILATIONS_*)` for all five counters.
- `RenderingServer.viewport_set_measure_render_time(viewport_rid, true)` followed by `viewport_get_measured_render_time_cpu/gpu` (`servers/rendering/rendering_server.cpp:2932-2935`).

Scene contract: a scene loaded by the harness must expose
`apply_frame(frame_index)`, and should expose `scene_parameters()` so the report
carries the content that actually ran. A scene may also expose
`unmet_requirements()`, a list of reasons why the active renderer cannot provide
what was requested; the harness records each one as an invalid reason.

Constraints on the harness itself:

- Preallocate the sample buffers; do not allocate, create nodes, or load resources during sampling.
- Do not call `Performance.add_custom_monitor` inside the measured loop (the callable would execute every frame and pollute the result).
- Write the JSON once, after sampling.
- Read the monitor array as a whole; per-frame `get_monitor` calls have a measurable cost that must be identical across configurations. The harness cost is part of every cell and is disclosed, not subtracted.

Output (one JSON document per run, abbreviated here; the implementation in `benchmarks/rd_3d/scripts/bench_harness.gd` is authoritative):

```json
{
  "schema": 1,
  "engine": { "major": 4, "minor": 8, "patch": 0, "status": "beta", "hash": "..." },
  "build": { "type": "template", "flags": ["..."], "user_args": ["--bench-scene", "S1"] },
  "scene": {
    "id": "S1",
    "parameters": { "box_count": 400, "...": "..." },
    "content_hash": "..."
  },
  "preset": "Q0",
  "configuration": "D1",
  "valid": true,
  "invalid_reason": "",
  "project_settings": { "...": "..." },
  "runtime": {
    "rendering_method": "forward_plus",
    "rendering_driver": "vulkan",
    "display_server": "x11",
    "vsync_mode": 0,
    "msaa_3d": 0,
    "use_taa": false,
    "scaling_3d_mode": 0,
    "scaling_3d_scale": 1.0,
    "adapter_name": "...",
    "adapter_vendor": "...",
    "api_version": "...",
    "window_size": [1920, 1080]
  },
  "timing": {
    "first_frame": 900,
    "last_frame": 4499,
    "warmup_frames": 900,
    "sampled_frames": 3600,
    "pso_at_start": 0,
    "pso_in_sample": 0,
    "last_compile_frame": -1
  },
  "pipeline_compilations": {
    "first_120": { "canvas": 0, "mesh": 0, "surface": 0, "draw": 0, "specialization": 0 },
    "total": { "canvas": 0, "mesh": 0, "surface": 0, "draw": 0, "specialization": 0 }
  },
  "frames": {
    "frame_us": [0],
    "draw_calls": [0],
    "primitives": [0],
    "pso_compilations": [0],
    "viewport_cpu_ms": [0.0],
    "viewport_gpu_ms": []
  },
  "summary": {
    "frames": 3600,
    "fps_median": 0.0,
    "frame_ms_median": 0.0,
    "frame_ms_p95": 0.0,
    "frame_ms_p99": 0.0,
    "hitches": 0,
    "gpu_ms_median": null,
    "render_cpu_ms_median": null
  },
  "frame_hash": "...",
  "screenshot": "",
  "image_diff": {},
  "notes": ["..."]
}
```

`null` for GPU time means "not available on this backend", never zero and never an estimate.

`screenshot` is the absolute path of the PNG written by `--bench-screenshot`
(empty when not requested). `image_diff` is `{}` unless `--bench-reference` was
used, in which case it contains `reference`, `width`, `height`,
`mean_abs_diff`, and `max_abs_diff`.

`pipeline_compilations.first_120` is measured from **before the scene is loaded**
to the end of the first 120 frames the harness renders, so it contains the
compiles caused by that scene's materials and first draw. It is the S3b
instrument. `total` starts when the scene is current and therefore includes the
warm-up; it is not expected to be zero in a valid run. Steady state is
`timing.pso_in_sample`, which counts the sampling window only.

The window is computed from frame index, not wall-clock time, so it contains the
same frames on every run.

The frame-hash capture holds the camera for one extra render of frame 299, then resumes the sequence. That render happens before warm-up and is neither counted nor sampled.

## 7. Validity gate

`benchmarks/tools/check_report.py` implements the mechanical part of this gate
over the JSON reports, including the S6 image-difference rule and equality of
`frame_hash` across repeated runs of one cell. The remaining checks — vsync
forced by the driver, thermal throttling, wrong content on screen — stay manual.

A cell is **invalid** and must be re-run if any of these is true:

- Content hash or frame hash differs from the cell's reference.
- `pipeline/compilations_*` increased during the sampling window (the counters are cumulative, so compare deltas, not absolute values).
- The run used `--headless`, an editor build in a template cell, or a resolution other than the recorded one.
- Driver-level vsync was active (FPS pinned at a refresh-rate divisor with a flat frame-time histogram).
- A post-effect cell that is supposed to run produces no image difference from P0. Backends that cannot provide the effect are classified as "not applicable" through the harness's unsupported-request reason, not by accepting a zero difference.
- The device thermally throttled during the run (visible as a monotonic frame-time rise; mobile runs must report cold/warm state).
- The backend reported "GPU time not available" and the report instead shows a CPU-derived GPU estimate.

## 8. Open items

- Scene authoring — **S1–S4 and S6 authored** in `benchmarks/rd_3d/` (see [benchmark projects](../benchmarks/README.md)); S5 **not written**; S7 is a **documented manual procedure** with no harness scene.
- Harness implementation — **authored and executed** (`benchmarks/rd_3d/scripts/bench_harness.gd`); every run so far fails the gate in §7 (tools build, clamped window), and the [run log](rendering-benchmark-run-log.md) records what was observed. No cell has been validated for publication.
- All counts in this document — **proposed**, not validated against real hardware.
- Whether procedural content generation produces identical meshes on all backends — **not verified** (floating-point differences in normal/tangent generation are possible; the frame-hash check will expose them).
- S3b feature-to-PSO mapping — **not verified**; must be read from monitors.
- Web cell specifics: canvas size, `devicePixelRatio`, and browser compositor behavior — **not verified** (see [Web export runtime](rendering-web-runtime.md)).
- Metal cells: `.metallib` presence depends on where the export was produced, and PSO cache is disabled — **must be recorded per run** (see [Metal rendering driver](rendering-metal-driver.md)).
- Upscaling (FSR 1.0/2.2, MetalFX) — **out of scope**, needs a separate image-quality specification.
