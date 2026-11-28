# Lua 5.5 SDK and script language

Lua 5.5.1 is the script language provided by Kaizen's `modules/lua_script` module. The module implements `ScriptLanguage`, `Script`, and per-object `ScriptInstance` resources, registers `.lua` loading/saving, and exposes engine Variants and objects through the Lua bridge. Deep acyclic table conversion no longer has the adapter's former fixed 32-level cutoff; cycles are detected because Godot Variant containers cannot represent cyclic graphs. See `modules/lua_script/README.md` for syntax, bindings, supported annotations, and current limitations.

## Dependency

| | |
|---|---|
| Component | Lua 5.5.1 reference sources (flat layout) |
| Source | `https://github.com/lua/lua`, tag `v5.5.1` |
| License | MIT |
| Location | `thirdparty/lua/lua/` (fetched locally; not committed) |

Fetch and validate the pinned checkout:

```bash
python3 tools/sdk/sdk_manager.py --download lua/lua
python3 tools/sdk/sdk_manager.py --validate lua/lua
```

## Build and test

```bash
./build.sh lua
bash tests/kaizen/run_lua_smoke.sh
```

The standalone smoke test executes real Lua through its C API. The engine integration probe additionally exercises `.lua` loading, script instances, callbacks, exported values, Variants, engine object access, and error handling; it requires an editor binary built with `lua_script`:

```bash
bash tests/kaizen/run_lua_engine_test.sh
```

The metadata parser has a separate no-SDK unit test and is included in `bash tests/kaizen/run_tests.sh`.
