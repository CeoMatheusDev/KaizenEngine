# Lua Script Module

Lua 5.5 scripting for KaizenEngine. Lua is the engine's primary script language.

## Runtime and module support

- The module builds the complete Lua 5.5.1 runtime library from the pinned
  upstream source files. It uses `luaL_openlibs`, so the normal Lua standard
  libraries are available; Kaizen does not replace them with a reduced Lua
  dialect. Scripts run as text chunks in per-instance environments.
- `LuaScript`/`LuaScriptInstance` are registered with `ScriptServer`; `.lua`
  resources load and save through the engine resource APIs.
- `lua_bridge.cpp` maps engine Variants and objects to Lua userdata and maps
  Lua tables/functions back to Godot `Array`/`Dictionary`/`Callable` values.
  Nested tables are no longer rejected at a fixed depth; active-path detection
  prevents infinite traversal of cyclic tables, which Godot Variants cannot
  represent. Object and Variant property access, methods, operators, `pairs`
  and `#` are bridged. Unknown members produce Lua errors rather than silently
  returning nil (except missing Dictionary keys).
- Editor language integration performs syntax validation and function lookup.
  It does not currently provide the full debugger/IDE feature set of the
  upstream GDScript editor integration.
- Kaizen-specific declarations use `--@extends`, `--@class_name`, `--@tool`,
  `--@export` and `--@signal`; these are resource/editor metadata conventions,
  not changes to Lua syntax. See *Script conventions*.

Runtime-verified (engine binary, headless): script load, `class_name` and
`extends`, method calls, `@export` defaults, Variant arithmetic and
conversions, Callable round trips, error propagation with a traceback,
`free()` without leak warnings, and the three churn projects in
`tests/kaizen/`. The GPU-side resource and pipeline counters need a GPU and
were not checked in the headless sandbox.

## Script conventions

- `--@extends Node2D` sets the native base class (default `Object`).
- `--@class_name Name` declares a global class name.
- `--@tool` marks the script as a tool script.
- `--@export name: type = expr` declares an exported member. The default
  expression is evaluated once per instance, unless the script set the value
  itself first.
- `--@signal name(params)` declares a signal.
- A column-zero `function name(...)` (or `name = function(...)`) is a script
  method. Methods are globals of the instance environment and are called
  without a receiver. The owning object is available as `self`.
- `_init` runs once, when the instance is created. Notifications dispatch to
  `_enter_tree`, `_ready`, `_process(delta)`, `_physics_process(delta)` and
  `_exit_tree`.
- `print` writes to the Godot output.

## Engine access

- `Vector2(...)`, `Vector3(...)` and `Color(...)` construct Variants. Their
  constants and static methods are reached by indexing the type:
  `Vector2.ZERO`, `Color.from_hsv(h, s, v)`.
- An engine class name used as a global is a class table:
  - integer constants: `BaseMaterial3D.TRANSPARENCY_ALPHA`, `Image.FORMAT_RGBA8`
  - constructors: `Sprite2D.new()` or `new("Sprite2D")`
  - static methods: `Image.create(w, h, false, Image.FORMAT_RGBA8)`
  - instance methods of a singleton: `RenderingServer.get_rendering_info(...)`
- `get_singleton("PhysicsServer3D")` returns an engine singleton, or nil.
- `randf_range(from, to)` is the GDScript helper.

Errors are raised as Lua errors and carry the Godot class and member name:
`'Vector2' has no constant or static method named 'nope'.` A missing key in a
Dictionary is nil, as for a Lua table. A Lua function that raises, when called
through a Callable, reports its error to the Godot error log and returns nil,
as GDScript does; the caller is not interrupted.

## GDScript removed

The GDScript module (`modules/gdscript/`) has been removed from the tree. Lua
is the only script language available in the engine.

Engine code that used to be guarded by `MODULE_GDSCRIPT_ENABLED` (the
`--gdscript-docs` and `--lsp-port` command-line flags, the GDScript language
server settings, and the GDScript syntax highlighter in the class reference)
has been removed entirely. The class reference documentation no longer
contains GDScript examples.

Projects that still use `.gd` scripts will not load. All sample and test
projects in `tests/kaizen/` use Lua.

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

## Test

```bash
bash tests/kaizen/run_lua_smoke.sh          # the bundled Lua 5.5 library, through the C API
bash tests/kaizen/run_lua_engine_test.sh    # the bridge, inside the engine (tests/kaizen/lua_engine_probe.lua)
bash tests/kaizen/run_tests.sh              # includes the metadata parser tests (test_lua_metadata)
```

`run_lua_engine_test.sh` needs the editor binary built with the module. It
prints `LUA_ENGINE_PROBE: PASS (N failure(s))` and exits non-zero on failure.

The churn projects (`tests/kaizen/churn_project`, `p251_dedup_project`,
`pipeline_churn_project`) are Lua ports of the former GDScript scenes. Run them
with the command in `tests/kaizen/README.md`; the GPU diagnostics need a GPU.

## Known engine-integration gaps

The embedded Lua VM and its standard language libraries are upstream Lua 5.5.1;
limitations listed here concern Kaizen's editor/engine adapter, not Lua language
features. Cyclic Lua table references have no direct Godot Variant equivalent;
the bridge reports them and substitutes nil at the recursive edge. Deep acyclic
tables are supported beyond the previous 32-level cutoff (subject to normal
process memory and call-stack limits). The module does not yet implement IDE syntax highlighting/completion,
a Lua debugger, script-to-script inheritance, or full argument-count diagnostics
for a Lua function called through a Godot `Callable`. Lua `--@signal` declarations
are exposed through the engine's Script signal interface and can be connected
and emitted by the owning object. These adapter gaps do not restrict standalone
Lua syntax, coroutines, metatables, modules, or the standard libraries.
