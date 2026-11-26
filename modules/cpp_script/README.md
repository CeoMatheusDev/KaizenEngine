# C++ Script Module

C++ as a script language for KaizenEngine, next to Lua. A `.cpp` file is a
script: the engine compiles it into a shared library, loads the library, and
reloads it when the file changes.

The engine ships no C++ compiler. Compiling needs clang++ or g++ on the
machine that runs the editor or the game.

## Model

- **One file, one library.** Each `.cpp` script is compiled on its own into a
  shared library. The engine never links the script against the engine
  binary, so a script only sees the API header.
- **Compile on load and on save.** Loading a `.cpp` compiles it. Saving it in
  the editor reloads it. Every compile writes a new library file, so the
  dynamic loader never returns a stale copy.
- **Reload keeps the node, not the state.** A C++ object cannot move between
  libraries, so each object is recreated after a reload. If the node is in the
  tree, the new object gets `_enter_tree` and `_ready` again.
- **Failed reloads keep the old code running.** A compile error is printed as
  an engine error and the last working library stays loaded. The script still
  loads, so the editor can show the file.

## Writing a script

```cpp
#include "kaizen_cpp.h"

class Spinner : public kaizen::Script {
public:
	void _ready() override {
		log("Spinner is ready");
	}

	void _process(double p_delta) override {
		// Runs every frame while the node is in the tree.
	}
};

KAIZEN_CPP_SCRIPT(Spinner)
```

- Derive from `kaizen::Script`. Override any of `_enter_tree`, `_ready`,
  `_process(double)`, `_physics_process(double)` and `_exit_tree`.
- `log(const char *)` prints a line in the engine output, prefixed with `[C++] `.
- `call_owner("method")` calls a method on the owning object, if it has one.
  It returns `true` when the call happened.
- `KAIZEN_CPP_SCRIPT(ClassName)` goes once per file, after the class. It
  exports the entry points the engine calls.
- `kaizen_cpp.h` is written by the engine into its cache on each compile. Do
  not edit it; changes are overwritten.

## Compiler

The engine looks for the compiler in this order:

1. `KAIZEN_CPP_COMPILER`, a path to the compiler.
2. `CXX`.
3. `clang++`, then `g++`, then `c++`, the first one that answers `--version`.

The command line is `-std=c++17 -shared -fPIC -O1 -I<cache>/include`. Files
are cached under `user://cpp_scripts/` (`include/`, `src/`, `bin/`).

## Editor

- `.cpp` files show up as scripts and open in the script editor with the
  standard C++ keyword and string highlighting.
- The function list comes from `void name(` lines. The compiler decides what is
  valid: compile errors show up when the file is saved or loaded.
- Code completion, documentation lookup and the debugger are not implemented.

## Limitations

These are the gaps in version 1 of the API:

- No exported properties and no signals. `has_method` answers only for the
  lifecycle methods.
- No calls from Lua or from other scripts into C++ methods.
- No state carried across a reload.
- No debugger integration: `debug_get_stack_*` report nothing.
- Scripts need C++17. The header is written for the compiler the engine finds,
  so a script that depends on another compiler's extensions may fail to build.

## Testing

`tests/kaizen/run_cpp_engine_test.sh` writes three fixtures (a working script,
a script that does not compile, and a script without `KAIZEN_CPP_SCRIPT`) into
a temporary project. It runs `tests/kaizen/cpp_engine_probe.lua` headless. The
probe checks the language registration, loading and instantiation, a failed
reload that keeps the old library, a successful reload of a live node, and
freeing. The runner then checks that the scripts printed their lines in
lifecycle order and that the engine logged no unexpected errors.

Run it with the editor binary built with this module, for example:

```bash
scons platform=linuxbsd target=editor module_gdscript_enabled=no -j2
bash tests/kaizen/run_cpp_engine_test.sh
```
