/**************************************************************************/
/*  cpp_script.cpp                                                        */
/**************************************************************************/
/*  C++ script module for KaizenEngine.                                   */
/**************************************************************************/

#include "cpp_script.h"

#include "cpp_language.h"
#include "cpp_script_api_header.h"

#include "core/config/project_settings.h"
#include "core/io/dir_access.h"
#include "core/io/file_access.h"
#include "core/os/os.h"
#include "core/string/print_string.h"
#include "scene/main/node.h"

namespace {

const char *const CACHE_DIR = "user://cpp_scripts";
constexpr int API_VERSION = 1;

// Event ids; must match kaizen::detail::Event in the API header.
enum EventId {
	EVENT_ENTER_TREE = 0,
	EVENT_READY = 1,
	EVENT_PROCESS = 2,
	EVENT_PHYSICS_PROCESS = 3,
	EVENT_EXIT_TREE = 4,
};

// Lifecycle method names a script instance answers to. The engine enables per-frame processing
// for a node when its script reports `_process`, so these always answer true.
bool is_lifecycle_method(const StringName &p_method) {
	const String name = p_method;
	return name == "_enter_tree" || name == "_ready" || name == "_process" || name == "_physics_process" || name == "_exit_tree";
}

void host_log(const char *p_message) {
	print_line(String("[C++] ") + String::utf8(p_message));
}

int host_call_owner(void *p_owner, const char *p_method) {
	Object *owner = static_cast<Object *>(p_owner);
	if (!owner) {
		return 0;
	}
	const StringName method(p_method);
	if (!owner->has_method(method)) {
		return 0;
	}
	Callable::CallError error;
	owner->callp(method, nullptr, 0, error);
	return error.error == Callable::CallError::CALL_OK ? 1 : 0;
}

const char *dylib_extension() {
#if defined(WINDOWS_ENABLED)
	return ".dll";
#elif defined(MACOS_ENABLED) || defined(IOS_ENABLED)
	return ".dylib";
#else
	return ".so";
#endif
}

// KAIZEN_CPP_COMPILER wins, then CXX, then the first of clang++, g++ or c++ that answers --version.
String find_compiler() {
	const String override_path = OS::get_singleton()->get_environment("KAIZEN_CPP_COMPILER");
	if (!override_path.is_empty()) {
		return override_path;
	}
	List<String> candidates;
	const String cxx = OS::get_singleton()->get_environment("CXX");
	if (!cxx.is_empty()) {
		candidates.push_back(cxx);
	}
	candidates.push_back("clang++");
	candidates.push_back("g++");
	candidates.push_back("c++");
	for (const String &candidate : candidates) {
		List<String> probe;
		probe.push_back("--version");
		String output;
		int exit_code = -1;
		if (OS::get_singleton()->execute(candidate, probe, &output, &exit_code, true) == OK && exit_code == 0) {
			return candidate;
		}
	}
	return String();
}

bool load_symbol(void *p_handle, const char *p_name, void *&r_symbol) {
	r_symbol = nullptr;
	return OS::get_singleton()->get_dynamic_library_symbol_handle(p_handle, String(p_name), r_symbol, true) == OK && r_symbol != nullptr;
}

void unload_library(CppLibrary &p_library) {
	if (!p_library.is_loaded()) {
		return;
	}
	OS::get_singleton()->close_dynamic_library(p_library.handle);
	// Every compile writes a new file name, so removing the old one is safe on POSIX. On Windows the
	// file may stay locked until exit, which is harmless.
	DirAccess::remove_absolute(p_library.file);
	p_library = CppLibrary();
}

// Writes the API header and the source, compiles them to a new shared library and loads it.
// On failure nothing is changed and `r_error` holds the compiler output.
Error compile_and_load(const String &p_source, const String &p_display_path, CppLibrary &r_library, String &r_error) {
	const String cache = ProjectSettings::get_singleton()->globalize_path(CACHE_DIR);
	const String include_dir = cache.path_join("include");
	const String src_dir = cache.path_join("src");
	const String bin_dir = cache.path_join("bin");
	for (const String &dir : { include_dir, src_dir, bin_dir }) {
		if (!DirAccess::dir_exists_absolute(dir) && DirAccess::make_dir_recursive_absolute(dir) != OK) {
			r_error = "Could not create the C++ script cache directory " + dir;
			return ERR_CANT_CREATE;
		}
	}

	const String header_path = include_dir.path_join("kaizen_cpp.h");
	{
		Ref<FileAccess> header = FileAccess::open(header_path, FileAccess::WRITE);
		if (header.is_null()) {
			r_error = "Could not write " + header_path;
			return ERR_CANT_CREATE;
		}
		header->store_string(String::utf8(CppScriptApi::HEADER));
	}

	const String display = p_display_path.is_empty() ? String("script.cpp") : p_display_path;
	const String id = display.md5_text();
	const String src_path = src_dir.path_join(id + ".cpp");
	{
		// `#line` makes compiler diagnostics point at the project file, not at the cache copy.
		const String generated = "#line 1 \"" + display.replace("\\", "/").replace("\"", "'") + "\"\n" + p_source;
		Ref<FileAccess> src = FileAccess::open(src_path, FileAccess::WRITE);
		if (src.is_null()) {
			r_error = "Could not write " + src_path;
			return ERR_CANT_CREATE;
		}
		src->store_string(generated);
	}

	const String compiler = find_compiler();
	if (compiler.is_empty()) {
		r_error = "No C++ compiler found for " + display + ". Install clang++ or g++, or set KAIZEN_CPP_COMPILER to the compiler path.";
		return ERR_FILE_NOT_FOUND;
	}

	// A new file name per compile: the dynamic loader caches libraries by path.
	static int build_counter = 0;
	build_counter++;
	const String out_path = bin_dir.path_join(id + "_" + itos(build_counter) + dylib_extension());

	List<String> args;
	args.push_back("-std=c++17");
	args.push_back("-shared");
#if !defined(WINDOWS_ENABLED)
	args.push_back("-fPIC");
#endif
	args.push_back("-O1");
	args.push_back("-I" + include_dir);
	args.push_back("-o");
	args.push_back(out_path);
	args.push_back(src_path);

	String output;
	int exit_code = -1;
	const Error exec_err = OS::get_singleton()->execute(compiler, args, &output, &exit_code, true);
	if (exec_err != OK || exit_code != 0) {
		r_error = "C++ compilation failed for " + display + ":\n" + output;
		return ERR_COMPILATION_FAILED;
	}

	CppLibrary library;
	library.file = out_path;
	if (OS::get_singleton()->open_dynamic_library(out_path, library.handle) != OK || !library.is_loaded()) {
		r_error = "Could not load the compiled library for " + display + ": " + out_path;
		DirAccess::remove_absolute(out_path);
		return ERR_CANT_OPEN;
	}

	void *symbol = nullptr;
	bool ok = load_symbol(library.handle, "kaizen_cpp_api_version", symbol);
	if (ok) {
		ok = reinterpret_cast<int (*)()>(symbol)() == API_VERSION;
		if (!ok) {
			r_error = "The script " + display + " was built against a different kaizen_cpp.h API version.";
		}
	} else {
		r_error = "The script " + display + " does not call KAIZEN_CPP_SCRIPT(YourClass).";
	}
	if (ok) {
		ok = load_symbol(library.handle, "kaizen_cpp_create", symbol);
		library.create = reinterpret_cast<CppCreateFn>(symbol);
	}
	if (ok) {
		ok = load_symbol(library.handle, "kaizen_cpp_destroy", symbol);
		library.destroy = reinterpret_cast<CppDestroyFn>(symbol);
	}
	if (ok) {
		ok = load_symbol(library.handle, "kaizen_cpp_event", symbol);
		library.event = reinterpret_cast<CppEventFn>(symbol);
	}
	if (!ok) {
		if (r_error.is_empty()) {
			r_error = "The compiled library for " + display + " is missing an entry point.";
		}
		unload_library(library);
		return ERR_CANT_RESOLVE;
	}

	r_library = library;
	return OK;
}

} // namespace

