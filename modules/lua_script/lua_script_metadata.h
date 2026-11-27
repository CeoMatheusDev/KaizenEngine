/**************************************************************************/
/*  lua_script_metadata.h                                                 */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#pragma once

// Pure C++ (standard library only) reader for the metadata a Lua script declares.
//
// The Lua script language keeps its declarations in `--@` comment annotations and in
// top-level `function` definitions, so the engine can learn about a script (its base
// class, exported variables, signals and methods) without running it. Keeping this
// logic free of engine types lets `tests/kaizen` compile and check it on its own.

#include <string>
#include <vector>

namespace LuaScriptMetadata {

struct Function {
	std::string name;
	std::vector<std::string> params;
	int line = 0; // One-based.
};

struct Export {
	std::string name;
	std::string type; // Empty when the annotation has no `: type`.
	std::string default_expr; // Empty when the annotation has no `= expr`.
	int line = 0;
};

struct Signal {
	std::string name;
	std::vector<std::string> params;
	int line = 0;
};

struct Metadata {
	bool tool = false;
	std::string extends; // Native base class, e.g. "Node2D". Empty when not declared.
	std::string class_name; // Global class name. Empty when not declared.
	std::vector<Export> exports;
	std::vector<Function> functions;
	std::vector<Signal> signals;
};

// Reads every annotation and top-level function in `p_source`.
Metadata parse(const std::string &p_source);

// One-based line of the first top-level definition of `p_name`, or -1 when absent.
int find_function_line(const std::string &p_source, const std::string &p_name);

} // namespace LuaScriptMetadata
