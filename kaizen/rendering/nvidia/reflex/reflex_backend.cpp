//************************************************************************//
//  reflex_backend.cpp                                                    //
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

#include "reflex_backend.h"

#include "core/os/os.h"

using namespace RendererRD;

ReflexBackend::ReflexBackend() {
	config.enabled = false;
	config.low_latency_mode = false;
	config.frame_warp = false;
	config.latency_stats = false;
	config.analyzer = false;
	supported = false;
}

ReflexBackend::~ReflexBackend() {
	shutdown();
}

bool ReflexBackend::init() {
	RenderingDevice *rd = RD::get_singleton();
	if (!rd) {
		return false;
	}

	String vendor = rd->get_device_vendor_name();
	if (vendor != "NVIDIA") {
		return false;
	}

#ifdef REFLEX_ENABLED
	// Reflex is dispatched through the Streamline interposer.
	Error err = OS::get_singleton()->open_dynamic_library("sl.interposer", reflex_handle);
	if (err != OK || !reflex_handle) {
		WARN_PRINT("ReflexBackend: Failed to load the Streamline interposer.");
		return false;
	}

	supported = true;
	return true;
#else
	return false;
#endif
}

void ReflexBackend::shutdown() {
#ifdef REFLEX_ENABLED
	if (reflex_handle) {
		OS::get_singleton()->close_dynamic_library(reflex_handle);
		reflex_handle = nullptr;
	}
	if (nvapi_handle) {
		OS::get_singleton()->close_dynamic_library(nvapi_handle);
		nvapi_handle = nullptr;
	}
#endif
	supported = false;
}

void ReflexBackend::begin_frame() {
	if (!supported || !config.enabled) {
		return;
	}

#ifdef REFLEX_ENABLED
	// Reflex begin frame.
#endif
}

void ReflexBackend::end_frame() {
	if (!supported || !config.enabled) {
		return;
	}

#ifdef REFLEX_ENABLED
	// Reflex end frame.
#endif
}

void ReflexBackend::set_marker(const char *p_name) {
	if (!supported || !config.enabled || !p_name) {
		return;
	}

#ifdef REFLEX_ENABLED
	// Reflex latency marker.
#endif
}

void ReflexBackend::un_set_marker() {
	if (!supported || !config.enabled) {
		return;
	}

#ifdef REFLEX_ENABLED
	// Reflex un-set marker.
#endif
}

void ReflexBackend::register_render_target(RID p_rid, const char *p_name) {
	if (!supported || !config.enabled) {
		return;
	}

#ifdef REFLEX_ENABLED
	// Register render target with Reflex.
#endif
}

void ReflexBackend::unregister_render_target(RID p_rid) {
	if (!supported || !config.enabled) {
		return;
	}

#ifdef REFLEX_ENABLED
	// Unregister render target from Reflex.
#endif
}
