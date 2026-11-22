# Upstream Differences — KaizenEngine vs Godot 4.8

## Summary

KaizenEngine is a fork of Godot 4.8.0-beta. As of the differential audit
(2026-11-19), **zero functional changes** were made to the renderer or
graphics driver source code. All 390 modified files in `servers/` and `drivers/`
contain only cosmetic license header updates (`LICENSE` → `LICENSE.txt`).

Two corrections to that sentence, both from the 2026-11-22 pass over collisions,
navigation/NPC AI and audio. First, "cosmetic license header updates" is no longer
interchangeable with "header changes": the headers were rebranded in a way that also rewrote
the copyright attribution line, and two banner styles are now in use that disagree about the
licence - see [licence notice consistency](license-notice-consistency.md). Second, "zero
functional changes" holds for the renderer and the graphics drivers, but the audit's own file
list (`core/`, `modules/`, `scene/`, `editor/`) contains changes that are not cosmetic and
were never listed here: the physics modules were renamed and so were their exposed classes
and engine-selection strings (`modules/godot_physics_2d` to `modules/kaizen_physics_2d`,
`GDCLASS(KaizenPhysicsServer2D, PhysicsServer2D)`, `physics/2d/physics_engine` value
`"KaizenPhysics2D"`), and navigation registers `"KaizenNavigation2D"`/`"KaizenNavigation3D"`
the same way. Those are upstream-facing: a project or a GDExtension that names
`GodotPhysics2D` no longer matches a registered name, and falls back to the default server
instead. Verified this pass: outside `docs/` no `GodotPhysics*`/`GodotNavigation*`/
`godot_physics*` string survives anywhere in the tree (the only mentions are the documents
that record the rename), and every manager registers a default, so the fallback is a verbose
message and not a failure. The fork's `modules/interactive_music` is not an invention either -
upstream has a module by that exact name at `4.5`, including the same six module-local
`doc_classes/*.xml` - so the ambient/adaptive audio layer is inherited, and nothing in it was
re-authored here.
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
| `patches/rendering-device/002-texture-buffer-callback.patch` (+ `.md`) | `servers/rendering/rendering_device.cpp` | none (bug fix) | Implemented. The `.patch` used to hold prose and no diff; it is the real diff of `0b5ec230` now, verified to reproduce that commit byte for byte. "Verified" there means built and smoke-run once, by the author: no in-repo test reaches the new branch |
| `patches/rendering-device/003-resource-tracker-integration.patch` (+ `.md`) | `servers/rendering/rendering_device.cpp`, `SConstruct` | diagnostics opt-in | Implemented. Had a note and no diff at all; the diff (`f4b8a1ca`) now exists, including the `SConscript("kaizen/SCsub")` line the hooks need to link |
| `patches/rendering-device/004-pipeline-lifetime-integration.patch` | `drivers/vulkan/rendering_device_driver_vulkan.cpp/.h` | `KAIZEN_PIPELINE_LIFETIME=on` | Implemented, verified |
| `patches/vulkan/001-image-buffer.patch` (+ `.md`) | `drivers/vulkan/rendering_device_driver_vulkan.cpp` | none | Implemented. Also prose-only until now; real diff of `0b5ec230`, byte-verified. No test creates or binds an image buffer uniform |
| `patches/rendering-server/001-pipeline-cache-telemetry.patch` (+ `.md`) | `servers/rendering/rendering_server_enums.h`, `rendering_server.cpp`, `rendering_server_default.cpp`, `doc/classes/RenderingServer.xml`, generated C# glue | none (read-only counters) | Written after the fact: `087edfa7` had changed three upstream files with no patch entry. Completes the missing doc XML entries and C# doc comments; guarded by `tests/kaizen/api_glue`. `kaizen_api_hash` in `NativeCalls.cs` stays unverified - only a built engine can compute it |
| `patches/raycast/001-hzb-level0-scatter.patch` | `modules/raycast/raycast_occlusion_cull.cpp` | none (output is bit-identical) | Applied; −46 % on the level-0 scatter at the default ray budget in an offline harness (≈2.4 µs per viewport per frame at 147×83, `-O3`); only reachable in builds with `modules/raycast`. Guarded by `tests/kaizen/hzb_occlusion` |
| `patches/raycast/002-camera-ray-generation-cost.patch` | `modules/raycast/raycast_occlusion_cull.cpp` | none (output is bit-identical) | Applied; -33 % on per-frame camera-ray generation at the default ray budget (130 -> 87 us of CPU per viewport per frame, offline `-O3`; the loop is split across worker threads, so this is CPU time, not frame time). Applies in either order with patch 001, both verified. Guarded by `tests/kaizen/hzb_occlusion` |
| `patches/renderer-rd/001-shadow-pcss-blocker-and-pcf-kernel.patch` | `servers/rendering/renderer_rd/shaders/scene_forward_lights_inc.glsl` | existing `ShadowQuality` setting | Applied, GPU cost unmeasured |
| `patches/misc-scripts/001-header-guards-comment-banner.patch` (+ `.md`) | `misc/scripts/header_guards.py` | none (tooling only) | Applied. The include-guard check understood only the `/* */` banner, so it reported 744 of the repository's own `//`-banner headers as `REQUIRES MANUAL CHANGES` and exited 1 on a clean tree; now 0 and 0. Measured before/after in the note |
| `patches/misc-scripts/002-copyright-headers-keep-slash-banner.patch` (+ `.md`) | `misc/scripts/copyright_headers.py` | none (tooling only) | Applied. The header generator rebuilds banners from its `/* */` template with no `--dry-run`, so running it over a `//`-banner file deleted 30 lines of licence notice per file (derivation statement, AGPL line, pointer to `LICENSE.txt`) from 1541 files. It now refuses those files; guarded by `tests/kaizen/license_box` |
| `patches/misc-scripts/003-header-guards-skip-generated-wrappers.patch` (+ `.md`) | `misc/scripts/header_guards.py` | none (tooling only) | Applied. With patch 001 in place the tool stopped mis-reporting but kept *writing*: over the 1960 tracked non-`thirdparty/` headers it still rewrites 21 files - the 17 generated `*-so_wrap.h` dynload wrappers (which say "This file is generated. Do not edit!") and 4 vendored headers under `modules/mono/thirdparty/`. It now skips both classes: 21 rewritten before, 0 after, exit 0, copies byte-identical |
| `patches/rendering-scene/001-hzb-occlusion-cpu-cost.patch` | `servers/rendering/renderer_scene_occlusion_cull.{h,cpp}` | none (output is bit-identical) | Applied; −31 % to −44 % on the mip build in an offline harness, ≈3 µs/frame in engine because the pyramid is ray-budget sized, not screen sized. Guarded by `tests/kaizen/hzb_occlusion` |

