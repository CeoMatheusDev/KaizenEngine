# Upstream Differences — KaizenEngine vs Godot 4.8

## Summary

KaizenEngine is a fork of Godot 4.8.0-beta. As of the differential audit
(2026-11-19), **zero functional changes** were made to the renderer or
graphics driver source code. All 390 modified files in `servers/` and `drivers/`
contain only cosmetic license header updates (`LICENSE` → `LICENSE.txt`).

That audit describes the fork baseline. Work landed after it does change
upstream behaviour, but never silently: every change is a small, isolated
patch recorded in `patches/` and listed below, and each one keeps a fallback
either through a runtime gate or by dropping the patch file.

## Method

- Compared `git diff 0f54129..HEAD` for all files under `servers/`, `drivers/`,
  `modules/`, `core/`, `scene/`, and `editor/`.
- Filtered out license/header-only changes by checking for non-cosmetic
  insertions/deletions.
- Result: 0 functional changes in renderer or graphics driver code.

## Modified files (cosmetic only)

| Directory | Files | Change type |
|---|---|---|
| `servers/rendering/` | ~200+ | License header only |
| `drivers/vulkan/` | ~5 | License header only |
| `drivers/gles3/` | ~10+ | License header only |
| `drivers/metal/` | ~5 | License header only |
| `platform/android/` | ~100+ | License header only |
| `servers/xr/` | ~30 | License header only |

## Known upstream limitations (inherited, not Kaizen bugs)

| # | File | Issue | Severity | Status |
|---|---|---|---|---|
| 1 | `rendering_device.cpp:924` | `texture_buffer` em callbacks: `DEV_ASSERT(false && "FIXME: Broken.")` | Alta | Patch 002 planejado |
| 2 | `rendering_device_driver_vulkan.cpp:4881` | `UNIFORM_TYPE_IMAGE_BUFFER`: `CRASH_NOW_MSG("Unimplemented!")` | Crítica | Patch 001 planejado |
| 3 | `rendering_device_driver_vulkan.cpp:5416` | Pipeline cache cresce sem limite | Média | Parcial: registry opt-in `KAIZEN_PIPELINE_CACHE=on`; evicção por orçamento continua desligada |
| 4 | `rasterizer_gles3.cpp:278` | Early return em construtor (GLAD fail) | Média | Avaliar |
| 5 | `forward_mobile/render_forward_mobile.h:447-448` | SSAO/SSIL stubs (vazios) | Informativa | Documentado |
| 6 | `rendering_device.cpp:7899,8021` | `frames_pending_resources_for_processing` set (não incrementado) | Baixa | By design |
| 7 | `rendering_device.cpp:8162-8195` | `execute_chained_cmds` usa `thread_local` | Baixa | Documentado |

## Policy

- Upstream code is preserved intact. No functional modifications to renderer or driver code.
- Known issues are documented here and in `docs/architecture/rendering-audit.md`.
- Fixes are applied as isolated patches in `patches/` directory.
- New Kaizen functionality is developed in `kaizen/` subsystems, not by modifying upstream files.

"Preserved intact" is enforced against the fork baseline, not against the
patched working tree: a change to an upstream file is acceptable only when it
exists as a patch file under `patches/`, is listed below, and can be reverted by
dropping that patch. Shader include files follow the same rule — a sampling
change that has to live inside the fragment loop is a patch, not a new
`kaizen/` subsystem.

## Applied patches

| Patch | Target | Gate | Status |
|---|---|---|---|
| `patches/rendering-device/002-texture-buffer-callback.patch` | `servers/rendering/rendering_device.cpp` | none (bug fix) | Implemented, verified |
| `patches/rendering-device/003-resource-tracker-integration.md` | `servers/rendering/rendering_device.cpp` | diagnostics opt-in | Implemented, verified |
| `patches/rendering-device/004-pipeline-lifetime-integration.patch` | `drivers/vulkan/rendering_device_driver_vulkan.cpp/.h` | `KAIZEN_PIPELINE_LIFETIME=on` | Implemented, verified |
| `patches/raycast/001-hzb-level0-scatter.patch` | `modules/raycast/raycast_occlusion_cull.cpp` | none (output is bit-identical) | Applied; −46 % on the level-0 scatter at the default ray budget in an offline harness (≈2.4 µs per viewport per frame at 147×83, `-O3`); only reachable in builds with `modules/raycast`. Guarded by `tests/kaizen/hzb_occlusion` |
| `patches/raycast/002-camera-ray-generation-cost.patch` | `modules/raycast/raycast_occlusion_cull.cpp` | none (output is bit-identical) | Applied; -33 % on per-frame camera-ray generation at the default ray budget (130 -> 87 us of CPU per viewport per frame, offline `-O3`; the loop is split across worker threads, so this is CPU time, not frame time). Applies in either order with patch 001, both verified. Guarded by `tests/kaizen/hzb_occlusion` |
| `patches/renderer-rd/001-shadow-pcss-blocker-and-pcf-kernel.patch` | `servers/rendering/renderer_rd/shaders/scene_forward_lights_inc.glsl` | existing `ShadowQuality` setting | Applied, GPU cost unmeasured |
| `patches/rendering-scene/001-hzb-occlusion-cpu-cost.patch` | `servers/rendering/renderer_scene_occlusion_cull.{h,cpp}` | none (output is bit-identical) | Applied; −31 % to −44 % on the mip build in an offline harness, ≈3 µs/frame in engine because the pyramid is ray-budget sized, not screen sized. Guarded by `tests/kaizen/hzb_occlusion` |

Neither of the last two entries has an in-engine measurement: they were written
in a sandbox with no GPU and no scons install, so no S4 A/B and no full build
exist yet. The HZB entry is backed by an offline harness that proves the output
is bit-identical and sizes the gain on the function alone; the shadow entry has
no measurement at all beyond code review. Treat both as unverified until
`benchmarks/rd_3d` is re-run against them.
