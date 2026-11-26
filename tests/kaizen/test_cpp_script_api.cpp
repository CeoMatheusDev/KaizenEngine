/**************************************************************************/
/*  test_cpp_script_api.cpp                                               */
/**************************************************************************/
/*  C++ script module for KaizenEngine.                                   */
/**************************************************************************/

// Standalone check of the C++ script ABI: the API header the engine writes next to each script,
// the KAIZEN_CPP_SCRIPT entry points, and the event dispatch. It builds a script exactly the way
// the engine does (header on the include path, -shared, C++17), loads it with dlopen and drives
// the entry points through the same signatures the engine uses. No engine headers are needed.

#include "modules/cpp_script/cpp_script_api_header.h"

#include <dlfcn.h>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <fstream>
#include <string>
#include <sys/stat.h>
#include <unistd.h>
#include <vector>

namespace {

int failures = 0;

void check(bool p_ok, const char *p_message) {
	std::printf("  [%s] %s\n", p_ok ? "ok" : "FAIL", p_message);
	if (!p_ok) {
		failures++;
	}
}

// What the scripts log, in order. The engine prints these through its own print_line.
std::vector<std::string> g_log;

void test_log(const char *p_message) {
	g_log.push_back(p_message);
}

int g_owner_calls = 0;

int test_call_owner(void *p_owner, const char *p_method) {
	(void)p_owner;
	if (std::strcmp(p_method, "on_ping") == 0) {
		g_owner_calls++;
		return 1;
	}
	return 0;
}

// Event ids and the entry-point types, as the engine declares them in cpp_script.h.
enum { EVENT_ENTER_TREE = 0,
	EVENT_READY = 1,
	EVENT_PROCESS = 2,
	EVENT_PHYSICS_PROCESS = 3,
	EVENT_EXIT_TREE = 4 };
typedef void *(*CreateFn)();
typedef void (*DestroyFn)(void *);
typedef void (*LogFn)(const char *);
typedef int (*CallOwnerFn)(void *, const char *);
typedef void (*EventFn)(void *, int, double, void *, LogFn, CallOwnerFn);
typedef int (*VersionFn)();

const char *SCRIPT_SOURCE = R"KZSRC(#include "kaizen_cpp.h"

class ProbeScript : public kaizen::Script {
public:
	void _enter_tree() override { log("enter"); }
	void _ready() override {
		log("ready");
		call_owner("on_ping");
	}
	void _process(double) override { log("process"); }
	void _exit_tree() override { log("exit"); }
};

KAIZEN_CPP_SCRIPT(ProbeScript)
)KZSRC";

// Compiles the sample script with the engine's API header and returns the path of the library.
std::string build_script(const std::string &p_dir) {
	const std::string include_dir = p_dir + "/include";
	const std::string src_path = p_dir + "/probe.cpp";
	const std::string lib_path = p_dir + "/probe.so";
	mkdir(include_dir.c_str(), 0755);

	{
		std::ofstream header(include_dir + "/kaizen_cpp.h", std::ios::binary);
		header << CppScriptApi::HEADER;
	}
	{
		std::ofstream src(src_path, std::ios::binary);
		src << SCRIPT_SOURCE;
	}
	const char *cxx_env = std::getenv("CXX");
	const std::string compiler = (cxx_env && *cxx_env) ? cxx_env : "g++";
	const std::string cmd = compiler + " -std=c++17 -shared -fPIC -O1 -Wall -Wextra -Werror -I" + include_dir + " -o " + lib_path + " " + src_path;
	if (std::system(cmd.c_str()) != 0) {
		return std::string();
	}
	return lib_path;
}

} // namespace

int main() {
	char tmpl[] = "/tmp/kaizen_cpp_api_XXXXXX";
	char *dir = mkdtemp(tmpl);
	if (!dir) {
		std::printf("FAIL: could not create a temporary directory\n");
		return 1;
	}
	const std::string work = dir;

	std::printf("C++ script API\n");
	const std::string lib_path = build_script(work);
	check(!lib_path.empty(), "the sample script compiles against kaizen_cpp.h with -Wall -Wextra -Werror");
	if (lib_path.empty()) {
		return 1;
	}

	void *handle = dlopen(lib_path.c_str(), RTLD_NOW | RTLD_LOCAL);
	check(handle != nullptr, "the compiled script loads as a shared library");
	if (!handle) {
		std::printf("  %s\n", dlerror());
		return 1;
	}

	VersionFn version = reinterpret_cast<VersionFn>(dlsym(handle, "kaizen_cpp_api_version"));
	CreateFn create = reinterpret_cast<CreateFn>(dlsym(handle, "kaizen_cpp_create"));
	DestroyFn destroy = reinterpret_cast<DestroyFn>(dlsym(handle, "kaizen_cpp_destroy"));
	EventFn event = reinterpret_cast<EventFn>(dlsym(handle, "kaizen_cpp_event"));
	check(version && create && destroy && event, "all four entry points are exported");
	if (!version || !create || !destroy || !event) {
		return 1;
	}
	check(std::strstr(CppScriptApi::HEADER, "#define KAIZEN_CPP_API_VERSION 1") != nullptr, "the header declares API version 1");
	check(version() == 1, "kaizen_cpp_api_version returns the version the header declares");

	void *state = create();
	check(state != nullptr, "kaizen_cpp_create returns an object");

	// Events, in the order the engine sends them for a node: enter, ready, process, exit.
	g_log.clear();
	g_owner_calls = 0;
	int owner_marker = 0;
	event(state, EVENT_ENTER_TREE, 0.0, &owner_marker, test_log, test_call_owner);
	event(state, EVENT_READY, 0.0, &owner_marker, test_log, test_call_owner);
	event(state, EVENT_PROCESS, 0.016, &owner_marker, test_log, test_call_owner);
	event(state, EVENT_EXIT_TREE, 0.0, &owner_marker, test_log, test_call_owner);

	const std::vector<std::string> expected = { "enter", "ready", "process", "exit" };
	check(g_log == expected, "events reach the script in lifecycle order");
	check(g_owner_calls == 1, "call_owner reaches the owner's method once");

	// The engine passes null callbacks when no owner is attached. The event must still be accepted.
	event(state, EVENT_PHYSICS_PROCESS, 0.0, nullptr, nullptr, nullptr);
	check(true, "an event with null callbacks is accepted without a crash");

	destroy(state);

	dlclose(handle);
	std::remove(lib_path.c_str());
	std::remove((work + "/probe.cpp").c_str());
	std::remove((work + "/include/kaizen_cpp.h").c_str());
	rmdir((work + "/include").c_str());
	rmdir(work.c_str());

	if (failures != 0) {
		std::printf("test_cpp_script_api: FAIL (%d failure(s))\n", failures);
		return 1;
	}
	std::printf("test_cpp_script_api: OK\n");
	return 0;
}
