//************************************************************************//
//  physics_backend.h                                                     //
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

#include "core/math/vector3.h"
#include "core/object/ref_counted.h"

// Engine-owned physics backend interface.
// All physics backends (Jolt, PhysX, etc.) implement this interface.
// This keeps physics backend implementations isolated from engine systems.
class PhysicsBackend : public RefCounted {
public:
	enum Type {
		TYPE_NONE,
		TYPE_JOLT,
		TYPE_PHYSX,
		TYPE_MAX,
	};

	// Returns the backend type.
	virtual Type get_type() const = 0;

	// Returns true if the backend is supported on the current platform.
	virtual bool is_supported() const = 0;

	// Initialize the backend.
	virtual bool init() = 0;

	// Shut down the backend.
	virtual void shutdown() = 0;

	// Run a simulation step.
	virtual bool simulate(float p_step) = 0;

	// Fetch simulation results.
	virtual bool fetch_results(bool p_wait = true) = 0;

	// Create a physics scene.
	virtual void *create_scene(const Vector3 &p_gravity) = 0;

	// Destroy a physics scene.
	virtual void destroy_scene(void *p_scene) = 0;

	// Clean up all resources.
	virtual void cleanup() = 0;

protected:
	PhysicsBackend() = default;
	virtual ~PhysicsBackend() = default;
};
