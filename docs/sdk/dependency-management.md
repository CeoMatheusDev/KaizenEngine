# Dependency Management Guide

## Overview

KaizenEngine uses a local SDK management system for NVIDIA and PhysX dependencies. All dependencies are installed locally and do not require network access during compilation.

## SDK Manager Tool

The SDK manager is located at `tools/sdk/sdk_manager.py` and provides the following commands:

### List SDKs
```bash
python tools/sdk/sdk_manager.py --list
```
Shows all known SDKs and their installation status.

### Validate Installation
```bash
python tools/sdk/sdk_manager.py --validate <sdk_key>
python tools/sdk/sdk_manager.py --validate all
```
Validates that an SDK is properly installed and complete.

### Install from Archive
```bash
python tools/sdk/sdk_manager.py --install <sdk_key> <archive_path>
```
Extracts an SDK archive into the correct local directory.

### Show Download Instructions
```bash
python tools/sdk/sdk_manager.py --instructions <sdk_key>
```
Shows manual download instructions for SDKs that require developer portal access.

## Manifest System

SDK manifests are stored in `tools/sdk/manifest/` as JSON files:

- `nvidia_sdks.json`: NVIDIA rendering SDKs (DLSS, Streamline, Reflex, NGX)
- `physx_sdks.json`: NVIDIA PhysX SDKs

Each manifest entry contains:
- Name, description, acquisition method
- Download URL, license, redistribution terms
- Current and minimum versions
- Expected directory structure
- Build flags and feature mappings
- Notes and special requirements

## SDK Acquisition

### Automatic Acquisition
Some SDKs can be downloaded automatically from GitHub:
- **Streamline**: `https://github.com/NVIDIA-RTX/Streamline`
- **PhysX**: `https://github.com/NVIDIA-Omniverse/PhysX`

### Manual Acquisition
SDKs requiring NVIDIA Developer Portal access:
- **DLSS**: `https://developer.nvidia.com/dlss-sdk`
- **NGX**: `https://developer.nvidia.com/ngx-sdk`
- **Reflex**: `https://developer.nvidia.com/downloads/nv-reflex-sdk-zip/`

For these, follow the download instructions and use `--install` to extract.

## Version Management

### Version Detection
The SDK manager checks for:
- Presence of expected include files
- Presence of expected library files
- Source directory structure (for source-built SDKs)

### Version Validation
- SHA-256 checksums are recorded in manifests for validation
- Incomplete or corrupted installations are detected
- Working SDK versions are not accidentally replaced

### Reproducible Builds
- SDK versions are explicitly tracked in manifests
- No silent version substitution
- Build diagnostics show SDK availability status

## Build Integration

### SDK Detection
The build system detects SDKs by checking for their include files:
- DLSS: `thirdparty/nvidia/dlss/include/nvsdk_ngx.h`
- Streamline: `thirdparty/nvidia/streamline/include/sl.h`
- Reflex: `thirdparty/nvidia/reflex/include/sl_reflex.h`
- NGX: `thirdparty/nvidia/ngx/include/nvsdk_ngx.h`
- PhysX: `thirdparty/physx/physx/physx/include/foundation/PxFoundation.h`

### Conditional Compilation
Each SDK is conditionally compiled based on availability:
- `NGX_DLSS_ENABLED`: DLSS SDK present
- `STREAMLINE_ENABLED`: Streamline SDK present
- `REFLEX_ENABLED`: Reflex SDK present
- `NGX_ENABLED`: NGX SDK present
- `PHYSX_ENABLED`: PhysX SDK present
- `PHYSX_EXTENSIONS_ENABLED`: PhysX Extensions present
- `PHYSX_GPU_ENABLED`: PhysX GPU acceleration enabled

### Fallback Behavior
- Without NVIDIA SDKs, the engine builds successfully with FSR2 fallback
- Without PhysX, the engine uses Kaizen physics
- No network connection required during compilation

## .gitignore

Proprietary NVIDIA binaries are automatically ignored by the `.gitignore`:
- `*.dll`, `*.lib`, `*.so`, `*.a` in thirdparty directories
- SDK build output directories

Only source code, headers, manifests, and build scripts are committed.

## Troubleshooting

### SDK Not Found
If the build system reports an SDK as not found:
1. Check the SDK directory path (default: `thirdparty/nvidia/` or `thirdparty/physx/`)
2. Verify the SDK is properly extracted
3. Run `python tools/sdk/sdk_manager.py --validate <sdk_key>` for details

### Build Errors
If compilation fails due to missing SDK headers:
1. Ensure the SDK is installed in the correct directory
2. Check that the build option is enabled (e.g., `builtin_nvidia_dlss=yes`)
3. Verify the include paths are correct

### Version Mismatch
If an SDK version is incompatible:
1. Check the minimum version in the manifest
2. Download the correct version from the official source
3. Reinstall using `--install`
