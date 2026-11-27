#!/bin/bash
# Runtime smoke test for the NVIDIA PhysX SDK build.
#
# Builds the PhysX SDK group (when its objects are missing), compiles
# tests/kaizen/physx_smoke.cpp against those objects and RUNS it: a dynamic
# box must land and settle on a static plane under gravity. This proves the
# SDK the engine links against actually simulates.
#
# Usage: bash tests/kaizen/run_physx_smoke.sh
set -e
set -o pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

CXX="${CXX:-g++}"

# Objects produced by the in-tree SDK build (redirect_build_objects=no keeps
# them next to their sources).
OBJ_COUNT="$(find thirdparty/physx -name '*.o' 2>/dev/null | wc -l)"
if [ "$OBJ_COUNT" -eq 0 ]; then
    echo "PhysX SDK objects not built yet - building the physx_physics_3d group..."
    scons platform=linuxbsd target=editor redirect_build_objects=no physx_physics_3d
    OBJ_COUNT="$(find thirdparty/physx -name '*.o' | wc -l)"
fi
if [ "$OBJ_COUNT" -eq 0 ]; then
    echo "FAIL: no PhysX SDK objects found under thirdparty/physx"
    exit 1
fi
echo "Linking smoke test against $OBJ_COUNT PhysX SDK objects..."

OUT="${TMPDIR:-/tmp}/kaizen_physx_smoke"
# The module TUs (modules/physx_physics_3d) depend on engine symbols; the
# smoke test links only the self-contained SDK objects.
# shellcheck disable=SC2046
# The defines must match modules/physx_physics_3d/SCsub: the SDK headers change their class
# layouts with PX_SUPPORT_GPU_PHYSX and PX_PUBLIC_RELEASE, so a mismatch is an ABI error.
"$CXX" -O1 -std=gnu++17 -DPX_PHYSX_STATIC_LIB -DPX_PUBLIC_RELEASE=1 -DDISABLE_CUDA_PHYSX \
    -I thirdparty/physx/physx/physx/include \
    -I thirdparty/physx/physx/source \
    tests/kaizen/physx_smoke.cpp \
    $(find thirdparty/physx -name '*.o') \
    -lpthread -o "$OUT"

echo "Running PhysX runtime smoke test..."
"$OUT"
echo "run_physx_smoke: OK"
