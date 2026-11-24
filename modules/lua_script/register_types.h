/**************************************************************************/
/*  register_types.h                                                      */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#ifndef LUA_SCRIPT_REGISTER_TYPES_H
#define LUA_SCRIPT_REGISTER_TYPES_H
#include "modules/register_module_types.h"
void initialize_lua_script_module(ModuleInitializationLevel p_level);
void uninitialize_lua_script_module(ModuleInitializationLevel p_level);
#endif // LUA_SCRIPT_REGISTER_TYPES_H
