# NVIDIA DLSS Integration Status

## Current Status

- ✅ Real NGX integration in `servers/rendering/renderer_rd/effects/dlss.cpp`:
  the SDK library is loaded at runtime (`libnvidia-ngx-dlss.so` / `nvngx.dll`,
  no link dependency), NGX initialises on the RenderingDevice's Vulkan device,
  and `create_context`/`upscale` drive `NVSDK_NGX_VULKAN_CreateFeature` /
  `EvaluateFeature` with the engine's colour/depth/velocity/reactive/exposure
  resources, pixel-space jitter and NDC-to-pixel motion-vector scaling.
- ✅ Runtime fallback: selecting DLSS when the SDK or an NVIDIA GPU is
  unavailable falls back to FSR2 upscaling with a one-time warning (no stale
  upscaled texture).
- ✅ Enum contract `SCALING_3D_MODE_DLSS = 6` synchronised across C++
  (`viewport.h`, `rendering_server_enums.h`), doc XML (`Viewport.xml`,
  `RenderingServer.xml`) and the C# glue; `tests/kaizen/api_glue` pins it.
- ✅ Build integration: `builtin_nvidia_dlss=yes` enables `NGX_DLSS_ENABLED`
  (requires the Vulkan backend); defines and include paths come from the SDK
  detection in `SConstruct`.
- ✅ All NVIDIA TUs compile against the real SDK headers (checked in CI-style
  sweeps; see `tests/kaizen/run_tests.sh` for the engine-independent suite).

## SDK Installation

```bash
python3 tools/sdk/sdk_manager.py --download nvidia/dlss   # GitHub mirror
python3 tools/sdk/sdk_manager.py --validate nvidia/dlss
# or the manual route: thirdparty/nvidia-dlss/install_sdk_from_nvidia.sh
```

Expected layout (validated by the manager):

```
thirdparty/nvidia/dlss/include/nvsdk_ngx*.h
thirdparty/nvidia/dlss/lib/Linux_x86_64/rel/libnvidia-ngx-dlss.so*
```

Then build with `scons builtin_nvidia_dlss=yes` (and Vulkan enabled).

## Runtime Flow

1. `RenderForwardClustered` maps `VIEWPORT_SCALING_3D_MODE_DLSS` to the DLSS
   upscale pass only when `DLSSEffect::is_supported()` reports the SDK came up;
   otherwise it falls back to FSR2.
2. `ensure_dlss` creates a `DLSSContext` (NGX feature) per resolution pair.
3. `DLSSEffect::upscale` flushes and syncs the frame, records the SDK dispatch
   into a dedicated command buffer with conservative barriers, submits and
   waits on the queue. Correct by construction; the cost is one stall per
   upscaled frame. The fast path (recording into the engine's in-flight
   command stream) needs an engine seam and is documented in `dlss.cpp`.

## Verification Steps

1. `./tests/kaizen/run_tests.sh` — engine-independent suite (pipeline cache,
   api_glue enum contract, license banners).
2. Compile sweep of the DLSS touchpoints with the SDK defines on
   (`effects/dlss.cpp`, `forward_clustered/render_forward_clustered.cpp`).
3. On NVIDIA hardware: select `SCALING_3D_MODE_DLSS` on a viewport and check
   the `DLSS` label in a Vulkan capture; on non-NVIDIA hardware the fallback
   warning must appear once and FSR2 must upscale.

## Remaining GPU-Test Items

- Validate the conservative sync cost and, if too heavy, add the RD command
  buffer seam for in-stream recording.
- Motion-vector axis conventions on non-Vulkan drivers (only Vulkan is wired).
