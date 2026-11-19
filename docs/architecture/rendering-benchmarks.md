# Rendering Benchmark Methodology (R0 follow-up)

> **Status:** Methodology specification — **twenty-eight cells measured** (five D1 sessions), the rest unmeasured
> **Verified against revision:** `27a28c1` (2026-11-18)
> **Related:** [Rendering audit (R0)](rendering-audit.md), [Rendering roadmap](rendering-roadmap.md), [Pipeline cache](rendering-pipeline-cache.md)

This document defines **how** Kaizen rendering performance will be measured, so that R1 and later milestones can be evaluated against a repeatable baseline. It intentionally contains **no numbers** of its own: the one measured cell lives in the [run log](rendering-benchmark-run-log.md) and its [reports](../../benchmarks/results/2026-11-19_s1_d1_q0/README.md), and every field without a run stays empty.

Nothing here changes engine code. It records which instrumentation already exists in the source and what must be authored (scenes, harness, report data).

## 1. What must be measured

| Question | Metric | Primary instrument |
|---|---|---|
| Frame rate / frame time | FPS, mspf | `--print-fps` |
| CPU frame cost | frame interval, renderer viewport CPU time | `frame_us`, `viewport_get_measured_render_time_cpu` |
| GPU frame cost | GPU milliseconds per pass | `--gpu-profile` / frame profiler |
| Scene complexity | draw calls, objects, primitives | `Performance` monitors |
| Memory | static memory, texture/buffer memory | `Performance` monitors |
| Pipeline churn | PSO compilations per frame | `Performance` pipeline monitors |
| Hitches | frames over a threshold, p95/p99 | Frame-time log |
| Startup cost | section timings | `--benchmark` |

## 2. Instruments available in the source

### 2.1 FPS and milliseconds per frame

- CLI `--print-fps` sets `print_fps`, which prints `Project FPS: <fps> (<mspf> mspf)` once per second. The one-second interval is an accumulator in microseconds (`frame += ticks_elapsed`, reset at `frame > 1000000`), and the first three prints after startup are suppressed (`main.cpp:4950`, `:5170-5183`).
- The same output can be enabled with the project setting `debug/settings/stdout/print_fps` (`main/main.cpp:2263`).
- In editor/project-manager runs the line is prefixed `Editor FPS:` (`main.cpp:5175-5178`); editor numbers must never be mixed with exported-game numbers.

### 2.2 GPU profile per pass

- CLI `--gpu-profile` sets `profile_gpu` (`main.cpp:1957-1958`); for non-editor runs the project setting `debug/settings/stdout/print_gpu_profile` does the same (`main.cpp:2264`, `:3616-3617`).
- It calls `RenderingServerDefault::set_print_gpu_profile()`, which enables timestamp capture (`servers/rendering/rendering_server_default.cpp:374-377`) and prints average GPU milliseconds per named task once per second (`rendering_server_default.cpp:177-215`).
- Timestamps come from `Utilities::capture_timestamp()`; the RD backend uses `RenderingDevice` timestamp queries, the GLES3 backend uses `GL_TIMESTAMP` queries **only when running desktop OpenGL** (`drivers/gles3/storage/utilities.cpp:351-355`). On real GLES/WebGL targets these queries are skipped, so GPU timings are unavailable there and must be reported as such.
- On Metal, timestamp queries are only available in barrier synchronization mode and only when the device supports counter sampling at stage boundaries (`drivers/metal/rendering_device_driver_metal3.cpp:90-101`, `:284-320`). A Metal run may therefore have no GPU profile at all; record that instead of substituting an estimate.
- The editor's remote frame profiler drives the same capture through `set_frame_profiling_enabled()` and reads `get_frame_profile()` (`servers/debugger/servers_debugger.cpp:374-393`), so its per-pass data is the same data `--gpu-profile` prints.

### 2.3 Per-viewport render time

