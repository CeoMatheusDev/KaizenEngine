//************************************************************************//
//  rtx_backend.cpp                                                       //
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

#include "rtx_backend.h"

#include "servers/rendering/rendering_device.h"

RTXBackend::RTXBackend() {
	config.enabled = false;
	config.ray_tracing = true;
	config.ray_reconstruction = false;
	config.denoising = false;
	config.hardware_acceleration = true;
	config.max_recursion_depth = 32;
	supported = false;
	rd = nullptr;
}

RTXBackend::~RTXBackend() {
	shutdown();
}

static Ref<RTXBackend> rtx_backend_singleton;

RTXBackend *RTXBackend::get_singleton() {
	if (rtx_backend_singleton.is_null()) {
		rtx_backend_singleton = memnew(RTXBackend);
	}
	return rtx_backend_singleton.ptr();
}

bool RTXBackend::init() {
	rd = RD::get_singleton();
	if (!rd) {
		return false;
	}

	String vendor = rd->get_device_vendor_name();
	if (vendor != "NVIDIA") {
		return false;
	}

	// Check ray tracing support.
	if (!rd->has_feature(RD::SUPPORTS_RAYTRACING_PIPELINE) && !rd->has_feature(RD::SUPPORTS_RAY_QUERY)) {
		WARN_PRINT("RTXBackend: Ray tracing not supported on this device.");
		return false;
	}

	supported = true;
	return true;
}

void RTXBackend::shutdown() {
	supported = false;
	rd = nullptr;
}

bool RTXBackend::is_ray_tracing_supported() const {
	if (!rd) {
		return false;
	}
	return rd->has_feature(RD::SUPPORTS_RAYTRACING_PIPELINE);
}

bool RTXBackend::is_ray_query_supported() const {
	if (!rd) {
		return false;
	}
	return rd->has_feature(RD::SUPPORTS_RAY_QUERY);
}

bool RTXBackend::is_hardware_acceleration_available() const {
	if (!rd) {
		return false;
	}
	// Hardware acceleration is available if the device supports ray tracing pipelines.
	return rd->has_feature(RD::SUPPORTS_RAYTRACING_PIPELINE);
}
