//************************************************************************//
//  physics_diagnostics.h                                                 //
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

#include "core/object/ref_counted.h"

// Physics diagnostics and profiling interface.
// Provides performance metrics, profiling data, and debugging information.
class PhysicsDiagnostics : public RefCounted {
public:
	struct FrameStats {
		float simulation_time = 0.0f;     // Time spent in simulation (ms)
		float collision_time = 0.0f;      // Time spent in collision detection (ms)
		float broadphase_time = 0.0f;     // Time spent in broadphase (ms)
		float narrowphase_time = 0.0f;    // Time spent in narrowphase (ms)
		float solver_time = 0.0f;         // Time spent in constraint solver (ms)
		float sync_time = 0.0f;           // Time spent syncing with render (ms)
		uint32_t active_bodies = 0;       // Number of active bodies
		uint32_t static_bodies = 0;       // Number of static bodies
		uint32_t kinematic_bodies = 0;    // Number of kinematic bodies
		uint32_t collisions = 0;          // Number of collisions detected
		uint32_t raycasts = 0;            // Number of raycasts performed
	};

	// Get the last frame's statistics.
	virtual FrameStats get_frame_stats() const = 0;

	// Reset statistics counters.
	virtual void reset_stats() = 0;

	// Enable/disable profiling.
	virtual void set_profiling_enabled(bool p_enabled) = 0;

	// Check if profiling is enabled.
	virtual bool is_profiling_enabled() const = 0;

	// Get a debug string with current stats.
	virtual String get_debug_string() const = 0;

protected:
	PhysicsDiagnostics() = default;
	virtual ~PhysicsDiagnostics() = default;
};
