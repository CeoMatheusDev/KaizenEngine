# Benchmark Run Log

> **Status:** One validated cell (S1/D1/Q0, 400 boxes) plus tools-build smoke runs
> **Engines:** tools `4.8.beta.custom_build.92482158f` (`target=editor`) and template `4.8.beta.custom_build.43c45bf02` (`target=template_release`)
> **Date:** 2026-11-19

This document records executions of the benchmark project. It exists so that a
run is never described from memory: what was run, on what, and what the report
said. It has two parts. Sections 1–5 are tools-build smoke runs that deliberately
break the validity gate and publish nothing. Section 6 is the first cell that
passes the gate end to end; its reports are committed in
[`benchmarks/results/2026-11-19_s1_d1_q0/`](../../benchmarks/results/2026-11-19_s1_d1_q0/README.md).

The contract is in [Benchmark scene specification](rendering-benchmark-scenes.md)
and [Benchmark methodology](rendering-benchmarks.md). The code is in
[benchmark projects](../../benchmarks/README.md).

## 1. What was executed

### 1.1 Tools-build smoke runs

```text
<kaizen-editor> --path benchmarks/rd_3d -- \
    --bench-scene <S1|S2|S3|S4|S6> --bench-warmup 300 --bench-frames 300 \
    --bench-config D1 --bench-allow-editor \
    --bench-out /tmp/opencode/kz-run/<cell>.json
```

- Warm-up 300 and sampling 300 frames: the shortest legal window, chosen to keep
  the smoke run short.
- `--bench-allow-editor` because no export template existed at that point. The
  harness marks every such report `valid = false` with that reason.
- S6 was run at `--bench-count 0` and `--bench-count 4`, with `--bench-preset`
  from `P0` to `P5`.
- S1 was also run once with the full 900/3600 window, while a 24-job engine build
  was running on the same machine (see finding in section 4).

### 1.2 Template runs

```text
<kaizen-template> --path benchmarks/rd_3d --screen 1 --fullscreen -- \
    --bench-scene S1 --bench-count 400 --bench-config D1 --bench-preset Q0 \
    --bench-warmup 900 --bench-frames 3600 \
    --bench-out <abs>/S1_D1_Q0_r<n>.json
```

- Five warm runs plus one cold run (pipeline cache deleted before it).
- Fullscreen on the 1920×1080 output: a windowed run loses client height to the
  window manager (section 5, finding 3).
- The template binary was built with
  `scons platform=linuxbsd target=template_release -j24 wayland=no alsa=no
  pulseaudio=no speechd=no udev=no dbus=no disable_path_overrides=no`, which is
  also why `--path` works in a template build.

## 2. Environment

| Item | Value |
|---|---|
| CPU | Intel Xeon E5-2680 v3 @ 2.50 GHz, 24 threads |
| GPU | AMD Radeon RX 550 / 550 Series (RADV POLARIS12) |
| Vulkan | 1.4.354, Forward+ |
| Mesa | 26.2.5 |
| Kernel | 7.2.15-200.fc44.x86_64 |
| Display server | X11 on a Wayland session (XWayland) |
| Requested window | 1920×1080 (project setting) |
| Tools-build window | **1600×834** — clamped on the 1600×900 primary display |
| Template window | **1920×1080** — fullscreen on the 1920×1080 output |
| VSync | 0 (project setting, reported by the harness) |

The tools-build runs render at another resolution than the one the baseline pins,
so `benchmarks/tools/check_report.py` fails them on the window size. Fullscreen
on the larger output fixed that for the template runs without changing any
project setting.

## 3. Tools-build values (invalid by design)

300 sampled frames, one run each, RADV/Forward+, 1600×834. `gpu` and `cpu` are
`summary.gpu_ms_median` and `summary.render_cpu_ms_median`; the FPS column is the
median wall-clock frame rate with presentation blocking included.

| Scene | Parameters | Draw calls | Primitives | FPS | GPU ms | CPU ms | `first_120` compiles |
|---|---|---|---|---|---|---|---|
| S1 | 400 boxes | 7 (constant) | 4802 | 707 | 1.23 | 0.12 | 5 |
| S2 | 4096 instances | 1 (constant) | 4,456,448 | 166 | 5.80 | 0.07 | 5 |
| S3a | 64 materials | 64 (constant) | 69,632 | 1292 | 0.59 | 0.10 | 5 |
| S3b | 8 feature materials | 16 (constant) | 69,632 | 1241 | 0.62 | 0.09 | 19 |
| S4 | 8 spots + 8 omnis | **11–15** | 2350 | 364 | 2.54 | 0.15 | 7 |
| S6 P0 | 4 quads | 3 (constant) | 394 | 623 | 1.42 | 0.08 | 7 |
| S6 P2 | 4 quads | 3 (constant) | 394 | 372 | 2.49 | 0.11 | 8 |

