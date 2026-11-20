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

"${RESOURCE_BIN}" "$@"
"${PIPELINE_BIN}" "$@"
