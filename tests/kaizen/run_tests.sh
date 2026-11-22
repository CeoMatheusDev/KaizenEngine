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

# The pipeline lifetime report helpers are pure formatting and tallying, so they are
# tested here rather than only inside the device test, which skips without a GPU.
REPORT_BIN="${OUT_DIR}/test_pipeline_lifetime_report"

"${CXX}" "${CXXFLAGS[@]}" \
	"${SCRIPT_DIR}/vulkan/test_pipeline_lifetime_report.cpp" \
	-o "${REPORT_BIN}"

# Both sides of the sanitizer question, not one: the report has to say "NOT BUILT"
# without the macro and "built with X" with it, so both builds are checked. Neither of
# these two lines links a sanitizer - that is run_vulkan_tests.sh's job, on a machine
# that asked for it - and the test asserts the wording, not the world.
"${CXX}" "${CXXFLAGS[@]}" -DKAIZEN_TEST_SANITIZERS=\"address,undefined\" \
	"${SCRIPT_DIR}/vulkan/test_pipeline_lifetime_report.cpp" \
	-o "${REPORT_BIN}_sanmarked"

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

# C# glue consistency: the generated bindings are tracked in-tree and maintained by
# hand, so the four sources that have to agree are compared instead of trusted.
python3 "${SCRIPT_DIR}/api_glue/check_api_glue.py" --repo-root "${REPO_ROOT}"
python3 -m unittest discover -s "${SCRIPT_DIR}/api_glue" -p 'test_*.py' 2>&1 | tail -3

# Licence banner geometry: the box rows are padding-held, and a rebrand that lengthens a
# row without moving a space is invisible to the tool that writes them - its template was
# ragged too. The budget is the recorded debt of rows that cannot be padded back without
# touching licence text; lowering it is the point, raising it needs a sentence in
# docs/license-notice-consistency.md.
python3 "${SCRIPT_DIR}/license_box/check_license_box.py" --repo-root "${REPO_ROOT}" --max-pending 2469
python3 -m unittest discover -s "${SCRIPT_DIR}/license_box" -p 'test_*.py' 2>&1 | tail -3

"${REPORT_BIN}" "$@"
"${REPORT_BIN}_sanmarked" "$@"
"${RESOURCE_BIN}" "$@"
"${PIPELINE_BIN}" "$@"
"${HZB_BIN}" "$@"