### Banner re-padding (tree-wide, no patch file)

Re-padded 2710 licence-banner rows in 2483 files so each row matches the width of its own
file's `/****/` border: the brand line grew by one column when `GODOT ENGINE` became
`KAIZEN ENGINE`, the contributors line when `Godot` became `Kaizen`, and the filename line
in 191 files when a module directory was renamed (`godot_view.h` to `kaizen_view.h`). Pure
trailing-space changes, provable on the commit: `git diff -w <commit>^ <commit>` is empty
for every one of those 2483 files. No `.patch` file, on purpose: a 2710-hunk whitespace diff
stored in `patches/` would conflict with the first upstream merge that touches a banner and
would buy nothing that git does not already record, and the invariant is enforced
mechanically instead by `tests/kaizen/license_box/check_license_box.py` (`--fix` re-applies
it; the budget catches regressions). 2469 rows stay deliberately ragged - see
[licence notice consistency](license-notice-consistency.md), section 4.

Neither of the last two entries has an in-engine measurement: they were written
in a sandbox with no GPU and no scons install, so no S4 A/B and no full build
exist yet. The HZB entry is backed by an offline harness that proves the output
is bit-identical and sizes the gain on the function alone; the shadow entry has
no measurement at all beyond code review. Treat both as unverified until
`benchmarks/rd_3d` is re-run against them.

## Areas with no Kaizen modification

Audio and physics are upstream code with no fork changes, and it is worth writing
that down rather than leaving it to be rediscovered: 56 files in `servers/audio`, 40
in `servers/physics_3d` and 38 in `servers/physics_2d` carry the "Modified by ... 2026"
header, which looks like a divergence claim. The method to tell the two apart is the
per-path history, not the header:

    gh api "repos/<fork>/commits?path=servers/audio&per_page=8"

`servers/audio`, `servers/physics_3d`, `servers/physics_2d`: touched only by `0f541294`
(initial import) and `e96f3e3a` ("docs: fix license headers, ..."), i.e. headers only, no
behaviour. `drivers/vulkan`: five commits of real work (`0b5ec230`, `ba797e61`,
`f8949606`, `d7524aee`, `7df9e26a`, `087053a3`), all listed in the table above.
`modules/mono`: one commit (`087edfa7`), and it edited generated glue rather than adding
an implementation - see `patches/rendering-server/001`. `modules/gdscript`: one commit,
and its whole content is an empty `modules/gdscript/tests/scripts/.kaizen/.gdignore`.
`core/`: nothing beyond the header pass.

Two consequences that are easy to get wrong. First, no audio or physics number in
`benchmarks/results/` measures fork code: those areas are covered by the S-scenes only as
workload, so an audio or physics regression cannot be attributed to this fork, and cannot
be detected by it either - there is no audio or physics scene in `benchmarks/rd_3d`. The
audio state a cell was measured in used to be invisible: `AudioServer` mixed on whatever
driver the host had, and there was no switch to look for in the report, because `--no-audio`
is not a switch this engine has (`--audio-driver Dummy` is, and `--headless` implies it).
`benchmarks/rd_3d/scripts/bench_harness.gd` now records `runtime.audio_driver` and
`check_report.py` warns when it is not `Dummy` and refuses to pair cells whose states
differ. That does not repair history: all 228 published reports predate the field, so the
state they were measured in is not recoverable, and those cells remain silent about audio.
Second, a future fork change in those areas starts from upstream behaviour: the table above
is the baseline to diff against, not the header.
