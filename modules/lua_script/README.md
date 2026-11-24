# Lua Script Module

Lua 5.5 scripting for KaizenEngine — intended to work alongside GDScript and
C# as a first-class script language.

## Status

**Phase 0 (this round): dependency.** The bundled Lua 5.5 interpreter sources
are downloaded, validated and compiled into the engine. The script language
implementation itself (ScriptLanguage, Script, ScriptInstance and the editor
integration) is the next phase and is not started yet.

## Getting the dependency

```bash
python3 tools/sdk/sdk_manager.py --download lua/lua   # Lua 5.5.1 (MIT)
```

Sources land in `thirdparty/lua/lua/` (the flat reference layout of
https://github.com/lua/lua at tag v5.5.1). The module's `can_build` reports
the download command when the sources are missing.

## Build

```bash
./build.sh lua      # just this module's group
scons lua_script    # same, via the dev alias
```

The SCsub compiles the full Lua library (`lapi.c` … `lvm.c`, 33 sources)
excluding the standalone `lua.c`/`luac.c` mains. Warnings from the vendored
C sources are silenced; module code builds with the engine's warning flags.

## Verify

```bash
./build.sh smoke    # includes the Lua runtime smoke test
bash tests/kaizen/run_lua_smoke.sh
```

The smoke test creates a `lua_State`, runs real Lua code and checks results —
proof the bundled interpreter works.

## Roadmap (future rounds)

1. `LuaScript` / `LuaScriptInstance` implementing `Script`/`ScriptInstance`.
2. `LuaLanguage` registering with `ScriptServer` (like `GDScriptLanguage`).
3. Variant <-> Lua value binding (the C API glue for Object/RefCounted).
4. Editor support: syntax highlighting, completion, debugger hooks.
5. `class_name`/preload semantics parity with GDScript where it makes sense.

## License

Lua 5.5 is MIT (see `thirdparty/lua/lua` README). Module code is AGPL-3.0,
like the rest of KaizenEngine.