- `RenderingServer.viewport_set_measure_render_time(viewport, true)` is exposed to scripting (`servers/rendering/rendering_server.cpp:2932`), backed by `RendererViewport::viewport_set_measure_render_time()` (`renderer_viewport.cpp:1608`).
- `viewport_get_measured_render_time_cpu()` returns milliseconds (`renderer_viewport.cpp:1615-1620`); `viewport_get_measured_render_time_gpu()` returns milliseconds computed from timestamps (`renderer_viewport.cpp:1622-1627`).
- Viewport timestamps arrive through `handle_timestamp()` with names prefixed `vp_` (`renderer_viewport.cpp:1734-1752`).

This is the correct instrument for a multi-viewport scene (split-screen, editor viewports), where global frame time hides per-viewport cost.

### 2.4 Engine monitors

`Performance` exposes the following relevant monitors (`main/performance.cpp:177-218`):

- `time/fps`, `time/process`, `time/physics_process`
- `memory/static`, `memory/static_max`
- `raster/total_draw_calls`, `raster/total_objects_drawn`, `raster/total_primitives_drawn`
- `video/texture_mem`, `video/buffer_mem`, `video/video_mem`
- `pipeline/compilations_canvas`, `compilations_mesh`, `compilations_surface`, `compilations_draw`, `compilations_specialization`

`pipeline/compilations_*` is the direct indicator of the on-demand compilation described in [pipeline cache](rendering-pipeline-cache.md): the counters are cumulative since startup, so an **increase** during steady state means the run is still creating PSOs and must not be used as a steady-state sample.

### 2.5 Startup benchmark

- `--benchmark` / `--benchmark-file <absolute path>` measure named startup and shutdown sections and dump JSON (`main/main.cpp:678-679`, `:1987-1996`; `core/os/os.cpp:771-825`).
- Measurement is compiled only with `TOOLS_ENABLED` (`os.cpp:772`, `:786`, `:795`). Exported templates therefore have no startup benchmark.
- This instrument does **not** measure frame time; it is used for shader/pipeline warm-up and load cost.

### 2.6 Sampling profilers (build-time)

`profiler=<none|tracy|perfetto|instruments>` plus `profiler_path`, `profiler_sample_callstack`, `profiler_track_memory`, `profiler_record_on_demand`, `profiler_broadcast_address` (`SConstruct:216-243`; wiring in `core/profiling/SCsub`). Zones are marked with `KaizenProfileZone*` macros (`core/profiling/profiling.h:63-80`), already present in the rendering server walk (`rendering_server_default.cpp:141-177`) and viewport pass (`renderer_viewport.cpp`).

Use a profiler build for attribution (which pass/function costs what); use the stdout instruments for the published baseline numbers, since profiler builds add overhead.

### 2.7 Determinism and override flags

- `--fixed-fps <n>` forces a fixed frame delta for reproducible simulation (`main.cpp:1928`).
- `--quit-after <n>` stops after n iterations, allowing unattended timed runs (`main.cpp:1757`).
- `--disable-vsync` overrides project vsync (`main.cpp:1948-1949`), but **does not** override driver-level vsync (`main.cpp:629`).
- `--max-fps <n>` caps frame rate (`main.cpp:1801`).
- `--rendering-method` and `--rendering-driver` override the project selection for a single run (`main.cpp:1282`, `:1290`).
- `--headless` must not be used for published frame-time results: it uses the headless display driver and does not exercise the presentation path (`main.cpp:573`), so FPS/mspf and frame pacing are not representative.

## 3. Reference configurations

Each configuration must be recorded with actual hardware before any number is published. A result without a configuration record is invalid.

