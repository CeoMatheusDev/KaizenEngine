/**************************************************************************/
/*  lua_bridge.h                                                          */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#pragma once

#include "core/object/ref_counted.h"
#include "core/string/ustring.h"
#include "core/variant/variant.h"

#include <string>

extern "C" {
#include "lauxlib.h"
#include "lua.h"
#include "lualib.h"
}

// Userdata that carries a Variant into Lua. Values without a natural Lua counterpart
// (objects, math types, Array, Dictionary, Callable, ...) travel inside one of these,
// and the metatable `LuaBridge::VARIANT_METATABLE` maps member access, indexing,
// operators and calls onto the engine.
struct LuaVariantBox {
	Variant value;
	// Keeps a RefCounted object alive while Lua holds it. Not set for `self`, because the
	// owner would then be kept alive by its own script environment.
	Ref<RefCounted> keepalive;
};

namespace LuaBridge {

inline constexpr const char *VARIANT_METATABLE = "kaizen.Variant";

// Creates the Variant metatable in the registry. Must run once per lua_State.
void register_metatable(lua_State *L);

// Pushes `p_value` as a Lua value: nil, boolean, integer, number and string map to their
// Lua counterparts; everything else is wrapped in a LuaVariantBox.
void push_variant(lua_State *L, const Variant &p_value, bool p_keep_alive = true);

// Reads the Lua value at `p_index` as a Variant. Lua tables become Array (when they are a
// sequence 1..n) or Dictionary; Lua functions become Callable values.
Variant to_variant(lua_State *L, int p_index);

bool is_variant_box(lua_State *L, int p_index);

// Protected call with a traceback message handler. The function and its `p_nargs`
// arguments must be on top of the stack. On failure the error message is left on top.
int pcall(lua_State *L, int p_nargs, int p_nresults);

// Copies the string at the top of the stack into a String without popping it.
String top_string(lua_State *L);

// UTF-8 conversions between Godot strings and the standard library strings used by the
// metadata reader.
std::string to_std_utf8(const String &p_text);
String from_std_utf8(const std::string &p_text);

} // namespace LuaBridge
