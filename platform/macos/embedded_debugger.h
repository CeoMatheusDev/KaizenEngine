//************************************************************************//
//  embedded_debugger.h                                                   //
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
// See the files LICENSE and LICENSE-ADDITIONAL-TERMS.md for details.     //
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

#include "core/templates/hash_map.h"
#include "core/variant/array.h"

class DisplayServerMacOSEmbedded;

/// @brief Singleton class to process embedded debugging message in the child process.
class EmbeddedDebugger {
	inline static EmbeddedDebugger *singleton = nullptr;

	EmbeddedDebugger(DisplayServerMacOSEmbedded *p_ds);

public:
	static void initialize(DisplayServerMacOSEmbedded *p_ds);
	static void deinitialize();

	~EmbeddedDebugger();

#ifdef DEBUG_ENABLED
private:
	DisplayServerMacOSEmbedded *ds;

	/// Message handler function for parse_message.
	typedef Error (EmbeddedDebugger::*ParseMessageFunc)(const Array &p_args);
	static HashMap<String, ParseMessageFunc> parse_message_handlers;
	static void _init_parse_message_handlers();

	Error _msg_window_size(const Array &p_args);
	Error _msg_mouse_set_mode(const Array &p_args);
	Error _msg_event(const Array &p_args);
	Error _msg_win_event(const Array &p_args);
	Error _msg_notification(const Array &p_args);
	Error _msg_ime_update(const Array &p_args);
	Error _msg_ds_state(const Array &p_args);

public:
	static Error parse_message(void *p_user, const String &p_msg, const Array &p_args, bool &r_captured);
#endif
};