// ---------------------------------------------------------------------------------------
// CppScript
// ---------------------------------------------------------------------------------------

void CppScript::_bind_methods() {
}

CppScript::CppScript() {
	if (CppLanguage *language = CppLanguage::get_singleton()) {
		language->register_script(this);
	}
}

CppScript::~CppScript() {
	unload_library(library);
	if (CppLanguage *language = CppLanguage::get_singleton()) {
		language->unregister_script(this);
	}
}

String CppScript::_display_path() const {
	return path.is_empty() ? get_path() : path;
}

bool CppScript::can_instantiate() const {
	return valid;
}

Ref<Script> CppScript::get_base_script() const {
	return Ref<Script>();
}

StringName CppScript::get_global_name() const {
	return StringName();
}

bool CppScript::inherits_script(const Ref<Script> &p_script) const {
	return false;
}

StringName CppScript::get_instance_base_type() const {
	return StringName("Object");
}

ScriptInstance *CppScript::instance_create(Object *p_this) {
	CppScriptInstance *instance = memnew(CppScriptInstance(Ref<CppScript>(this), p_this));
	instance->_create_state();
	return instance;
}

bool CppScript::has_source_code() const {
	return true;
}

String CppScript::get_source_code() const {
	return source;
}

void CppScript::set_source_code(const String &p_code) {
	source = p_code;
}

