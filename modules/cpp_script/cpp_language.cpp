/**************************************************************************/
/*  cpp_language.cpp                                                      */
/**************************************************************************/
/*  C++ script module for KaizenEngine.                                   */
/**************************************************************************/

#include "cpp_language.h"

#include "cpp_script.h"

namespace {

const char *const CPP_KEYWORDS[] = {
	"alignas", "alignof", "and", "and_eq", "asm", "auto", "bitand", "bitor", "bool", "break",
	"case", "catch", "char", "char8_t", "char16_t", "char32_t", "class", "compl", "concept", "const",
	"consteval", "constexpr", "constinit", "const_cast", "continue", "co_await", "co_return", "co_yield", "decltype", "default",
	"delete", "do", "double", "dynamic_cast", "else", "enum", "explicit", "export", "extern", "false",
	"final", "float", "for", "friend", "goto", "if", "inline", "int", "long", "mutable",
	"namespace", "new", "noexcept", "not", "not_eq", "nullptr", "operator", "or", "or_eq", "override",
	"private", "protected", "public", "register", "reinterpret_cast", "requires", "return", "short", "signed", "sizeof",
	"static", "static_assert", "static_cast", "struct", "switch", "template", "this", "thread_local", "throw", "true",
	"try", "typedef", "typeid", "typename", "union", "unsigned", "using", "virtual", "void", "volatile",
	"wchar_t", "while", "xor", "xor_eq",
};

const char *const CONTROL_FLOW_KEYWORDS[] = {
	"if", "else", "for", "while", "do", "switch", "case", "default", "break", "continue",
	"return", "goto", "try", "catch", "throw", "co_return", "co_await", "co_yield",
};

// Source for a new script. The engine's API header is found through the include path the
// compiler is given, so the template needs only the one include.
String default_source(const String &p_class_name) {
	const String name = p_class_name.is_empty() ? String("KaizenScript") : p_class_name;
	return "#include \"kaizen_cpp.h\"\n\n"
		   "class " +
			name + " : public kaizen::Script {\n"
				   "public:\n"
				   "\tvoid _ready() override {\n"
				   "\t}\n\n"
				   "\tvoid _process(double p_delta) override {\n"
				   "\t}\n"
				   "};\n\n"
				   "KAIZEN_CPP_SCRIPT(" +
			name + ")\n";
}

#ifdef TOOLS_ENABLED
// Names of the functions defined at the start of a line as `void name(`. Good enough for the
// script editor's function list; the compiler is the authority on what is valid.
Vector<String> function_names(const String &p_code) {
	Vector<String> names;
	const PackedStringArray lines = p_code.split("\n");
	for (const String &line : lines) {
		const String trimmed = line.strip_edges();
		if (!trimmed.begins_with("void ")) {
			continue;
		}
		const String rest = trimmed.substr(5);
		const int paren = rest.find("(");
		if (paren <= 0) {
			continue;
		}
		const String name = rest.left(paren).strip_edges();
		if (!name.is_empty() && name.is_valid_identifier()) {
			names.push_back(name);
		}
	}
	return names;
}
#endif // TOOLS_ENABLED

} // namespace

CppLanguage *CppLanguage::singleton = nullptr;

#ifdef TOOLS_ENABLED
bool CppEditorLanguage::validate(const String &p_code, const String &p_path, List<ScriptError> *r_errors, List<Warning> *r_warnings, List<String> *r_functions, HashSet<int> *r_safe_lines) const {
	if (r_functions) {
		for (const String &name : function_names(p_code)) {
			r_functions->push_back(name);
		}
	}
	return true;
}

int32_t CppEditorLanguage::find_function(const String &p_function, const String &p_code) const {
	const PackedStringArray lines = p_code.split("\n");
	for (int i = 0; i < lines.size(); i++) {
		const String trimmed = lines[i].strip_edges();
		if (trimmed.begins_with("void " + p_function + "(") || trimmed.begins_with("void " + p_function + " (")) {
			return i;
		}
	}
	return -1;
}
#endif // TOOLS_ENABLED

