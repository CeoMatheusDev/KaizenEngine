/**************************************************************************/
/*  lua_smoke.c                                                           */
/**************************************************************************/
/*  Lua 5.5 runtime smoke test for KaizenEngine.                          */
/**************************************************************************/

// Runtime smoke test for the bundled Lua 5.5 dependency.
//
// Creates a real lua_State, runs Lua code and checks the results. This proves
// the interpreter the future Lua script language will embed actually works.

#include "lua.h"
#include "lauxlib.h"
#include "lualib.h"

#include <stdio.h>
#include <string.h>

static int failures = 0;

static void check(int ok, const char *what) {
    printf("  [%s] %s\n", ok ? "ok" : "FAIL", what);
    if (!ok) {
        failures++;
    }
}

int main(void) {
    lua_State *L = luaL_newstate();
    check(L != NULL, "luaL_newstate");
    if (L == NULL) {
        return 1;
    }
    luaL_openlibs(L);

    // Version sanity.
    check(LUA_VERSION_MAJOR_N == 5, "major version is 5");
    check(LUA_VERSION_MINOR_N == 5, "minor version is 5");
    printf("  running %s\n", LUA_RELEASE);

    // Execute real Lua and read the result back through the C API.
    int rc = luaL_dostring(L, "return 6 * 7");
    check(rc == LUA_OK, "luaL_dostring (return 6 * 7)");
    check(lua_isinteger(L, -1) && lua_tointeger(L, -1) == 42, "C API reads 42");

    // Tables, loops and the standard library.
    rc = luaL_dostring(L, "t = {}; for i = 1, 100 do t[i] = i * i end; return #t");
    check(rc == LUA_OK && lua_tointeger(L, -1) == 100, "table loop returns 100");

    // String library through the C API.
    rc = luaL_dostring(L, "return string.rep('ab', 3)");
    check(rc == LUA_OK && strcmp(lua_tostring(L, -1), "ababab") == 0, "string.rep via C API");

    // Error handling: a failing chunk reports through luaL_dostring.
    rc = luaL_dostring(L, "error('boom')");
    check(rc != LUA_OK, "runtime error surfaces as a status code");
    lua_pop(L, 1);

    // Coroutines (the yield/resume machinery scripts rely on).
    lua_State *co = lua_newthread(L);
    check(co != NULL, "lua_newthread (coroutine)");
    if (co) {
        int nres = 0;
        luaL_loadstring(co, "coroutine.yield('tick'); return 'done'");
        lua_resume(co, L, 0, &nres);
        check(strcmp(lua_tostring(co, -1), "tick") == 0, "coroutine yields");
        lua_resume(co, L, 0, &nres);
        check(strcmp(lua_tostring(co, -1), "done") == 0, "coroutine resumes");
    }

    lua_close(L);
    check(1, "lua_close");

    if (failures == 0) {
        printf("lua_smoke: ALL CHECKS PASSED\n");
        return 0;
    }
    printf("lua_smoke: %d FAILURE(S)\n", failures);
    return 1;
}
