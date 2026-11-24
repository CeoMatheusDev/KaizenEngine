#!/usr/bin/env bash
# KaizenEngine build entry point.
#
# Hides the platform plumbing: the engine targets the "linuxbsd" family -
# one shared POSIX/X11/Wayland backend for Linux and the BSDs - so callers
# just ask for what they want, not how the scons invocation is spelled.
#
# Usage:
#   ./build.sh                 Build the editor (default).
#   ./build.sh engine [args]   Build the editor; extra args pass to scons.
#   ./build.sh release [args]  Build the release template.
#   ./build.sh physx           Build just the PhysX SDK group.
#   ./build.sh lua             Build just the Lua script group.
#   ./build.sh test            Run the test suite (license + engine checks).
#   ./build.sh smoke           Run the runtime smoke tests (PhysX + Lua).
#   ./build.sh help            Show this help.
set -euo pipefail
cd "$(dirname "$0")"

COMMON="platform=linuxbsd redirect_build_objects=no"

case "${1:-engine}" in
    engine|editor)
        shift || true
        exec scons $COMMON target=editor "$@"
        ;;
    release)
        shift || true
        exec scons $COMMON target=template_release "$@"
        ;;
    physx)
        shift || true
        exec scons $COMMON target=editor physx_physics_3d "$@"
        ;;
    lua)
        shift || true
        exec scons $COMMON target=editor lua_script "$@"
        ;;
    test)
        shift || true
        exec bash tests/kaizen/run_tests.sh "$@"
        ;;
    smoke)
        shift || true
        bash tests/kaizen/run_physx_smoke.sh "$@"
        exec bash tests/kaizen/run_lua_smoke.sh "$@"
        ;;
    help|-h|--help)
        sed -n '2,15p' "$0" | sed 's/^# \{0,1\}//'
        exit 0
        ;;
    *)
        echo "build.sh: unknown command '$1' (try: ./build.sh help)" >&2
        exit 2
        ;;
esac
