//************************************************************************//
//  reference_rect.cpp                                                    //
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

#include "reference_rect.h"

#include "core/config/engine.h"
#include "core/object/class_db.h"

void ReferenceRect::_notification(int p_what) {
	switch (p_what) {
		case NOTIFICATION_DRAW: {
			if (!is_inside_tree()) {
				return;
			}
			if (Engine::get_singleton()->is_editor_hint() || !editor_only) {
				draw_rect(Rect2(Point2(), get_size()), border_color, false, border_width);
			}
		} break;
	}
}

void ReferenceRect::set_border_color(const Color &p_color) {
	if (border_color == p_color) {
		return;
	}

	border_color = p_color;
	queue_redraw();
}

Color ReferenceRect::get_border_color() const {
	return border_color;
}

void ReferenceRect::set_border_width(float p_width) {
	float width_max = MAX(0.0, p_width);
	if (border_width == width_max) {
		return;
	}

	border_width = width_max;
	queue_redraw();
}

float ReferenceRect::get_border_width() const {
	return border_width;
}

void ReferenceRect::set_editor_only(const bool &p_enabled) {
	if (editor_only == p_enabled) {
		return;
	}

	editor_only = p_enabled;
	queue_redraw();
}

bool ReferenceRect::get_editor_only() const {
	return editor_only;
}

void ReferenceRect::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_border_color"), &ReferenceRect::get_border_color);
	ClassDB::bind_method(D_METHOD("set_border_color", "color"), &ReferenceRect::set_border_color);

	ClassDB::bind_method(D_METHOD("get_border_width"), &ReferenceRect::get_border_width);
	ClassDB::bind_method(D_METHOD("set_border_width", "width"), &ReferenceRect::set_border_width);

	ClassDB::bind_method(D_METHOD("get_editor_only"), &ReferenceRect::get_editor_only);
	ClassDB::bind_method(D_METHOD("set_editor_only", "enabled"), &ReferenceRect::set_editor_only);

	ADD_PROPERTY(PropertyInfo(Variant::COLOR, "border_color"), "set_border_color", "get_border_color");
	ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "border_width", PROPERTY_HINT_RANGE, "0.0,5.0,0.1,or_greater,suffix:px"), "set_border_width", "get_border_width");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "editor_only"), "set_editor_only", "get_editor_only");
}
