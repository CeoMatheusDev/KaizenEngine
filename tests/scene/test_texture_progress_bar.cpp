//************************************************************************//
//  test_texture_progress_bar.cpp                                         //
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

TEST_FORCE_LINK(test_texture_progress_bar)

#include "scene/gui/texture_progress_bar.h"

namespace TestTextureProgressBar {

TEST_CASE("[SceneTree][TextureProgressBar]") {
	TextureProgressBar *texture_progress_bar = memnew(TextureProgressBar);

	SUBCASE("[TextureProgressBar] set_radial_initial_angle() should wrap angle between 0 and 360 degrees (inclusive).") {
		texture_progress_bar->set_radial_initial_angle(0.0);
		CHECK(Math::is_equal_approx(texture_progress_bar->get_radial_initial_angle(), (float)0.0));

		texture_progress_bar->set_radial_initial_angle(360.0);
		CHECK(Math::is_equal_approx(texture_progress_bar->get_radial_initial_angle(), (float)360.0));

		texture_progress_bar->set_radial_initial_angle(30.5);
		CHECK(Math::is_equal_approx(texture_progress_bar->get_radial_initial_angle(), (float)30.5));

		texture_progress_bar->set_radial_initial_angle(-30.5);
		CHECK(Math::is_equal_approx(texture_progress_bar->get_radial_initial_angle(), (float)(360 - 30.5)));

		texture_progress_bar->set_radial_initial_angle(36000 + 30.5);
		CHECK(Math::is_equal_approx(texture_progress_bar->get_radial_initial_angle(), (float)30.5));

		texture_progress_bar->set_radial_initial_angle(-(36000 + 30.5));
		CHECK(Math::is_equal_approx(texture_progress_bar->get_radial_initial_angle(), (float)(360 - 30.5)));
	}

	SUBCASE("[TextureProgressBar] set_radial_initial_angle() should not set non-finite values.") {
		texture_progress_bar->set_radial_initial_angle(30.5);

		ERR_PRINT_OFF;
		texture_progress_bar->set_radial_initial_angle(Math::INF);
		ERR_PRINT_ON;
		CHECK(Math::is_equal_approx(texture_progress_bar->get_radial_initial_angle(), (float)30.5));

		ERR_PRINT_OFF;
		texture_progress_bar->set_radial_initial_angle(-Math::INF);
		ERR_PRINT_ON;
		CHECK(Math::is_equal_approx(texture_progress_bar->get_radial_initial_angle(), (float)30.5));

		ERR_PRINT_OFF;
		texture_progress_bar->set_radial_initial_angle(Math::NaN);
		ERR_PRINT_ON;
		CHECK(Math::is_equal_approx(texture_progress_bar->get_radial_initial_angle(), (float)30.5));

		ERR_PRINT_OFF;
		texture_progress_bar->set_radial_initial_angle(-Math::NaN);
		ERR_PRINT_ON;
		CHECK(Math::is_equal_approx(texture_progress_bar->get_radial_initial_angle(), (float)30.5));
	}

	memdelete(texture_progress_bar);
}

} // namespace TestTextureProgressBar
