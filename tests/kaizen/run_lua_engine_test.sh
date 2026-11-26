#!/bin/bash
# Engine-level regression test for the Lua 5.5 script language.
#
# Runs tests/kaizen/lua_engine_probe.lua headless as the main loop, in a temporary project.
# Requires the editor binary built with the lua_script module (./build.sh lua or ./build.sh physx).
# Fails when any check reports FAIL, the probe exits non-zero, or the engine logs a script error
# that the probe did not expect.
#
# Usage: bash tests/kaizen/run_lua_engine_test.sh [path/to/editor-binary]
set -o pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

BIN="${1:-$ROOT/bin/kaizen.linuxbsd.editor.x86_64}"
if [ ! -x "$BIN" ]; then
    echo "FAIL: editor binary not found at $BIN"
    echo "Build it first: ./build.sh lua -j2"
    exit 1
fi

WORK="$(mktemp -d "${TMPDIR:-/tmp}/kaizen_lua_engine.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT

# Kaizen identifies a project by project.kaizen.
cat > "$WORK/project.kaizen" <<'EOF'
config_version=5

[application]
config/name="lua_engine_probe"
EOF
cp "$ROOT/tests/kaizen/lua_engine_probe.lua" "$WORK/"

LOG="$WORK/run.log"
timeout 120 "$BIN" --headless --path "$WORK" --script "$WORK/lua_engine_probe.lua" > "$LOG" 2>&1
RC=$?
grep -E "^\s+\[(ok|FAIL)\]|LUA_ENGINE_PROBE|SCRIPT ERROR|ERROR:" "$LOG" | grep -v "Invalid setter" || true

# Errors the probe provokes on purpose: a missing singleton, and a Lua error raised inside a
# Callable (checked to return nil, not to abort). Any other engine or script error fails the run.
UNEXPECTED=$(grep -E "SCRIPT ERROR|^ERROR:" "$LOG" \
    | grep -v "Invalid setter" \
    | grep -v "non-existent singleton 'NoSuchSingletonXyz'" \
    | grep -v "expected failure from a Callable" || true)

if [ "$RC" -ne 0 ]; then
    echo "FAIL: probe exited with status $RC"
    exit 1
fi
if [ -n "$UNEXPECTED" ]; then
    echo "FAIL: unexpected engine or script error:"
    echo "$UNEXPECTED"
    exit 1
fi
if grep -q "\[FAIL\]" "$LOG"; then
    echo "FAIL: one or more Lua engine checks failed"
    exit 1
fi
echo "run_lua_engine_test: OK"
