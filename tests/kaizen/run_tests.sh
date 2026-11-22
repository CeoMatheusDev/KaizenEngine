#!/usr/bin/env bash
# Builds and runs the standalone Kaizen subsystem tests.
# These tests do not require an engine build: the core depends only on the
# C++ standard library.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
OUT_DIR="${TMPDIR:-/tmp}/kaizen-tests"
mkdir -p "${OUT_DIR}"

CXX="${CXX:-g++}"
CXXFLAGS=(-std=c++17 -O2 -Wall -Wextra -pthread -I"${REPO_ROOT}")

RESOURCE_BIN="${OUT_DIR}/test_resource_registry"
PIPELINE_BIN="${OUT_DIR}/test_pipeline_registry"

"${CXX}" "${CXXFLAGS[@]}" \
	"${SCRIPT_DIR}/test_resource_registry.cpp" \
	"${REPO_ROOT}/kaizen/diagnostics/rendering/resource_registry.cpp" \
	-o "${RESOURCE_BIN}"

"${CXX}" "${CXXFLAGS[@]}" \
	"${SCRIPT_DIR}/test_pipeline_registry.cpp" \
	"${REPO_ROOT}/kaizen/rendering/pipeline_cache/pipeline_lru.cpp" \
	"${REPO_ROOT}/kaizen/rendering/pipeline_cache/pipeline_registry.cpp" \
	"${REPO_ROOT}/kaizen/rendering/pipeline_cache/pipeline_retirement.cpp" \
	"${REPO_ROOT}/kaizen/rendering/pipeline_cache/pipeline_submission.cpp" \
	"${REPO_ROOT}/kaizen/rendering/pipeline_cache/pipeline_budget.cpp" \
	-o "${PIPELINE_BIN}"

# The HZB occlusion test compiles function bodies lifted out of the engine sources,
# because those translation units need the generated headers this suite exists to
# avoid. The extraction step is part of the build: if an engine function moves, it
# fails here rather than testing nothing.
HZB_INC="${OUT_DIR}/hzb_extracted.inc"
HZB_BIN="${OUT_DIR}/test_hzb_occlusion"

python3 "${SCRIPT_DIR}/hzb_occlusion/extract_source_bodies.py" \
	--repo-root "${REPO_ROOT}" \
	--out "${HZB_INC}"

"${CXX}" "${CXXFLAGS[@]}" -I"${OUT_DIR}" \
	"${SCRIPT_DIR}/hzb_occlusion/test_hzb_occlusion.cpp" \
	-o "${HZB_BIN}"

"${RESOURCE_BIN}" "$@"
"${PIPELINE_BIN}" "$@"
"${HZB_BIN}" "$@"
