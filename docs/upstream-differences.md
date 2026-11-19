# Upstream Differences — KaizenEngine vs Godot 4.8

## Summary

KaizenEngine is a fork of Godot 4.8.0-beta. As of the differential audit
(2026-11-19), **zero functional changes** were made to the renderer or
graphics driver source code. All 390 modified files in `servers/` and `drivers/`
contain only cosmetic license header updates (`LICENSE` → `LICENSE.txt`).

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
| 3 | `rendering_device_driver_vulkan.cpp:5416` | Pipeline cache cresce sem limite | Média | Avaliar |
| 4 | `rasterizer_gles3.cpp:278` | Early return em construtor (GLAD fail) | Média | Avaliar |
| 5 | `forward_mobile/render_forward_mobile.h:447-448` | SSAO/SSIL stubs (vazios) | Informativa | Documentado |
| 6 | `rendering_device.cpp:7899,8021` | `frames_pending_resources_for_processing` set (não incrementado) | Baixa | By design |
| 7 | `rendering_device.cpp:8162-8195` | `execute_chained_cmds` usa `thread_local` | Baixa | Documentado |

## Policy

- Upstream code is preserved intact. No functional modifications to renderer or driver code.
- Known issues are documented here and in `docs/architecture/rendering-audit.md`.
- Fixes are applied as isolated patches in `patches/` directory.
- New Kaizen functionality is developed in `kaizen/` subsystems, not by modifying upstream files.