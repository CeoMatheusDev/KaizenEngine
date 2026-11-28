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

#include "core/config/engine.h"
#include "core/config/project_settings.h"
#include "core/object/class_db.h"
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
		rtx_backend_singleton->load_settings();
	}
	return rtx_backend_singleton.ptr();
}

void RTXBackend::cleanup_singleton() {
	if (rtx_backend_singleton.is_null()) {
		return;
	}
	Engine *engine = Engine::get_singleton();
	if (engine) {
		engine->remove_singleton("RTXBackend");
	}
	rtx_backend_singleton.unref();
}

void RTXBackend::load_settings() {
	ProjectSettings *ps = ProjectSettings::get_singleton();
	if (!ps) {
		return;
	}
	// Tolerate running before the settings are registered (init order).
	auto get_bool = [&](const char *p_name, bool p_current) -> bool {
		return ps->has_setting(p_name) ? (bool)ps->get_setting(p_name) : p_current;
	};
	config.enabled = get_bool("rendering/ray_tracing/enabled", config.enabled);
	config.ray_reconstruction = get_bool("rendering/ray_tracing/ray_reconstruction", config.ray_reconstruction);
	config.denoising = get_bool("rendering/ray_tracing/denoising", config.denoising);
	config.hardware_acceleration = get_bool("rendering/ray_tracing/hardware_acceleration", config.hardware_acceleration);
	if (ps->has_setting("rendering/ray_tracing/max_recursion_depth")) {
		config.max_recursion_depth = (int)ps->get_setting("rendering/ray_tracing/max_recursion_depth");
	}
}

void RTXBackend::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_enabled", "enabled"), &RTXBackend::set_enabled);
	ClassDB::bind_method(D_METHOD("is_enabled"), &RTXBackend::is_enabled);
	ClassDB::bind_method(D_METHOD("set_ray_reconstruction_enabled", "enabled"), &RTXBackend::set_ray_reconstruction_enabled);
	ClassDB::bind_method(D_METHOD("is_ray_reconstruction_enabled"), &RTXBackend::is_ray_reconstruction_enabled);
	ClassDB::bind_method(D_METHOD("set_denoising_enabled", "enabled"), &RTXBackend::set_denoising_enabled);
	ClassDB::bind_method(D_METHOD("is_denoising_enabled"), &RTXBackend::is_denoising_enabled);
	ClassDB::bind_method(D_METHOD("set_hardware_acceleration_enabled", "enabled"), &RTXBackend::set_hardware_acceleration_enabled);
	ClassDB::bind_method(D_METHOD("is_hardware_acceleration_enabled"), &RTXBackend::is_hardware_acceleration_enabled);
	ClassDB::bind_method(D_METHOD("set_max_recursion_depth", "depth"), &RTXBackend::set_max_recursion_depth);
	ClassDB::bind_method(D_METHOD("get_max_recursion_depth"), &RTXBackend::get_max_recursion_depth);

	ClassDB::bind_method(D_METHOD("is_supported"), &RTXBackend::is_supported);
	ClassDB::bind_method(D_METHOD("init"), &RTXBackend::init);
	ClassDB::bind_method(D_METHOD("shutdown"), &RTXBackend::shutdown);
	ClassDB::bind_method(D_METHOD("load_settings"), &RTXBackend::load_settings);
	ClassDB::bind_method(D_METHOD("is_ray_tracing_supported"), &RTXBackend::is_ray_tracing_supported);
	ClassDB::bind_method(D_METHOD("is_ray_query_supported"), &RTXBackend::is_ray_query_supported);
	ClassDB::bind_method(D_METHOD("is_hardware_acceleration_available"), &RTXBackend::is_hardware_acceleration_available);

	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "enabled"), "set_enabled", "is_enabled");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "ray_reconstruction_enabled"), "set_ray_reconstruction_enabled", "is_ray_reconstruction_enabled");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "denoising_enabled"), "set_denoising_enabled", "is_denoising_enabled");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "hardware_acceleration_enabled"), "set_hardware_acceleration_enabled", "is_hardware_acceleration_enabled");
	ADD_PROPERTY(PropertyInfo(Variant::INT, "max_recursion_depth", PROPERTY_HINT_RANGE, "1,64,1"), "set_max_recursion_depth", "get_max_recursion_depth");
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
