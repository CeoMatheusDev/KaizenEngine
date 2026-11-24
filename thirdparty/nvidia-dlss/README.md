# NVIDIA DLSS / Streamline SDK

This directory holds optional NVIDIA-specific integration notes for the
KaizenEngine renderer. The SDKs themselves are **not** vendored here; they
are installed under `thirdparty/nvidia/` (see below) and never committed.

## Scope

- DLSS Super Resolution (2.2 / 3.x family via NGX / NVIDIA Streamline).
- DLSS Ray Reconstruction.
- DLSS Frame Generation.
- NVIDIA RTX hardware ray tracing acceleration.

## Distribution Terms

The NVIDIA SDK / API binaries are provided under NVIDIA's developer program
terms. This repository does not vendor those binaries; they are obtained
through the NVIDIA developer portal or the public GitHub mirrors and
integrated locally per NVIDIA's distribution requirements. Check the license
before redistributing builds that include them.

## Installation

The build system expects the SDKs under `thirdparty/nvidia/`:

```
thirdparty/nvidia/
├── dlss/include/nvsdk_ngx*.h   # DLSS + NGX headers
├── dlss/lib/Linux_x86_64/rel/  # libnvidia-ngx-dlss.so and friends
├── streamline/include/sl*.h    # Streamline (and bundled Reflex) headers
└── common/include/
```

Use the SDK manager (it knows the exact expected files):

```bash
python3 tools/sdk/sdk_manager.py --list
python3 tools/sdk/sdk_manager.py --download nvidia/dlss     # or --install
python3 tools/sdk/sdk_manager.py --validate all
```

Then build with the feature flags you want:

```bash
scons builtin_nvidia_dlss=yes builtin_nvidia_ngx=yes
```

## Integration Notes

The engine-side hooks live in:

- `servers/rendering/renderer_rd/effects/dlss.h/.cpp` - DLSSEffect integration point.
- `kaizen/rendering/nvidia/dlss/` - DLSS backend (upscaling interface).
- `servers/rendering/rendering_server_enums.h` - `VIEWPORT_SCALING_3D_MODE_DLSS` selection.

The Vulkan backend registers the optional NVIDIA extensions
(`VK_NV_ray_tracing`, etc.) used by the DLSS upscaler and ray tracing pipeline.
