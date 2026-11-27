#!/bin/bash
# Engine-level regression test for the C++ script language.
#
# Writes three C++ fixtures into a temporary project, runs tests/kaizen/cpp_engine_probe.lua
# headless as the main loop, and checks two things: the probe reports PASS, and the engine logs
# the expected "[C++] ..." lines and errors. Any other engine error fails the run.
#
# Needs the editor binary built with the cpp_script module (enabled by default) and a C++
# compiler on PATH, or KAIZEN_CPP_COMPILER / CXX pointing at one.
#
# Usage: bash tests/kaizen/run_cpp_engine_test.sh [path/to/editor-binary]
set -o pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

BIN="${1:-$ROOT/bin/kaizen.linuxbsd.editor.x86_64}"
if [ ! -x "$BIN" ]; then
    echo "FAIL: editor binary not found at $BIN"
    echo "Build it first: scons platform=linuxbsd target=editor module_gdscript_enabled=no -j2"
    exit 1
fi

WORK="$(mktemp -d "${TMPDIR:-/tmp}/kaizen_cpp_engine.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT

# Kaizen identifies a project by project.kaizen.
cat > "$WORK/project.kaizen" <<'EOF_PROJECT'
config_version=5

[application]
config/name="cpp_engine_probe"
EOF_PROJECT

cat > "$WORK/cpp_ok.cpp" <<'EOF_OK'
#include "kaizen_cpp.h"

class OkScript : public kaizen::Script {
public:
	void _enter_tree() override { log("enter_tree v1"); }
	void _ready() override { log("ready v1"); }
	void _process(double p_delta) override {
		if (!logged_process) {
			logged_process = true;
			log("process first");
		}
	}
	void _exit_tree() override { log("exit_tree v1"); }

private:
	bool logged_process = false;
};

KAIZEN_CPP_SCRIPT(OkScript)
EOF_OK

cat > "$WORK/cpp_broken.cpp" <<'EOF_BROKEN'
#include "kaizen_cpp.h"

class Broken : public kaizen::Script {
	void _ready() override { this is not C++ }
};

KAIZEN_CPP_SCRIPT(Broken)
EOF_BROKEN

cat > "$WORK/cpp_noentry.cpp" <<'EOF_NOENTRY'
#include "kaizen_cpp.h"

class NoEntry : public kaizen::Script {
};
EOF_NOENTRY

cp "$ROOT/tests/kaizen/cpp_engine_probe.lua" "$WORK/"

LOG="$WORK/run.log"
timeout 300 "$BIN" --headless --path "$WORK" --script "$WORK/cpp_engine_probe.lua" > "$LOG" 2>&1
RC=$?
grep -E "^\s+\[(ok|FAIL)\]|CPP_ENGINE_PROBE|\[C\+\+\]" "$LOG" || true

# Errors the fixtures provoke on purpose: a compile failure (first load, and the broken reload),
# and a script without KAIZEN_CPP_SCRIPT. Anything else fails the run.
UNEXPECTED=$(grep -E "SCRIPT ERROR|^ERROR:" "$LOG" \
    | grep -v "Invalid setter" \
    | grep -v "C++ compilation failed for res://cpp_broken.cpp" \
    | grep -v "C++ compilation failed for res://cpp_ok.cpp" \
    | grep -v "does not call KAIZEN_CPP_SCRIPT" || true)

if [ "$RC" -ne 0 ]; then
    echo "FAIL: probe exited with status $RC"
    exit 1
fi
if [ -n "$UNEXPECTED" ]; then
    echo "FAIL: unexpected engine or script error:"
    echo "$UNEXPECTED"
    exit 1
fi
if ! grep -q "CPP_ENGINE_PROBE: PASS" "$LOG"; then
    echo "FAIL: C++ engine probe did not pass"
    exit 1
fi
# Lines the scripts must print, in lifecycle order.
EXPECTED=(
    "[C++] enter_tree v1"
    "[C++] ready v1"
    "[C++] process first"
    "[C++] enter_tree v2"
    "[C++] ready v2"
    "[C++] exit_tree v2"
)
LAST=0
for line in "${EXPECTED[@]}"; do
    N=$(grep -n -F "$line" "$LOG" | head -n1 | cut -d: -f1)
    if [ -z "$N" ]; then
        echo "FAIL: missing script output: $line"
        exit 1
    fi
    if [ "$N" -lt "$LAST" ]; then
        echo "FAIL: script output out of order: $line"
        exit 1
    fi
    LAST=$N
done
echo "run_cpp_engine_test: OK"
