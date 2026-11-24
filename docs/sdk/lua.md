# Lua SDK (Scripting)

KaizenEngine bundles the Lua 5.5 interpreter as the foundation for a future
first-class Lua script language (alongside GDScript and C#).

## Status

Phase 0: the dependency is downloaded, validated and compiled into the
engine. The script language implementation (ScriptLanguage/Script/
ScriptInstance + editor integration) is a later phase — see
`modules/lua_script/README.md` for the roadmap.

## Dependency

| | |
|---|---|
| Component | Lua 5.5.1 (reference sources, flat layout) |
| Source | https://github.com/lua/lua tag `v5.5.1` |
| License | MIT — redistribution permitted |
| Location | `thirdparty/lua/lua/` (gitignored, fetched on demand) |

```bash
python3 tools/sdk/sdk_manager.py --download lua/lua
python3 tools/sdk/sdk_manager.py --validate lua/lua
```

The sdk_manager pins the tag through `acquisition_ref` in
`tools/sdk/manifest/lua_sdks.json`, so the fetched version always matches
the manifest instead of the repository's default branch.

## Build

The `modules/lua_script` module compiles the embeddable library (33 sources:
everything except the standalone `lua.c`/`luac.c` mains and the `onelua.c`
amalgamation):

```bash
./build.sh lua      # just this group
./build.sh smoke    # runtime smoke test (PhysX + Lua)
```

## Runtime smoke

`tests/kaizen/lua_smoke.c` creates a `lua_State`, opens the standard
libraries and executes real Lua (arithmetic through the C API, tables,
strings, error statuses, coroutine yield/resume). It runs in CI on every
push/PR.