Error CppScript::reload(bool p_keep_state) {
	// Membership of `p_keep_state` is not honoured: compiled C++ objects cannot be copied across
	// libraries, so every instance gets a fresh state from the new library.
	CppLibrary fresh;
	String error;
	if (compile_and_load(source, _display_path(), fresh, error) != OK) {
		CppLanguage::get_singleton()->report_error(error);
		// Keep the last library that compiled, so running objects keep working until the error is fixed.
		valid = library.is_loaded();
		return ERR_COMPILATION_FAILED;
	}

	for (CppScriptInstance *instance : instances) {
		instance->_destroy_state();
	}
	unload_library(library);
	library = fresh;
	valid = true;
	for (CppScriptInstance *instance : instances) {
		instance->_create_state();
	}
	return OK;
}

StringName CppScript::get_doc_class_name() const {
	return StringName();
}

Vector<DocData::ClassDoc> CppScript::get_documentation() const {
	return Vector<DocData::ClassDoc>();
}

String CppScript::get_class_icon_path() const {
	return String();
}

bool CppScript::has_method(const StringName &p_method) const {
	return is_lifecycle_method(p_method);
}

MethodInfo CppScript::get_method_info(const StringName &p_method) const {
	if (is_lifecycle_method(p_method)) {
		MethodInfo info;
		info.name = p_method;
		return info;
	}
	return MethodInfo();
}

bool CppScript::is_tool() const {
	return false;
}

bool CppScript::is_script_valid() const {
	return valid;
}

bool CppScript::is_abstract() const {
	return false;
}

ScriptLanguage *CppScript::get_language() const {
	return CppLanguage::get_singleton();
}

bool CppScript::has_script_signal(const StringName &p_signal) const {
	return false;
}

void CppScript::get_script_signal_list(List<MethodInfo> *r_signals) const {
}

bool CppScript::get_property_default_value(const StringName &p_property, Variant &r_value) const {
	return false;
}

void CppScript::get_script_method_list(List<MethodInfo> *r_list) const {
	for (const char *method_name : { "_enter_tree", "_ready", "_process", "_physics_process", "_exit_tree" }) {
		MethodInfo info;
		info.name = method_name;
		r_list->push_back(info);
	}
}

void CppScript::get_script_property_list(List<PropertyInfo> *r_list) const {
}

const Variant CppScript::get_rpc_config() const {
	return Dictionary();
}

