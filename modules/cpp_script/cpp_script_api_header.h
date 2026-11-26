/**************************************************************************/
/*  cpp_script_api_header.h                                               */
/**************************************************************************/
/*  C++ script module for KaizenEngine.                                   */
/**************************************************************************/

#pragma once

// The C++ API header the engine writes next to every compiled script. It is embedded in the
// engine so that scripts compile without the engine headers; the ABI between the engine and a
// script is C linkage plus plain function pointers.

namespace CppScriptApi {

static const char *const HEADER = R"KZHDR(#pragma once

// Kaizen C++ script API, version 1. The engine writes this file on every compile; do not
// edit it, your changes are overwritten.
//
// A script is one .cpp file: derive from kaizen::Script, then end the file with
// KAIZEN_CPP_SCRIPT(YourClass). The file is compiled to a shared library and reloaded on save.

#define KAIZEN_CPP_API_VERSION 1

#if defined(_WIN32)
#define KAIZEN_CPP_EXPORT extern "C" __declspec(dllexport)
#else
#define KAIZEN_CPP_EXPORT extern "C" __attribute__((visibility("default")))
#endif

namespace kaizen {

typedef void (*LogFn)(const char *);
typedef int (*CallOwnerFn)(void *, const char *);

class Script {
public:
	virtual ~Script() = default;

	// Lifecycle. Each one runs when the owning node reaches the matching point in the tree.
	virtual void _enter_tree() {}
	virtual void _ready() {}
	virtual void _process(double /*p_delta*/) {}
	virtual void _physics_process(double /*p_delta*/) {}
	virtual void _exit_tree() {}

	// Prints a line in the engine output.
	void log(const char *p_message) const {
		if (log_fn) {
			log_fn(p_message);
		}
	}

	// Calls `p_method` on the owning object if it has it. Returns true when it was called.
	bool call_owner(const char *p_method) const {
		return call_owner_fn != nullptr && call_owner_fn(owner, p_method) != 0;
	}

	// Set by the engine for the duration of each event.
	void *owner = nullptr;
	LogFn log_fn = nullptr;
	CallOwnerFn call_owner_fn = nullptr;
};

namespace detail {
enum Event {
	EVENT_ENTER_TREE = 0,
	EVENT_READY = 1,
	EVENT_PROCESS = 2,
	EVENT_PHYSICS_PROCESS = 3,
	EVENT_EXIT_TREE = 4,
};

inline void dispatch(Script *p_script, int p_event, double p_delta, void *p_owner, LogFn p_log, CallOwnerFn p_call_owner) {
	p_script->owner = p_owner;
	p_script->log_fn = p_log;
	p_script->call_owner_fn = p_call_owner;
	switch (p_event) {
		case EVENT_ENTER_TREE:
			p_script->_enter_tree();
			break;
		case EVENT_READY:
			p_script->_ready();
			break;
		case EVENT_PROCESS:
			p_script->_process(p_delta);
			break;
		case EVENT_PHYSICS_PROCESS:
			p_script->_physics_process(p_delta);
			break;
		case EVENT_EXIT_TREE:
			p_script->_exit_tree();
			break;
		default:
			break;
	}
	p_script->owner = nullptr;
	p_script->log_fn = nullptr;
	p_script->call_owner_fn = nullptr;
}
} // namespace detail

} // namespace kaizen

// Exports the entry points the engine calls. Put it once per file, after the class.
#define KAIZEN_CPP_SCRIPT(ScriptClass)                                                                        \
	KAIZEN_CPP_EXPORT int kaizen_cpp_api_version() { return KAIZEN_CPP_API_VERSION; }                         \
	KAIZEN_CPP_EXPORT kaizen::Script *kaizen_cpp_create() { return new ScriptClass(); }                       \
	KAIZEN_CPP_EXPORT void kaizen_cpp_destroy(kaizen::Script *p_script) { delete p_script; }                  \
	KAIZEN_CPP_EXPORT void kaizen_cpp_event(kaizen::Script *p_script, int p_event, double p_delta,            \
			void *p_owner, kaizen::LogFn p_log, kaizen::CallOwnerFn p_call_owner) {                            \
		kaizen::detail::dispatch(p_script, p_event, p_delta, p_owner, p_log, p_call_owner);                    \
	}
)KZHDR";

} // namespace CppScriptApi
