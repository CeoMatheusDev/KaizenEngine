//************************************************************************//
//  debugger_helpers.cpp                                                  //
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

#ifdef DEBUG_ENABLED
#include "debugger_helpers.h"

#include "core/input/input.h"
#include "core/input/shortcut.h"
#include "scene/main/node.h"

bool DebuggerHelpers::is_shortcut_pressed(const int p_idx, const HashMap<int, Ref<Shortcut>> &p_shortcuts, bool p_true_if_empty) {
	ERR_FAIL_COND_V(!p_shortcuts.has(p_idx), p_true_if_empty);
	if (is_shortcut_empty(p_idx, p_shortcuts)) {
		return p_true_if_empty;
	}

	Ref<Shortcut> shortcut = p_shortcuts[p_idx];
	for (Ref<InputEventKey> k : shortcut->get_events()) {
		if (k.is_valid() && (Input::get_singleton()->is_key_pressed(k->get_keycode()) || Input::get_singleton()->is_physical_key_pressed(k->get_physical_keycode()))) {
			return true;
		}
	}

	return false;
}

bool DebuggerHelpers::SelectResult::operator<(const SelectResult &p_rr) const {
	if (order != p_rr.order) {
		return p_rr.order < order;
	}

	if (item && p_rr.item) {
		return item->is_greater_than(p_rr.item);
	}

	if (item || p_rr.item) {
		return item != nullptr;
	}

	return false;
}

bool DebuggerHelpers::is_shortcut_empty(const int p_idx, const HashMap<int, Ref<Shortcut>> &p_shortcuts) {
	ERR_FAIL_COND_V(!p_shortcuts.has(p_idx), true);
	Ref<Shortcut> shortcut = p_shortcuts[p_idx];
	return shortcut.is_null() || shortcut->get_events().is_empty();
}

#endif // DEBUG_ENABLED