`first_120` is the per-source pipeline-compilation delta for the first 120 frames
after the scene starts loading. `timing.pso_in_sample` was 0 in every run, and
`last_compile_frame` was -1 in every run: the compiles happened during scene
load, before frame 0.

S6 with `--bench-count 0`, GPU median only:

| Preset | Effects | GPU ms |
|---|---|---|
| P0 | none | 1.42 |
| P1 | glow | 3.14 |
| P2 | SSAO | 2.49 |
| P3 | SSAO, SSIL | 5.54 |
| P4 | glow, SSAO, SSIL | 7.25 |
| P5 | glow, SSAO, SSIL, volumetric fog | 8.06 |

Image difference against the P0 screenshot of the same run, over every RGBA byte
(0–255):

| Preset | Mean abs diff | Max | Pixels changed |
|---|---|---|---|
| P1 | 3.055 | 37 | 100% |
| P2 | 0.0011 | 1 | 0.35% |
| P3 | 0.0049 | 5 | 1.34% |
| P4 | 3.051 | 37 | 99.99% |
| P5 | 17.24 | 87 | 94.36% |

S1 with the same content on the same machine, one renderer per run:

| Renderer | Draw calls | Primitives |
|---|---|---|
| Forward+ (Vulkan) | 7 | 4802 |
| Mobile (Vulkan) | 401 | 4802 |
| Compatibility (OpenGL) | 401 | 4802 |

S6 on the Mobile renderer: 37 draw calls (32 unbatched boxes, ground, four
quads). `P5` there was refused with `post effect 'ssao' is not supported by the
mobile renderer`, and `P1` (glow, which Mobile does provide) was accepted.
`P2` on Mobile produced an image byte-identical to `P0`, which is the evidence
that the refusal is correct there.

S6 on Compatibility (OpenGL), `--bench-count 0`:

| Preset | GPU ms | Image vs P0 |
|---|---|---|
| P0 | 0.50 | — |
| P2 | 1.37 | mean 1.021, 49.0% of pixels changed |

`P2` costs 0.87 ms more than `P0` and changes half the frame, so SSAO really runs
on this backend.

## 4. What the smoke runs established

- **S1 is deterministic.** Two independent runs of S1/400/D1/Q0 produced the
  same `frame_hash` (`c150718e…`) and the same draw-call median; the GPU median
  spread between them was 0.04%. A third run with the full protocol window
  (900 warm-up, 3600 sampled) reproduced the same frame hash, the same 7 draw
  calls and the same 4802 primitives. The camera-by-frame-index contract holds
  when the engine actually renders.
- **A run is only as quiet as the machine.** The first full-protocol S1 run
  shared the machine with a 24-job engine build: 737 of 3600 frames were above
  twice the median, with an 11.8 ms p99 and an 18.6 ms worst frame. Repeating
  the same cell on an idle machine gave 0 hitches in each of five runs. CPU
  contention, not the scene, explained the tail; a benchmark run has to own the
  machine.
- **Every scene builds its content from code and runs.** No binary asset is
  needed, and no scene failed to expose `apply_frame` or `scene_parameters`.
- **The S6 refusal path is real.** Forward+ reports support for all four
  effects. On Mobile, `P1` (glow) is accepted and `P5` is marked invalid with the
  unsupported effect named. On Compatibility, `P5` is refused for SSIL and
  volumetric fog, and `P2` (SSAO) is accepted. The refusal is not a paper rule.
- **`--bench-min-diff` works as a gate.** S6 P2 against P0 at 0.5 marked the run
  invalid with the measured value in the reason, which is the intended behaviour.

## 5. Findings that changed the code or the documents

1. **The project was named `project.godot`.** This engine loads `project.kaizen`
   (`core/config/project_settings.cpp:709`), so the project was invisible and the
   engine opened the project manager instead. Renamed; every reference updated.
   Nothing before this run had ever loaded the project.
2. **`Performance.TIME_PROCESS` and `TIME_PHYSICS_PROCESS` are unusable in a
   benchmark.** `main/main.cpp:5186` publishes them as the maximum over a
   1,000,000-frame window, so in a 600-frame run they hold the startup frame:
   every sample read 104.281 ms. They were removed; the renderer's own
   `viewport_cpu_ms` is published instead.
3. **The window can be smaller than the pinned viewport.** A window manager can
   silently shrink the client area: the same 1920×1080 request measured 1600×834
   on the 1600×900 primary display and 1920×1052 when moved to the 1080p output,
   because decorations ate 28 px. `--resolution` does not win in either case; a
   fullscreen window on the 1080p output gives exactly 1920×1080. The report
   records the real size, and the checker fails a mismatch instead of warning.
