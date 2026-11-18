//************************************************************************//
//  test_input_event_mouse.cpp                                            //
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

#include "tests/test_macros.h"

TEST_FORCE_LINK(test_input_event_mouse)

#include "core/input/input_event.h"

namespace TestInputEventMouse {

TEST_CASE("[InputEventMouse] Mouse button mask is set correctly") {
	InputEventMouse mousekey;

	mousekey.set_button_mask(MouseButtonMask::LEFT);
	CHECK(mousekey.get_button_mask().has_flag(MouseButtonMask::LEFT));

	mousekey.set_button_mask(MouseButtonMask::MB_XBUTTON1);
	CHECK(mousekey.get_button_mask().has_flag(MouseButtonMask::MB_XBUTTON1));

	mousekey.set_button_mask(MouseButtonMask::MB_XBUTTON2);
	CHECK(mousekey.get_button_mask().has_flag(MouseButtonMask::MB_XBUTTON2));

	mousekey.set_button_mask(MouseButtonMask::MIDDLE);
	CHECK(mousekey.get_button_mask().has_flag(MouseButtonMask::MIDDLE));

	mousekey.set_button_mask(MouseButtonMask::RIGHT);
	CHECK(mousekey.get_button_mask().has_flag(MouseButtonMask::RIGHT));
}

TEST_CASE("[InputEventMouse] Setting the mouse position works correctly") {
	InputEventMouse mousekey;

	mousekey.set_position(Vector2{ 10, 10 });
	CHECK(mousekey.get_position() == Vector2{ 10, 10 });

	mousekey.set_position(Vector2{ -1, -1 });
	CHECK(mousekey.get_position() == Vector2{ -1, -1 });
}

TEST_CASE("[InputEventMouse] Setting the global mouse position works correctly") {
	InputEventMouse mousekey;

	mousekey.set_global_position(Vector2{ 10, 10 });
	CHECK(mousekey.get_global_position() == Vector2{ 10, 10 });
	CHECK(mousekey.get_global_position() != Vector2{ 1, 1 });

	mousekey.set_global_position(Vector2{ -1, -1 });
	CHECK(mousekey.get_global_position() == Vector2{ -1, -1 });
	CHECK(mousekey.get_global_position() != Vector2{ 1, 1 });
}

} // namespace TestInputEventMouse
