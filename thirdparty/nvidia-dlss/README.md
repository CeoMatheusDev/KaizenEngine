# NVIDIA DLSS / Streamline SDK

This directory contains optional NVIDIA-specific integration sources for the
KaizenEngine renderer.

## Scope

- DLSS Super Resolution (2.2 / 3.x family via NVIDIA Streamline / SDK).
- DLSS Ray Reconstruction.
- DLSS Frame Generation.
- NVIDIA RTX hardware ray tracing acceleration.

## Distribution Terms

The NVIDIA SDK / API binaries and the `nvsdk` NuGet-style packages are provided
under NVIDIA's developer program terms. This repository does not vendor those
binaries; it expects them to be obtained through the NVIDIA developer portal and
integrated locally per NVIDIA's distribution requirements.

## Integration Notes

The engine-side hooks live in:

- `servers/rendering/renderer_rd/effects/dlss/` - backend implementation layer.
- `servers/rendering/rendering_server_enums.h` - `VIEWPORT_SCALING_3D_MODE_DLSS` selection.

The Vulkan backend registers the optional NVIDIA extensions
(`VK_NV_ray_tracing`, etc.) used by the DLSS upscaler and ray tracing pipeline.
