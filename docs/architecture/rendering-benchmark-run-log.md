# Benchmark Run Log

> **Status:** Twenty-eight validated D1 cells (S1 at 400 boxes; S2, S3a, S3b, six S4 light cells, and S6 P0–P5 at 1, 4 and 16 quads) plus D2 (Mobile) and D3 (Compatibility) template batches plus S5 (2D/Canvas) template batch plus tools-build smoke runs
> **Engines:** tools `4.8.beta.custom_build.92482158f` (`target=editor`) and template `4.8.beta.custom_build.43c45bf02` (`target=template_release`)
> **Date:** 2026-11-19

This document records executions of the benchmark project. It exists so that a
run is never described from memory: what was run, on what, and what the report
said. It has two parts. Sections 1–5 are tools-build smoke runs that deliberately
break the validity gate and publish nothing. Sections 6 and 7 are the templates
that pass the gate end to end; their reports are committed in
[`benchmarks/results/`](../../benchmarks/results/README.md).

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
9. **`--bench-preset Q1` was a mislabeling trap.** In S1–S4 the flag only sets a
   label: the scenes pin Q0, and Q1 is a project-setting plus per-environment
   change that no scene applies. A `--bench-preset Q1` run would have produced a
   Q0 frame under a Q1 label, and the checker had no way to see it. The harness
   now marks such a run invalid unless the scene reports
   `quality_preset = "Q1"` in `scene_parameters()`. The guard was added after the
   runs in section 7, which are all Q0 and therefore unaffected.

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

## 7. The D1 session: S2, S3a, S3b, S4

Nine more cells, same machine and template build as section 6, fullscreen
1920×1080, 900 warm-up and 3600 sampled frames, one cold run and five warm runs
each. The reports are in
[`benchmarks/results/2026-11-19_d1/`](../../benchmarks/results/2026-11-19_d1/README.md);
`check_report.py` reports 54 reports, 9 repeated cells, 0 failures.

| Cell | FPS median | GPU ms | Draw calls | Primitives | `first_120` | Load ms |
|---|---|---|---|---|---|---|
| S2, 4096 instances | 138.6–138.7 | 6.437–6.438 | 1 | 4 456 448 | 5 | 6622–6640 |
| S3a, 64 materials | 638.6–641.4 | 0.835–0.836 | 64 | 69 632 | 5 | 1571–1587 |
| S3b, 8 features | 622.7–624.2 | 0.875–0.876 | 16 | 69 632 | 19 | 1667–1673 |
| S4, 0 spots / 0 omnis | 293.6–294.4 | 2.656–2.657 | 13 | 1 870 | 6 | 3210–3228 |
| S4, 8 spots / 0 omnis | 245.8–246.1 | 3.315 | 13 | 1 870 | 6 | 3801–3817 |
| S4, 0 spots / 32 omnis | 192.1–192.8 | 4.442–4.444 | 13 | 1 870 | 7 | 4821–4885 |
| S4, 8 spots / 32 omnis | 178.5–178.9 | 4.832–4.834 | 13 | 1 870 | 7 | 5163–5175 |
| S4, 32 spots / 64 omnis | 119.6–120.0 | 7.551–7.560 | 13 | 1 870 | 7 | 7643–7656 |
| S4, 8 spots, 4 shadowed | 241.3–242.8 | 3.377–3.378 | 13 | 1 870 | 6 | 3861–3870 |

- Every cell has one frame hash across its six runs; `pso_in_sample` is 0 and
  `pso_at_start` equals `first_120` in all 54 reports.
- **S2 draws once for 4096 instances.** The instance count never reaches the
  draw-call counter; the cost shows up as 4.46 M primitives and 6.44 ms of GPU
  time. Load to first sample is 6.6 s, the longest of the session.
- **S3b turns pipelines into draws.** Same 64 spheres and the same primitive
  count as S3a, but 16 draws instead of 64 and 19 pipelines during load instead
  of 5. Variety in shader features costs pipeline creation, not draw calls.
- **S4 costs GPU time, not draws.** All six cells report 11–15 draws and 133–200
  objects (the camera orbits, so culling changes inside a run), while the GPU
  median rises from 2.656 ms with no extra lights to 7.555 ms with 32 spots and
  64 omnis. Thirty-two omnis cost more than eight spots (+1.786 ms against
  +0.658 ms).
- **Four shadowed spots cost 0.062 ms** over the same cell without spot shadows
  (3.377 against 3.315 ms). The number is recorded as measured; it does not say
  what a larger shadow atlas or a different light mixture would cost.
- One hitch in one S3b run (0/1/0/0/0/0); every other run reported none. The
  gate does not require zero hitches, and the frame hash and GPU medians are
  unaffected.
- Frame drift stays between -0.89% and +0.48% across all cells.

