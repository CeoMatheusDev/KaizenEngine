# NVIDIA SDK Integration Guide

## Overview

KaizenEngine supports NVIDIA rendering SDKs for advanced upscaling, ray tracing, and latency reduction. All SDKs are integrated locally and do not require network access during compilation.

## Supported SDKs

### NVIDIA DLSS SDK
- **Purpose**: Deep Learning Super Sampling (Super Resolution, Ray Reconstruction, Frame Generation)
- **Acquisition**: Manual download from [NVIDIA Developer Portal](https://developer.nvidia.com/dlss-sdk) or [GitHub](https://github.com/NVIDIA/DLSS)
- **License**: NVIDIA SDK License Agreement (proprietary, NDA)
- **Redistribution**: Restricted (no watermarked DLLs in shipping builds)
- **Build flag**: `NGX_DLSS_ENABLED`
- **Headers**: `nvsdk_ngx.h`, `nvsdk_ngx_defs.h`, `nvsdk_ngx_params.h`, etc.

### NVIDIA Streamline SDK
- **Purpose**: Cross-IHV integration framework for DLSS, Reflex, NIS, NRD
- **Acquisition**: [GitHub](https://github.com/NVIDIA-RTX/Streamline) (open source, no login required)
- **License**: NVIDIA Streamline SDK License
- **Redistribution**: Permitted
- **Build flag**: `STREAMLINE_ENABLED`
- **Headers**: `sl.h`, `sl_core_api.h`, `sl_dlss.h`, `sl_reflex.h`, etc.

### NVIDIA Reflex SDK
- **Purpose**: Low-latency mode for reduced render latency
- **Acquisition**: [NVIDIA Developer Portal](https://developer.nvidia.com/downloads/nv-reflex-sdk-zip/) (login required) or via Streamline
- **License**: NVIDIA Reflex SDK License
- **Redistribution**: Permitted
- **Build flag**: `REFLEX_ENABLED`
- **Headers**: `sl_reflex.h` (via Streamline), `nvReflex.h` (standalone)

### NVIDIA NGX SDK
- **Purpose**: Core neural graphics infrastructure for DLSS and other AI features
- **Acquisition**: [NVIDIA Developer Portal](https://developer.nvidia.com/ngx-sdk) (login required)
- **License**: NVIDIA NGX License Agreement
- **Redistribution**: Prohibited (NVIDIA-confidential)
- **Build flag**: `NGX_ENABLED`
- **Headers**: `nvsdk_ngx.h`, `nvsdk_ngx_core.h`, `nvsdk_ngx_dlss.h`

## Installation

### Automatic SDK Status Check
```bash
python tools/sdk/sdk_manager.py --list
```

### Manual Installation
1. Download the SDK from the official source
2. Extract to the appropriate directory under `thirdparty/nvidia/`
3. Run: `python tools/sdk/sdk_manager.py --install <sdk_key> <archive_path>`
4. Validate: `python tools/sdk/sdk_manager.py --validate <sdk_key>`

### Build Configuration
```bash
# Enable NVIDIA SDKs during build
scons builtin_nvidia_dlss=yes builtin_nvidia_streamline=yes builtin_nvidia_reflex=yes builtin_nvidia_ngx=yes
```

## Build Options

| Option | Default | Description |
|--------|---------|-------------|
| `builtin_nvidia_dlss` | no | Enable NVIDIA DLSS SDK integration |
| `builtin_nvidia_streamline` | no | Enable NVIDIA Streamline SDK integration |
| `builtin_nvidia_reflex` | no | Enable NVIDIA Reflex SDK integration |
| `builtin_nvidia_ngx` | no | Enable NVIDIA NGX SDK integration |
| `nvidia_sdk_path` | thirdparty/nvidia | Custom path to NVIDIA SDK root |

## Directory Structure
```
thirdparty/nvidia/
├── dlss/           # DLSS SDK (headers, libs, binaries)
├── streamline/     # Streamline SDK (headers, libs, binaries)
├── reflex/         # Reflex SDK (headers, libs, binaries)
├── ngx/            # NGX SDK (headers, libs, binaries)
└── common/         # Common NVIDIA headers
```

## Runtime Detection
- DLSS is only enabled on NVIDIA GPUs (detected via device vendor name)
- On non-NVIDIA GPUs, the engine automatically falls back to FSR2
- Capability detection is performed at runtime via `RenderingDevice::has_feature()`

## Licensing Notes
- DLSS and NGX are NVIDIA-confidential and require NDA approval
- Streamline is open source and can be freely redistributed
- Reflex standalone SDK requires developer account login
- Never commit proprietary NVIDIA binaries to version control
