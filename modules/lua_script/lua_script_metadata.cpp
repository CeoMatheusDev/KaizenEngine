/**************************************************************************/
/*  lua_script_metadata.cpp                                               */
/**************************************************************************/
/*  Lua script module for KaizenEngine.                                   */
/**************************************************************************/

#include "lua_script_metadata.h"

namespace LuaScriptMetadata {

namespace {

bool is_space(char c) {
	return c == ' ' || c == '\t' || c == '\r';
}

bool is_ident_start(char c) {
	return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
}

bool is_ident_char(char c) {
	return is_ident_start(c) || (c >= '0' && c <= '9');
}

std::string trim(const std::string &p_text) {
	size_t begin = 0;
	size_t end = p_text.size();
	while (begin < end && is_space(p_text[begin])) {
		begin++;
	}
	while (end > begin && is_space(p_text[end - 1])) {
		end--;
	}
	return p_text.substr(begin, end - begin);
}

// Returns the identifier starting at `p_pos` (after skipping blanks) and advances `p_pos`.
std::string read_identifier(const std::string &p_text, size_t &p_pos) {
	while (p_pos < p_text.size() && is_space(p_text[p_pos])) {
		p_pos++;
	}
	if (p_pos >= p_text.size() || !is_ident_start(p_text[p_pos])) {
		return std::string();
	}
	size_t start = p_pos;
	while (p_pos < p_text.size() && is_ident_char(p_text[p_pos])) {
		p_pos++;
	}
	return p_text.substr(start, p_pos - start);
}

void skip_spaces(const std::string &p_text, size_t &p_pos) {
	while (p_pos < p_text.size() && is_space(p_text[p_pos])) {
		p_pos++;
	}
}

// Splits "a, b, ..." between the parentheses that start at `p_pos`.
std::vector<std::string> read_parameters(const std::string &p_text, size_t p_pos) {
	std::vector<std::string> params;
	skip_spaces(p_text, p_pos);
	if (p_pos >= p_text.size() || p_text[p_pos] != '(') {
		return params;
	}
	p_pos++;
	std::string current;
	while (p_pos < p_text.size() && p_text[p_pos] != ')') {
		if (p_text[p_pos] == ',') {
			params.push_back(trim(current));
			current.clear();
		} else {
			current += p_text[p_pos];
		}
		p_pos++;
	}
	const std::string last = trim(current);
	if (!last.empty()) {
		params.push_back(last);
	}
	return params;
}

// Recognises `function name(...)` and `name = function(...)` at column zero, which are the
// forms that define a top-level (module-level) function in a script.
bool match_top_level_function(const std::string &p_line, std::string &r_name, std::vector<std::string> &r_params) {
	if (p_line.empty() || is_space(p_line[0])) {
		return false;
	}
	size_t pos = 0;
	if (p_line.compare(0, 9, "function ") == 0 || p_line.compare(0, 9, "function\t") == 0) {
		pos = 8;
		r_name = read_identifier(p_line, pos);
		if (r_name.empty()) {
			return false;
		}
		r_params = read_parameters(p_line, pos);
		return true;
	}
	r_name = read_identifier(p_line, pos);
	if (r_name.empty()) {
		return false;
	}
	skip_spaces(p_line, pos);
	if (pos >= p_line.size() || p_line[pos] != '=') {
		return false;
	}
	pos++;
	skip_spaces(p_line, pos);
	if (p_line.compare(pos, 8, "function") != 0) {
		return false;
	}
	pos += 8;
	r_params = read_parameters(p_line, pos);
	return true;
}

void parse_export(const std::string &p_rest, int p_line, Metadata &r_metadata) {
	Export entry;
	entry.line = p_line;
	size_t pos = 0;
	entry.name = read_identifier(p_rest, pos);
	if (entry.name.empty()) {
		return;
	}
	skip_spaces(p_rest, pos);
	if (pos < p_rest.size() && p_rest[pos] == ':') {
		pos++;
		entry.type = read_identifier(p_rest, pos);
		skip_spaces(p_rest, pos);
	}
	if (pos < p_rest.size() && p_rest[pos] == '=') {
		entry.default_expr = trim(p_rest.substr(pos + 1));
	}
	r_metadata.exports.push_back(entry);
}

void parse_signal(const std::string &p_rest, int p_line, Metadata &r_metadata) {
	Signal entry;
	entry.line = p_line;
	size_t pos = 0;
	entry.name = read_identifier(p_rest, pos);
	if (entry.name.empty()) {
		return;
	}
	entry.params = read_parameters(p_rest, pos);
	r_metadata.signals.push_back(entry);
}

void parse_annotation(const std::string &p_body, int p_line, Metadata &r_metadata) {
	const std::string body = trim(p_body);
	if (body == "tool") {
		r_metadata.tool = true;
		return;
	}
	if (body.compare(0, 8, "extends ") == 0 || body.compare(0, 8, "extends\t") == 0) {
		r_metadata.extends = trim(body.substr(8));
		return;
	}
	if (body.compare(0, 11, "class_name ") == 0 || body.compare(0, 11, "class_name\t") == 0) {
		size_t pos = 10;
		r_metadata.class_name = read_identifier(body, pos);
		return;
	}
	if (body.compare(0, 7, "export ") == 0 || body.compare(0, 7, "export\t") == 0) {
		parse_export(body.substr(6), p_line, r_metadata);
		return;
	}
	if (body.compare(0, 7, "signal ") == 0 || body.compare(0, 7, "signal\t") == 0) {
		parse_signal(body.substr(6), p_line, r_metadata);
	}
}

} // namespace

Metadata parse(const std::string &p_source) {
	Metadata metadata;
	int line_number = 0;
	size_t start = 0;
	while (start <= p_source.size()) {
		size_t end = p_source.find('\n', start);
		if (end == std::string::npos) {
			end = p_source.size();
		}
		const std::string line = p_source.substr(start, end - start);
		line_number++;

		if (line.compare(0, 3, "--@") == 0) {
			parse_annotation(line.substr(3), line_number, metadata);
		} else {
			std::string name;
			std::vector<std::string> params;
			if (match_top_level_function(line, name, params)) {
				Function function;
				function.name = name;
				function.params = params;
				function.line = line_number;
				metadata.functions.push_back(function);
			}
		}

		if (end == p_source.size()) {
			break;
		}
		start = end + 1;
	}
	return metadata;
}

int find_function_line(const std::string &p_source, const std::string &p_name) {
	const Metadata metadata = parse(p_source);
	for (const Function &function : metadata.functions) {
		if (function.name == p_name) {
			return function.line;
		}
	}
	return -1;
}

} // namespace LuaScriptMetadata
