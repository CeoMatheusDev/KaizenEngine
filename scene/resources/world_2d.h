//************************************************************************//
//  world_2d.h                                                            //
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

#include "core/io/resource.h"

#ifndef PHYSICS_2D_DISABLED
class PhysicsDirectSpaceState2D;
#endif // PHYSICS_2D_DISABLED

class VisibleOnScreenNotifier2D;
class Viewport;
struct SpatialIndexer2D;

// World2D is needed for Viewport for CanvasItem rendering even when 2D is disabled.
class World2D : public Resource {
	GDCLASS(World2D, Resource);

	RID canvas;
#ifndef NAVIGATION_2D_DISABLED
	mutable RID navigation_map;
#endif // NAVIGATION_2D_DISABLED
#ifndef PHYSICS_2D_DISABLED
	mutable RID space;
#endif // PHYSICS_2D_DISABLED

	HashSet<Viewport *> viewports;

protected:
	static void _bind_methods();
	friend class Viewport;

public:
	RID get_canvas() const;
#ifndef NAVIGATION_2D_DISABLED
	RID get_navigation_map() const;
#endif // NAVIGATION_2D_DISABLED

#ifndef PHYSICS_2D_DISABLED
	RID get_space() const;
	PhysicsDirectSpaceState2D *get_direct_space_state();
#endif // PHYSICS_2D_DISABLED

	void register_viewport(Viewport *p_viewport);
	void remove_viewport(Viewport *p_viewport);

	_FORCE_INLINE_ const HashSet<Viewport *> &get_viewports() { return viewports; }

	World2D();
	~World2D();
};
