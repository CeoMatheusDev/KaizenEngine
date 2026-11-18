//************************************************************************//
//  test_physics_material.cpp                                             //
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

TEST_FORCE_LINK(test_physics_material)

#ifndef PHYSICS_3D_DISABLED

#include "scene/resources/physics_material.h"

namespace TestPhysicsMaterial {

TEST_CASE("[Physics_material] Defaults") {
	Ref<PhysicsMaterial> physics_material;
	physics_material.instantiate();

	CHECK(physics_material->get_friction() == 1.);
	CHECK(physics_material->is_rough() == false);
	CHECK(physics_material->get_bounce() == 0.);
	CHECK(physics_material->is_absorbent() == false);
}

TEST_CASE("[Physics_material] Friction") {
	Ref<PhysicsMaterial> physics_material;
	physics_material.instantiate();

	real_t friction = 0.314;
	physics_material->set_friction(friction);
	CHECK(physics_material->get_friction() == friction);
}

TEST_CASE("[Physics_material] Rough") {
	Ref<PhysicsMaterial> physics_material;
	physics_material.instantiate();

	bool rough = true;
	physics_material->set_rough(rough);
	CHECK(physics_material->is_rough() == rough);

	real_t friction = 0.314;
	physics_material->set_friction(friction);
	CHECK(physics_material->computed_friction() == -friction);

	rough = false;
	physics_material->set_rough(rough);
	CHECK(physics_material->is_rough() == rough);

	CHECK(physics_material->computed_friction() == friction);
}

TEST_CASE("[Physics_material] Bounce") {
	Ref<PhysicsMaterial> physics_material;
	physics_material.instantiate();

	real_t bounce = 0.271;
	physics_material->set_bounce(bounce);
	CHECK(physics_material->get_bounce() == bounce);
}

TEST_CASE("[Physics_material] Absorbent") {
	Ref<PhysicsMaterial> physics_material;
	physics_material.instantiate();

	bool absorbent = true;
	physics_material->set_absorbent(absorbent);
	CHECK(physics_material->is_absorbent() == absorbent);

	real_t bounce = 0.271;
	physics_material->set_bounce(bounce);
	CHECK(physics_material->computed_bounce() == -bounce);

	absorbent = false;
	physics_material->set_absorbent(absorbent);
	CHECK(physics_material->is_absorbent() == absorbent);

	CHECK(physics_material->computed_bounce() == bounce);
}

} // namespace TestPhysicsMaterial

#endif // PHYSICS_3D_DISABLED
