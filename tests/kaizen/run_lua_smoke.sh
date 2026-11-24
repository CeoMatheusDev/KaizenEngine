#!/bin/bash
# Runtime smoke test for the bundled Lua 5.5 dependency.
#
# Compiles the bundled Lua library (minus the standalone lua/luac mains),
# links it with tests/kaizen/lua_smoke.c and RUNS it: a real lua_State must
# execute Lua code and return results through the C API.
#
# Usage: bash tests/kaizen/run_lua_smoke.sh
set -e
set -o pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

CC="${CC:-cc}"
LUA_DIR="thirdparty/lua/lua"

if [ ! -f "$LUA_DIR/lua.h" ]; then
    echo "Lua 5.5 sources not found. Run:"
    echo "  python3 tools/sdk/sdk_manager.py --download lua/lua"
    exit 1
fi

SRCS=""
for f in "$LUA_DIR"/*.c; do
    case "$(basename "$f")" in
        lua.c|luac.c|onelua.c) ;;
        *) SRCS="$SRCS $f" ;;
    esac
done

OUT="${TMPDIR:-/tmp}/kaizen_lua_smoke"
echo "Building Lua runtime smoke test against $(echo "$SRCS" | wc -w) library sources..."
# shellcheck disable=SC2086
"$CC" -O1 -std=gnu99 -I "$LUA_DIR" tests/kaizen/lua_smoke.c $SRCS -lm -o "$OUT"

echo "Running Lua runtime smoke test..."
"$OUT"
echo "run_lua_smoke: OK"
