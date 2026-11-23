#!/bin/bash
# NVIDIA DLSS SDK Installation Script
# This script helps download and install the NVIDIA DLSS SDK into the thirdparty directory
# After running this script, update dlss.cpp and build configuration

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SDK_DIR="${SCRIPT_DIR}"
TARGET_DIR="${SCRIPT_DIR}"

echo "=== NVIDIA DLSS SDK Installation ==="
echo "Target directory: ${TARGET_DIR}"
echo ""

# Check if SDK is already present
if [ -f "${TARGET_DIR}/include/nvrhi.h" ] || [ -f "${TARGET_DIR}/include/nvkfb.h" ] || [ -f "${TARGET_DIR}/include/nvDLSS.h" ]; then
    echo "SDK appears to already be present. Checking for required files..."
    if [ -f "${TARGET_DIR}/include/nvrhi.h" ] && [ -f "${TARGET_DIR}/include/nvDLSS.h" ]; then
        echo "SDK is already properly installed."
        exit 0
    fi
fi

echo "The NVIDIA DLSS SDK requires manual download from NVIDIA Developer Portal."
echo ""
echo "Steps to complete manually:"
echo "1. Visit: https://developer.nvidia.com/dlss-sdk"
echo "2. Log in to your NVIDIA Developer account"
echo "3. Download the NVIDIA DLSS SDK (requires registration)"
echo "4. Extract the SDK to the thirdparty/nvidia-dlss/ directory"
echo ""
echo "Expected structure after extraction:"
echo "  ${TARGET_DIR}/"
echo "  ├── include/"
echo "  │   ├── nvDLSS.h"
echo "  │   ├── nvDLSS_2.h"
echo "  │   ├── nvxr_enumerate_gpu_info.h"
echo "  │   └── nvrhi.h (optional, for NVRHI abstraction)"
echo "  ├── lib/"
echo "  │   ├── x64/ (or appropriate arch)"
echo "  │   │   ├── nvDLSS_2_64.dll"
echo "  │   │   └── nvngx_core_64.dll"
echo "  └── thirdparty/"
echo "      └── (NVIDIA-specific third party licenses)"
echo ""
echo "Alternative: Check https://github.com/NVIDIAGameWorks/Streamline for Streamline SDK"
echo ""

# Create expected directory structure
mkdir -p "${TARGET_DIR}/include"
mkdir -p "${TARGET_DIR}/lib/x64"
mkdir -p "${TARGET_DIR}/thirdparty/Streamline/include"

echo "Created directory structure:"
find "${TARGET_DIR}" -type d | head -20