4. **Batching is a Forward+ property, and it is partial.** The same S1 cell
   reported 7 draw calls on Forward+ and 401 on both Mobile and Compatibility,
   where each instance is its own draw. On Forward+, batching merges only
   adjacent render-list elements, and the sort order changes with the camera:
   1/64/65/128/256/400 boxes produced 2/5/4/5/6/7 draw calls. The count is
   non-monotonic, so no fixed expected count is valid; only stability across
   runs is.
5. **S4 draw calls change within one run (11–15).** The cell was 8 spots + 8
   omnis with 64 boxes and a moving camera, and the count varies because the
   batching of shadowed and lit surfaces changes as the view moves. The
   specification's across-cell comparison (same camera path, different light
   counts) was **not** run, so this does not say whether light count changes the
   count; it says only that a within-run constant is not a property of S4.
6. **SSAO exists on Compatibility, and the support map said it did not.** The
   first version of `_post_support()` treated SSAO as Forward+ only. GLES3 has
   its own SSAO (`drivers/gles3/rasterizer_scene_gles3.cpp`, the `ssao_enabled`
   post-shader path), and the run proves it: S6 `P2` changed 49% of the pixels
   and cost 0.87 ms more than `P0`. The map now claims SSAO on `forward_plus` and
   `gl_compatibility`, and SSIL and volumetric fog on `forward_plus` only.
   Claiming less than a backend can do is a measurement bug: "not applicable"
   silently removes a cell that could have been measured.
7. **SSAO barely changes this image.** It modulates the ambient term, and the S6
   base scene lights everything from a sun with no shadows, so P2's mean
   difference is 0.0011 even though SSAO costs about 1 ms. "Exactly zero means the
   effect did not run" is the correct gate; a fixed 0.5 threshold would be
   satisfied only by glow and fog, and per-preset thresholds belong to the
   collector, not to the specification.
8. **The harness could not describe a cold run.** The protocol asks for
   time-to-first-steady-frame, but the report had no such field, so the cold/warm
   comparison was impossible. The harness now records
   `timing.load_to_sample_ms`, measured from the scene-load call to the first
   sampled frame, and the checker requires a positive value.

## 6. The first validated cell

S1 / D1 / Q0, 400 boxes, template build `4.8.beta.custom_build.43c45bf02`
(commit `43c45bf`), fullscreen 1920×1080, 900 warm-up and 3600 sampled frames,
five warm runs and one cold run.

| Metric | Five warm runs | Cold run |
|---|---|---|
| FPS median | 379.0–380.5 | 380.8 |
| Frame ms median | 2.628–2.639 | 2.626 |
| p99 frame ms | 2.893–2.982 | 2.965 |
| GPU ms median | 1.894–1.895 | 1.894 |
| CPU render ms median | 0.115–0.116 | 0.117 |
| Load to first sample | 2511–2605 ms | 2509 ms |
| `first_120` compiles | 5 | 5 |
| Hitches | 0 | 0 |

- `frame_hash` = `afeb8045b4b4c5d09c10cee5f35343e78235a6c76d68088ad63bdc36c4094fd6`
  in all six runs. `content_hash` = `32288e115d97…`.
- `draw_calls` = 7, `objects` = 401, `primitives` = 4802 in every sampled frame.
- Deleting the pipeline cache changed neither the compile count nor the load
  time: the scene requests five pipelines and pays for them regardless, and the
  900-frame warm-up dominates the 2.5 s. The cache file was recreated at a
  different size (1,256,676 → 716,464 bytes), so the deletion did take effect.
- A frame hash only compares frames rendered at the same size: the earlier
  tools build at 1600×834 produced a different hash. The SSAO fix, which was
  GDScript-only and does not touch S1, did not change the 1920×1080 hash — the
  same cell produced `afeb8045…` with both the `acde0c3` and the `43c45bf`
  binary.
- The reports, the frame-299 screenshot, the environment and the manual checks
  are archived in
  [`benchmarks/results/2026-11-19_s1_d1_q0/`](../../benchmarks/results/2026-11-19_s1_d1_q0/README.md).
- Manual checks recorded there: VSync inferred from 380 FPS and a p99 of at most
  2.98 ms (no swapchain counter), no monotonic thermal drift (-0.11% to +0.21%
  within a run, no temperature sensor), and the screenshot inspected for correct
  content.
- This is one machine and one cell. It is a reference for that GPU, driver, and
  commit; it is not a portable claim.

## 7. Still missing

- Every other cell of the matrix: S2, S3, S4, S6, and the Mobile, Compatibility,
  Metal, and Web configurations.
- An exported `.pck` run. The validated run uses the template binary with
  `--path`, loading the project directory; that is a template build, but not the
  shipping package layout.
- Driver-level VSync proof and a GPU temperature reading; both remain manual.
- A second machine and a second GPU, to separate engine behaviour from this
  hardware.
- S5 (2D) and S7 (manual editor procedure) are still as described in the
  specification.