// ---------------------------------------------------------------------------------------
// CppScriptInstance
// ---------------------------------------------------------------------------------------

CppScriptInstance::CppScriptInstance(const Ref<CppScript> &p_script, Object *p_owner) :
		script(p_script),
		owner(p_owner) {
	script->instances.insert(this);
}

CppScriptInstance::~CppScriptInstance() {
	_destroy_state();
	script->instances.erase(this);
}

void CppScriptInstance::_create_state() {
	if (state || !script->library.is_loaded() || !script->library.create) {
		return;
	}
	state = script->library.create();
	if (!state) {
		CppLanguage::get_singleton()->report_error("The script " + script->_display_path() + " could not create its object.");
		return;
	}
	// After a reload, bring the new state up to the point the node had reached.
	if (entered) {
		_dispatch(EVENT_ENTER_TREE, 0.0);
	}
	if (readied) {
		_dispatch(EVENT_READY, 0.0);
	}
}

void CppScriptInstance::_destroy_state() {
	if (state && script->library.destroy) {
		script->library.destroy(state);
	}
	state = nullptr;
}

void CppScriptInstance::_dispatch(int p_event, double p_delta) {
	if (!state || !script->library.event) {
		return;
	}
	script->library.event(state, p_event, p_delta, static_cast<void *>(owner), host_log, host_call_owner);
}

bool CppScriptInstance::set(const StringName &p_name, const Variant &p_value) {
	return false;
}

bool CppScriptInstance::get(const StringName &p_name, Variant &r_ret) const {
	return false;
}

void CppScriptInstance::get_property_list(List<PropertyInfo> *r_properties) const {
}

Variant::Type CppScriptInstance::get_property_type(const StringName &p_name, bool *r_is_valid) const {
	if (r_is_valid) {
		*r_is_valid = false;
	}
	return Variant::NIL;
}

void CppScriptInstance::validate_property(PropertyInfo &p_property) const {
}

bool CppScriptInstance::property_can_revert(const StringName &p_name) const {
	return false;
}

bool CppScriptInstance::property_get_revert(const StringName &p_name, Variant &r_ret) const {
	return false;
}

void CppScriptInstance::get_method_list(List<MethodInfo> *r_list) const {
	script->get_script_method_list(r_list);
}

bool CppScriptInstance::has_method(const StringName &p_method) const {
	return is_lifecycle_method(p_method);
}

Variant CppScriptInstance::callp(const StringName &p_method, const Variant **p_args, int p_argcount, Callable::CallError &r_error) {
	// Scripts expose no callable methods yet; only the lifecycle events run.
	r_error.error = Callable::CallError::CALL_ERROR_INVALID_METHOD;
	return Variant();
}

void CppScriptInstance::notification(int p_notification, bool p_reversed) {
	switch (p_notification) {
		case Node::NOTIFICATION_ENTER_TREE:
			entered = true;
			_dispatch(EVENT_ENTER_TREE, 0.0);
			break;
		case Node::NOTIFICATION_READY:
			readied = true;
			_dispatch(EVENT_READY, 0.0);
			break;
		case Node::NOTIFICATION_EXIT_TREE:
			entered = false;
			_dispatch(EVENT_EXIT_TREE, 0.0);
			break;
		case Node::NOTIFICATION_PROCESS:
		case Node::NOTIFICATION_PHYSICS_PROCESS: {
			const Node *node = Object::cast_to<Node>(owner);
			if (!node) {
				return;
			}
			const bool physics = p_notification == Node::NOTIFICATION_PHYSICS_PROCESS;
			const double delta = physics ? node->get_physics_process_delta_time() : node->get_process_delta_time();
			_dispatch(physics ? EVENT_PHYSICS_PROCESS : EVENT_PROCESS, delta);
			break;
		}
		default:
			break;
	}
}

Script *CppScriptInstance::get_script() const {
	return script.ptr();
}

ScriptLanguage *CppScriptInstance::get_language() {
	return CppLanguage::get_singleton();
}
