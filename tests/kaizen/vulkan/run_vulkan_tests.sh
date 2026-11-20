#!/usr/bin/env bash
# Builds and runs the Kaizen P2 device-level test against a real Vulkan device.
#
# Requirements:
#   - a Vulkan 1.2 loader and a physical device with graphics + compute queues
#     and timeline semaphore support;
#   - the Khronos validation layer (VK_LAYER_KHRONOS_validation);
#   - spirv-as (SPIRV-Tools) to assemble the minimal compute shader.
#
# Exit codes:
#   0  test passed
#   1  test failed or the build failed
#   77 requirements are not available (skipped, not a pass)
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../../.." && pwd)"
OUT_DIR="${TMPDIR:-/tmp}/kaizen-vulkan-tests"
mkdir -p "${OUT_DIR}"

skip() {
	echo "SKIP: $1"
	exit 77
}

CXX="${CXX:-clang++}"
SPIRV_AS="${SPIRV_AS:-$(command -v spirv-as || true)}"

[ -n "${SPIRV_AS}" ] || skip "spirv-as (SPIRV-Tools) is not installed"

VULKAN_LIB="${VULKAN_LIB:-}"
if [ -z "${VULKAN_LIB}" ]; then
	for candidate in \
		"$(pkg-config --variable=libdir vulkan 2>/dev/null)/libvulkan.so" \
		/usr/lib/x86_64-linux-gnu/libvulkan.so \
		/usr/lib64/libvulkan.so \
		/lib64/libvulkan.so.1 \
		/usr/lib/libvulkan.so; do
		if [ -n "${candidate}" ] && [ -e "${candidate}" ]; then
			VULKAN_LIB="${candidate}"
			break
		fi
	done
fi
[ -n "${VULKAN_LIB}" ] || skip "Vulkan loader library not found"

"${SPIRV_AS}" --target-env vulkan1.2 \
	-o "${OUT_DIR}/simple.spv" "${SCRIPT_DIR}/simple.comp.spvasm"

"${CXX}" -std=c++17 -O1 -g -Wall -Wextra -I"${REPO_ROOT}" \
	"${SCRIPT_DIR}/pipeline_lifetime_vulkan.cpp" \
	"${REPO_ROOT}/kaizen/rendering/pipeline_cache/pipeline_lru.cpp" \
	"${REPO_ROOT}/kaizen/rendering/pipeline_cache/pipeline_registry.cpp" \
	"${REPO_ROOT}/kaizen/rendering/pipeline_cache/pipeline_retirement.cpp" \
	"${REPO_ROOT}/kaizen/rendering/pipeline_cache/pipeline_submission.cpp" \
	"${VULKAN_LIB}" \
	-o "${OUT_DIR}/pipeline_lifetime_vulkan"

"${OUT_DIR}/pipeline_lifetime_vulkan" "${OUT_DIR}/simple.spv"
