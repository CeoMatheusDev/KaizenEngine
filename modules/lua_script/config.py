name = "lua_script"
description = "Lua 5.5 scripting language for KaizenEngine (bundled interpreter; language implementation in progress)"
author = "Hardion Labs"
version = "0.1.0"
url = "https://github.com/CeoMatheusDev/KaizenEngine"
license = "AGPL-3.0"
dependencies = []


def can_build(env, platform):
    import os
    if not os.path.exists("thirdparty/lua/lua/lua.h"):
        print("Lua 5.5 sources not found - run: python3 tools/sdk/sdk_manager.py --download lua/lua")
        return False
    return True


def configure(env):
    pass
