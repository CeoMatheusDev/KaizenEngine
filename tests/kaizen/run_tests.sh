#!/usr/bin/env bash
# Builds and runs the standalone Kaizen resource registry tests.
# These tests do not require an engine build: the registry core depends only
# on the C++ standard library.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
OUT_DIR="${TMPDIR:-/tmp}/kaizen-tests"
mkdir -p "${OUT_DIR}"

BIN="${OUT_DIR}/test_resource_registry"

CXX="${CXX:-g++}"
"${CXX}" -std=c++17 -O2 -Wall -Wextra -pthread \
	-I"${REPO_ROOT}" \
	"${SCRIPT_DIR}/test_resource_registry.cpp" \
	"${REPO_ROOT}/kaizen/diagnostics/rendering/resource_registry.cpp" \
	-o "${BIN}"

"${BIN}" "$@"