| ID | Platform | Backend | Build | Required record |
|---|---|---|---|---|
| D1 | Linux x86_64 | Forward+ (Vulkan) | editor / export template | CPU, GPU, driver version, kernel, compositor |
| D2 | Linux x86_64 | Mobile (Vulkan) | export template | same as D1 |
| D3 | Linux x86_64 | Compatibility (OpenGL) | export template | same as D1 |
| W1 | Windows x86_64 | Forward+ (Vulkan) | export template | CPU, GPU, driver version, OS build |
| W2 | Windows x86_64 | Forward+ (D3D12) | export template | same as W1 |
| M1 | macOS arm64 | Forward+ (Metal) | export template | chip, unified memory, macOS version |
| M2 | macOS arm64 | Forward+ (Vulkan/MoltenVK) | export template | same as M1 |
| A1 | Android | Mobile (Vulkan) | export template | SoC, GPU, RAM, Android version, thermal state |
| B1 | Browser (WebGL 2) | Compatibility | Web export | browser + version, OS, GPU |

Configuration gaps that are already known from the source, not from measurement:

- macOS defaults to Metal (`main.cpp:2395`), and the Metal build option is disabled for non-arm64 targets (`platform/macos/detect.py:333-335`), so Intel Macs use Vulkan/MoltenVK.
- Metal has **no persistent PSO cache** (`drivers/metal/rendering_device_driver_metal.cpp:1640-1675`), so M1 pays cold compilation every run.
- Web is pinned to `gl_compatibility` (`main.cpp:2662`).

## 4. Benchmark scene set

Scenes must be authored (none exist in the repository today). Each scene is a separate project or a separate main scene with fixed settings; changing a scene invalidates comparison with earlier results. The implementable definitions — content counts, camera paths, quality presets, acceptance criteria, cell matrix and harness contract — are specified in [Benchmark scene specification](rendering-benchmark-scenes.md).

| ID | Scene | Purpose | Fixed content |
|---|---|---|---|
| S1 | Static 3D | Baseline geometry + material throughput | One camera path (or fixed camera), N meshes with one material, 1 directional light, no shadows |
| S2 | Instancing | Draw-call scaling | Same mesh via MultiMesh at several instance counts |
| S3 | Material variety | Shader/PSO variety | Same geometry with K distinct materials and parameter combinations |
| S4 | Lighting and shadows | Shadow and light cost | Directional + spot + omni lights, shadow atlas enabled |
| S5 | 2D/Canvas | Canvas renderer cost | Sprites, text, CanvasLayer overlay |
| S6 | Transparency and post | Overdraw and post-processing | Overlapping transparent quads, glow/SSAO/SSIL per quality preset |
| S7 | Editor viewport | Editor-specific path | Scene opened in the 3D editor with 1 and 4 viewports |

Rules for the scene set:

1. Content counts (mesh count, instance count, material count, lights) are parameters, not magic numbers. Counts are frozen in the report, not in the engine.
2. Presets must be switchable without re-authoring: the same scene runs on Forward+, Mobile, and Compatibility.
3. Each scene must report `raster/total_draw_calls` and `raster/total_primitives_drawn` so results can be normalized against scene complexity.
4. No script logic in the measured frame beyond camera movement; physics and navigation stay disabled unless the scene is explicitly about them.

## 5. Procedure

For every (scene, configuration) pair:

1. **Build**: record commit hash, build flags, `profiler=none`, and whether it is an editor or export-template build. Editor and template results are separate tables.
2. **Cold run**: delete the pipeline cache (`user://vulkan/pipelines.*.cache`) and run for a fixed number of frames with `--quit-after`. Record startup benchmark (`--benchmark-file`) and the number of PSO compilations before steady state.
3. **Warm-up**: run at least 10 seconds before sampling. Skip samples while `pipeline/compilations_*` is nonzero.
4. **Sample**: capture at least 60 seconds of steady state, vsync disabled with `--disable-vsync` (and driver vsync disabled manually where the flag cannot override it).
5. **Repetitions**: minimum 5 independent runs per cell; report median, p95, and the spread. Publish the raw per-run values.
6. **Thermals**: on mobile, record whether the device was cold or warm and discard runs showing sustained clock reduction.
7. **Recording**: fill the report template (section 7) with the exact commands used.

