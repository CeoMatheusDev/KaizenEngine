#!/bin/bash
# Engine-level regression test for the PhysX 3D backend.
#
# Runs tests/kaizen/physx_engine_probe.lua headless in a temporary project whose
# physics/3d/physics_engine is "PhysX Physics". Requires the editor binary built
# with the physx_physics_3d module (./build.sh physx). Fails when the probe fails
# or when the engine logs an area/space error (the regression signature).
#
# Usage: bash tests/kaizen/run_physx_engine_test.sh [path/to/editor-binary]
set -o pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

BIN="${1:-$ROOT/bin/kaizen.linuxbsd.editor.x86_64}"
if [ ! -x "$BIN" ]; then
    echo "FAIL: editor binary not found at $BIN"
    echo "Build it first: ./build.sh physx -j2"
    exit 1
fi

WORK="$(mktemp -d "${TMPDIR:-/tmp}/kaizen_physx_engine.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT

# Kaizen identifies a project by project.kaizen (not project.godot).
cat > "$WORK/project.kaizen" <<'EOF'
config_version=5

[application]
config/name="physx_engine_probe"

[physics]
3d/physics_engine="PhysX Physics"
EOF
cp "$ROOT/tests/kaizen/physx_engine_probe.lua" "$WORK/"

LOG="$WORK/run.log"
timeout 180 "$BIN" --headless --path "$WORK" --script "$WORK/physx_engine_probe.lua" > "$LOG" 2>&1
RC=$?
cat "$LOG" | grep -E "^\s+\[(ok|FAIL)\]|PHYSX_ENGINE_PROBE|SCRIPT ERROR" || true

if [ "$RC" -ne 0 ]; then
    echo "FAIL: probe exited with status $RC"
    exit 1
fi
if grep -q 'Parameter "area" is null' "$LOG"; then
    echo "FAIL: engine reported a null area parameter (space gravity not routed)"
    exit 1
fi
echo "run_physx_engine_test: OK"
