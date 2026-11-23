//************************************************************************//
//  simulation_interface.h                                                //
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

// Simulation scheduling interface.
// Manages physics simulation stepping and synchronization
// with the engine's execution model.
class SimulationInterface : public RefCounted {
public:
	struct StepInfo {
		float step = 1.0f / 60.0f;
		int max_sub_steps = 4;
		bool async = false;
	};

	// Configure simulation parameters.
	virtual void set_step_info(const StepInfo &p_info) = 0;

	// Get current step info.
	virtual StepInfo get_step_info() const = 0;

	// Step the simulation.
	virtual bool step(float p_delta) = 0;

	// Synchronize simulation state with the rendering server.
	virtual bool sync() = 0;

	// Set the physics scene to simulate.
	virtual void set_scene(void *p_scene) = 0;

protected:
	SimulationInterface() = default;
	virtual ~SimulationInterface() = default;
};