CppLanguage *CppLanguage::get_singleton() {
	return singleton;
}

CppLanguage::CppLanguage() {
	singleton = this;
}

CppLanguage::~CppLanguage() {
	finish();
	if (singleton == this) {
		singleton = nullptr;
	}
}

void CppLanguage::report_error(const String &p_message) {
	last_error = p_message;
	ERR_PRINT(p_message);
}

void CppLanguage::register_script(CppScript *p_script) {
	scripts.insert(p_script);
}

void CppLanguage::unregister_script(CppScript *p_script) {
	scripts.erase(p_script);
}

String CppLanguage::get_name() const {
	return "C++";
}

void CppLanguage::init() {
}

String CppLanguage::get_type() const {
	return "CppScript";
}

String CppLanguage::get_extension() const {
	return "cpp";
}

void CppLanguage::finish() {
	scripts.clear();
}

#ifdef TOOLS_ENABLED
EditorLanguage *CppLanguage::get_editor_language() {
	return &editor_language;
}
#endif // TOOLS_ENABLED

Vector<String> CppLanguage::get_reserved_words() const {
	Vector<String> words;
	for (const char *keyword : CPP_KEYWORDS) {
		words.push_back(keyword);
	}
	return words;
}

bool CppLanguage::is_control_flow_keyword(const String &p_string) const {
	for (const char *keyword : CONTROL_FLOW_KEYWORDS) {
		if (p_string == keyword) {
			return true;
		}
	}
	return false;
}

Vector<String> CppLanguage::get_comment_delimiters() const {
	Vector<String> delimiters;
	delimiters.push_back("//");
	delimiters.push_back("/* */");
	return delimiters;
}

Vector<String> CppLanguage::get_doc_comment_delimiters() const {
	Vector<String> delimiters;
	delimiters.push_back("///");
	return delimiters;
}

Vector<String> CppLanguage::get_string_delimiters() const {
	Vector<String> delimiters;
	delimiters.push_back("\" \"");
	delimiters.push_back("' '");
	delimiters.push_back("R\"( )\"");
	return delimiters;
}

Ref<Script> CppLanguage::make_template(const String &p_template, const String &p_class_name, const String &p_base_class_name) const {
	Ref<CppScript> script;
	script.instantiate();
	script->set_source_code(p_template.is_empty() ? default_source(p_class_name) : p_template);
	return script;
}

Vector<ScriptLanguage::ScriptTemplate> CppLanguage::get_built_in_templates(const StringName &p_object) {
	return Vector<ScriptTemplate>();
}

String CppLanguage::make_function(const String &p_class, const String &p_name, const PackedStringArray &p_args) const {
	String args;
	for (int i = 0; i < p_args.size(); i++) {
		if (i > 0) {
			args += ", ";
		}
		args += p_args[i];
	}
	return "void " + p_name + "(" + args + ") {\n}\n";
}

void CppLanguage::add_global_constant(const StringName &p_variable, const Variant &p_value) {
	// C++ scripts have no shared global scope.
}

String CppLanguage::debug_get_error() const {
	return last_error;
}

void CppLanguage::reload_all_scripts() {
	for (CppScript *script : scripts) {
		script->reload(true);
	}
}

void CppLanguage::reload_scripts(const Array &p_scripts) {
	for (int i = 0; i < p_scripts.size(); i++) {
		if (CppScript *script = Object::cast_to<CppScript>(p_scripts[i])) {
			script->reload(true);
		}
	}
}

void CppLanguage::reload_tool_script(const Ref<Script> &p_script) {
	if (CppScript *script = Object::cast_to<CppScript>(p_script.ptr())) {
		script->reload(true);
	}
}

bool CppLanguage::handles_global_class_type(const String &p_type) const {
	return false;
}

String CppLanguage::get_global_class_name(const String &p_path, String *r_base_type, String *r_icon_path, bool *r_is_abstract, bool *r_is_tool) const {
	return String();
}
