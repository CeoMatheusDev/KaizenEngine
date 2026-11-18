//************************************************************************//
//  test_parallax_2d.cpp                                                  //
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

#include "tests/test_macros.h"

TEST_FORCE_LINK(test_parallax_2d)

#include "scene/2d/parallax_2d.h"

// Test cases for the Parallax2D class to ensure its properties are set and retrieved correctly.

namespace TestParallax2D {

TEST_CASE("[SceneTree][Parallax2D] Scroll Scale") {
	// Test setting and getting the scroll scale.
	Parallax2D *parallax = memnew(Parallax2D);
	Size2 scale(2, 2);
	parallax->set_scroll_scale(scale);
	CHECK(parallax->get_scroll_scale() == scale);
	memdelete(parallax);
}

TEST_CASE("[SceneTree][Parallax2D] Repeat Size") {
	// Test setting and getting the repeat size.
	Parallax2D *parallax = memnew(Parallax2D);
	Size2 size(100, 100);
	parallax->set_repeat_size(size);
	CHECK(parallax->get_repeat_size() == size);
	memdelete(parallax);
}

TEST_CASE("[SceneTree][Parallax2D] Repeat Times") {
	// Test setting and getting the repeat times.
	Parallax2D *parallax = memnew(Parallax2D);
	int times = 5;
	parallax->set_repeat_times(times);
	CHECK(parallax->get_repeat_times() == times);
	memdelete(parallax);
}

TEST_CASE("[SceneTree][Parallax2D] Autoscroll") {
	// Test setting and getting the autoscroll values.
	Parallax2D *parallax = memnew(Parallax2D);
	Point2 autoscroll(1, 1);
	parallax->set_autoscroll(autoscroll);
	CHECK(parallax->get_autoscroll() == autoscroll);
	memdelete(parallax);
}

TEST_CASE("[SceneTree][Parallax2D] Scroll Offset") {
	// Test setting and getting the scroll offset.
	Parallax2D *parallax = memnew(Parallax2D);
	Point2 offset(10, 10);
	parallax->set_scroll_offset(offset);
	CHECK(parallax->get_scroll_offset() == offset);
	memdelete(parallax);
}

TEST_CASE("[SceneTree][Parallax2D] Screen Offset") {
	// Test setting and getting the screen offset.
	Parallax2D *parallax = memnew(Parallax2D);
	Point2 offset(20, 20);
	parallax->set_screen_offset(offset);
	CHECK(parallax->get_screen_offset() == offset);
	memdelete(parallax);
}

TEST_CASE("[SceneTree][Parallax2D] Limit Begin") {
	// Test setting and getting the limit begin values.
	Parallax2D *parallax = memnew(Parallax2D);
	Point2 limit_begin(-100, -100);
	parallax->set_limit_begin(limit_begin);
	CHECK(parallax->get_limit_begin() == limit_begin);
	memdelete(parallax);
}

TEST_CASE("[SceneTree][Parallax2D] Limit End") {
	// Test setting and getting the limit end values.
	Parallax2D *parallax = memnew(Parallax2D);
	Point2 limit_end(100, 100);
	parallax->set_limit_end(limit_end);
	CHECK(parallax->get_limit_end() == limit_end);
	memdelete(parallax);
}

TEST_CASE("[SceneTree][Parallax2D] Follow Viewport") {
	// Test setting and getting the follow viewport flag.
	Parallax2D *parallax = memnew(Parallax2D);
	parallax->set_follow_viewport(false);
	CHECK_FALSE(parallax->get_follow_viewport());
	memdelete(parallax);
}

TEST_CASE("[SceneTree][Parallax2D] Ignore Camera Scroll") {
	// Test setting and getting the ignore camera scroll flag.
	Parallax2D *parallax = memnew(Parallax2D);
	parallax->set_ignore_camera_scroll(true);
	CHECK(parallax->is_ignore_camera_scroll());
	memdelete(parallax);
}

} // namespace TestParallax2D
