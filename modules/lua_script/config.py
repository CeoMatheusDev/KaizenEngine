name = "lua_script"
description = "Lua 5.5 script language for KaizenEngine (Script resources, instances, editor validation, and engine bindings)"
author = "Hardion Labs"
version = "0.1.0"
url = "https://github.com/CeoMatheusDev/KaizenEngine"
license = "AGPL-3.0"
dependencies = []


def can_build(env, platform):
    import os

    lua_root = "thirdparty/lua/lua"
    required_files = ("lua.h", "lauxlib.h", "lualib.h", "lapi.c", "lstate.c", "lvm.c")
    missing = [name for name in required_files if not os.path.isfile(os.path.join(lua_root, name))]
    if missing:
        print(
            "Lua 5.5 sources are missing or incomplete ({}). Run: "
            "python3 tools/sdk/sdk_manager.py --download lua/lua".format(", ".join(missing))
        )
        return False
    return True


def configure(env):
    pass