## 8. The S6 sessions: post presets P0–P5 at three overdraw levels

Eighteen cells on the same machine and template build, same 900/3600 sampling,
one cold run and five warm runs each: `quad_count` 1, 4 and 16 with presets
P0–P5. Reports are in
[`2026-11-19_s6_d1_q1/`](../../benchmarks/results/2026-11-19_s6_d1_q1/README.md),
[`2026-11-19_s6_d1_q4/`](../../benchmarks/results/2026-11-19_s6_d1_q4/README.md)
and [`2026-11-19_s6_d1_q16/`](../../benchmarks/results/2026-11-19_s6_d1_q16/README.md);
`check_report.py` reports 36 reports and 0 failures per session, 18 cells in
total.

Each level's P0 cold run wrote its own `p0/frame_299.png`, and every other run
in that level passed it as `--bench-reference`, so each preset records its mean
absolute difference from P0 at the same overdraw. The three P0 references were
inspected: one, four and sixteen nested translucent quads over the checker
ground, with the box field behind them (barely visible at sixteen).

GPU median per preset, in milliseconds:

| Preset | Effects | 1 quad | 4 quads | 16 quads |
|---|---|---|---|---|
| P0 | none | 1.866 | 2.213 | 4.610 |
| P1 | glow | 4.509 | 4.856 | 7.253 |
| P2 | SSAO | 3.478 | 3.825 | 6.220 |
| P3 | SSAO + SSIL | 8.475 | 8.822 | 11.212 |
| P4 | glow + SSAO + SSIL | 11.118 | 11.464 | 13.856 |
| P5 | P4 + volumetric fog | 12.005 | 12.436 | 15.358 |

Delta against P0 at the same overdraw level:

| Preset | 1 quad | 4 quads | 16 quads |
|---|---|---|---|
| P1 | +2.643 | +2.643 | +2.643 |
| P2 | +1.612 | +1.612 | +1.610 |
| P3 | +6.609 | +6.609 | +6.602 |
| P4 | +9.252 | +9.251 | +9.246 |
| P5 | +10.139 | +10.223 | +10.748 |

- **The transparent pass costs what the quads cost.** P0 rises from 1.866 ms
  with one quad to 4.610 ms with sixteen. The rise is not linear: 1 → 4 adds
  0.347 ms (0.116 per quad) and 4 → 16 adds 2.397 ms (0.200 per quad).
- **Glow, SSAO and SSIL do not see the overdraw.** Their deltas are the same at
  all three levels to within 0.007 ms: glow 2.643 everywhere, SSAO 1.610–1.612,
  SSAO + SSIL 6.602–6.609, and the three together 9.246–9.252. This is
  consistent with the quads being transparent and not writing depth, so the
  depth-based passes process the same opaque depth buffer at every level; that
  is an explanation from the scene composition, not a measurement.
- **Volumetric fog is the exception.** Its delta over P0 grows from +10.139 to
  +10.223 to +10.748, and the P5 − P4 step grows from 0.887 to 0.972 to
  1.502 ms. More transparent layers cost the fog more, consistent with fog being
  evaluated per transparent fragment; again, an explanation, not a measurement.
- **Image visibility moves in the other direction.** The P0 image gets more
  tinted as quads are added, so the same effect changes less of it: SSAO's mean
  absolute difference falls from 0.0096 to 0.0057 to 0.0045, and volumetric fog
  from 18.7847 to 16.2981 to 4.7839, while its cost rises. Cost and visible
  difference are separate axes, and both are recorded.
- Every cell reports 3 draw calls and one frame hash across its six runs, with
  `pso_in_sample` 0 and `first_120` 7 for P0–P1 and 8 for P2–P5. Primitives are
  388 at one quad, 394 at four and 418 at sixteen (386 base plus two per quad),
  identical across presets at each level.
- No run reported a hitch. Frame drift stays between −0.69% and +0.46%.
  `report.preset` equals `scene.parameters.post.preset` in all 108 reports.

## 9. Still missing

- The rest of the cell matrix: the other S1 and S2 counts, Q1 (which no scene
  implements), and the M1, W1/W2, A1, and B1 configurations. D2 (Mobile) and
  D3 (Compatibility) are now measured; their reports are in
  `benchmarks/results/2026-11-19_1252_d2_mobile/` and
  `benchmarks/results/2026-11-19_1329_d3_compat/`.
- An exported `.pck` run. The validated runs use the template binary with
  `--path`, loading the project directory; that is a template build, but not the
  shipping package layout.
- Driver-level VSync proof and a GPU temperature reading; both remain manual.
- A second machine and a second GPU, to separate engine behaviour from this
  hardware.
- S5 (2D) is now authored and measured: 12 cells in
  `benchmarks/results/2026-11-19_1414_s5_2d/`. S7 (manual editor procedure)
  remains as described in the specification.