Frame-time statistics are computed from the per-second `mspf` output and, where available, the frame profiler; a hitch is defined as a frame whose time exceeds twice the run median.

## 6. Threats to validity

- **Compositor and driver vsync** can cap FPS below the measured capability; `--disable-vsync` does not override driver-level enforcement (`main.cpp:629`).
- **Background load** (browser, IDE, other GPU clients) changes results on all platforms.
- **Shader cache state** dominates first-run numbers, especially on Metal where no persistent cache exists.
- **Editor overhead** is not representative of exported games; editor results are reported separately (S7).
- **GPU timestamp availability** differs per backend; absence of GPU timing must be reported, never replaced by an estimate.
- **Version drift**: driver updates, OS updates, and engine commits change results; each report row must pin all three.
- **Thermal throttling** on laptops and phones invalidates long runs.

## 7. Report template

```text
Report:            <title>
Engine commit:     <hash>
Build:             editor | template, flags: <...>
Scene:             S<n> <name>, parameters: <counts>
Configuration:     <ID from section 3>
Hardware:          CPU <...>, GPU <...>, RAM <...>
Driver / OS:       <...>
Command:           <exact command line>

Startup (--benchmark):
  [Startup] Main::Setup        <ms>
  [Startup] Servers            <ms>
  [Startup] Rendering          <ms>

Steady state (N runs):
  FPS median / p95             <...> / <...>
  mspf median / p95            <...> / <...>
  CPU process time             <ms>
  GPU total                    <ms> or "not available on this backend"
  GPU per pass                 <pass: ms>
  Draw calls                   <n>
  Objects / primitives         <n> / <n>
  texture_mem / buffer_mem     <MB> / <MB>
  PSO compilations             <n per frame after warm-up>
  Hitches (> 2x median)        <n>

Notes / anomalies:
```

## 8. Automation (implemented and validated for one cell)

- A GDScript harness (`benchmarks/rd_3d/scripts/bench_harness.gd`) moves the camera by frame index, reads `Performance` monitors, and writes one JSON report per run. The camera is a function of the frame index rather than of elapsed time, so a run covers the same frames at any frame rate; `--fixed-fps` changes the simulation delta, not the rendered frame sequence, and is not required. The harness refuses to run in the editor unless explicitly allowed for scene development, and the report is then marked invalid.
- The harness does not create or destroy resources during sampling, so results are not mixed with deferred disposal or PSO compilation.
- Each run writes one JSON file, and `benchmarks/tools/check_report.py` validates individual files and repeated runs against the gate. Collecting files into a versioned data set and generating the published summaries from it is **not implemented**.

## 9. Open items

- Scene authoring — **S1–S4 and S6 authored**, S5 **not written**, S7 a **manual editor procedure**; the implementable definitions are in [Benchmark scene specification](rendering-benchmark-scenes.md) and the code in [benchmark projects](../../benchmarks/README.md).
- Harness implementation — **authored and executed**; one cell passes the validity gate with five warm runs and one cold run. The observations are in the [run log](rendering-benchmark-run-log.md).
- Section 1 — **twenty-eight cells measured** on D1 (template build, 1920×1080), published in [`benchmarks/results/2026-11-19_s1_d1_q0/`](../../benchmarks/results/2026-11-19_s1_d1_q0/README.md), [`benchmarks/results/2026-11-19_d1/`](../../benchmarks/results/2026-11-19_d1/README.md) and the three S6 sessions [`q1`](../../benchmarks/results/2026-11-19_s6_d1_q1/README.md), [`q4`](../../benchmarks/results/2026-11-19_s6_d1_q4/README.md), [`q16`](../../benchmarks/results/2026-11-19_s6_d1_q16/README.md); every other row is **not measured**. Tools-build smoke runs are recorded in the [run log](rendering-benchmark-run-log.md), not here.
- Whether the GLES3 timestamp path behaves on each target driver — **not verified at runtime**.
- Thermal and battery behavior on mobile — **not measured**.
