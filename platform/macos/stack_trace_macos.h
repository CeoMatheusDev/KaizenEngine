//************************************************************************//
//  stack_trace_macos.h                                                   //
//************************************************************************//
//                         This file is part of:                          //
//                            KaizenEngine                                //
//              https://github.com/CeoMatheusDev/Kaizen                   //
//************************************************************************//
// Copyright (c) 2007-2014 Juan Linietsky, Ariel Manzur.                  //
// Copyright (c) 2014-present Godot Engine contributors.                  //
// Copyright (c) 2026 Matheus A. Santos, Hardion Labs.                    //
//                                                                        //
// This file contains code derived from the Godot Engine                  //
// (https://godotengine.org), which is licensed under the MIT License.    //
// The original MIT notice is preserved below.                            //
//                                                                        //
// Modified by Matheus A. Santos, Hardion Labs on 2026-11-17.             //
// Modifications and additional code are licensed under the GNU Affero    //
// General Public License v3.0 (AGPL-3.0).                                //
// See the files LICENSE.txt and LICENSE-ADDITIONAL-TERMS.md for details. //
//                                                                        //
// Permission is hereby granted, free of charge, to any person obtaining  //
// a copy of this software and associated documentation files (the        //
// "Software"), to deal in the Software without restriction, including    //
// without limitation the rights to use, copy, modify, merge, publish,    //
// distribute, sublicense, and/or sell copies of the Software, and to     //
// permit persons to whom the Software is furnished to do so, subject to  //
// the following conditions:                                              //
//                                                                        //
// The above copyright notice and this permission notice shall be         //
// included in all copies or substantial portions of the Software.        //
//                                                                        //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        //
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     //
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. //
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   //
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   //
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE      //
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 //
//************************************************************************//

#pragma once

#include "core/os/os.h"

#include <dlfcn.h>
#include <mach-o/dyld.h>

namespace StackTraceMacOS {

static void *find_executable_load_address() {
	char full_path[1024];
	uint32_t size = sizeof(full_path);
	if (!_NSGetExecutablePath(full_path, &size)) {
		void *handle = dlopen(full_path, RTLD_LAZY | RTLD_NOLOAD);
		void *addr = dlsym(handle, "main");
		Dl_info info;
		if (dladdr(addr, &info)) {
			return info.dli_fbase;
		}
	}
	return nullptr;
}

static Vector<String> symbolize_with_atos(const String &p_exec_path, const void *p_load_addr, const void *const *p_backtrace_addresses, size_t p_backtrace_size) {
	List<String> args;
	args.push_back("-o");
	args.push_back(p_exec_path);
#if defined(__x86_64) || defined(__x86_64__) || defined(__amd64__)
	args.push_back("-arch");
	args.push_back("x86_64");
#elif defined(__aarch64__) || defined(__arm64__)
	args.push_back("-arch");
	args.push_back("arm64");
#endif
	args.push_back("--fullPath");
	args.push_back("-l");
	// Add load address first, then the backtrace addresses.
	char str[1024];
	snprintf(str, sizeof(str), "%p", p_load_addr);
	args.push_back(String(str));
	for (size_t i = 0; i < p_backtrace_size; i++) {
		snprintf(str, sizeof(str), "%p", p_backtrace_addresses[i]);
		args.push_back(String(str));
	}
	// Execute atos with the arguments and capture the output.
	String atos_output;
	int ret = 0;
	const Error err = OS::get_singleton()->execute("atos", args, &atos_output, &ret);
	if (err == OK && ret == 0 && !atos_output.is_empty()) {
		return atos_output.split("\n", false);
	}
	return Vector<String>();
}

} // namespace StackTraceMacOS
