//************************************************************************//
//  collision_interface.h                                                 //
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
// Modified by Matheus A. Santos, Hardion Labs on 2026-11-23.             //
// Modifications and additional code are licensed under the GNU Affero    //
// General Public License v3.0 (AGPL-3.0).                                //
// See the files LICENSE.txt and LICENSE-ADDITIONAL-TERMS.md for details. //
//************************************************************************//

#pragma once

#include "core/math/transform_3d.h"
#include "core/math/vector3.h"
#include "core/object/ref_counted.h"
#include "core/templates/rid.h"

// Collision detection interface.
// Abstracts collision shapes, raycasts, and sweep tests across backends.
class CollisionInterface : public RefCounted {
public:
	enum ShapeType {
		SHAPE_NONE,
		SHAPE_SPHERE,
		SHAPE_BOX,
		SHAPE_CAPSULE,
		SHAPE_CYLINDER,
		SHAPE_CONVEX,
		SHAPE_HEIGHTFIELD,
		SHAPE_MESH,
		SHAPE_MAX,
	};

	struct RaycastResult {
		bool hit = false;
		Vector3 position;
		Vector3 normal;
		RID collider;
		RID shape;
		float fraction = 1.0f;
	};

	struct SweepResult {
		bool hit = false;
		Vector3 position;
		Vector3 normal;
		RID collider;
		RID shape;
		float fraction = 1.0f;
	};

	// Create a collision shape.
	virtual RID create_shape(ShapeType p_type) = 0;

	// Destroy a collision shape.
	virtual void destroy_shape(RID p_shape) = 0;

	// Raycast against the physics world.
	virtual RaycastResult raycast(const Vector3 &p_from, const Vector3 &p_to, int p_collision_mask = 1) = 0;

	// Sweep test against the physics world.
	virtual SweepResult sweep(RID p_shape, const Transform3D &p_from, const Transform3D &p_to, int p_collision_mask = 1) = 0;

protected:
	CollisionInterface() = default;
	virtual ~CollisionInterface() = default;
};
