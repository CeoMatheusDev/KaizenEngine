//************************************************************************//
//  streamline_backend.cpp                                                //
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

#include "streamline_backend.h"

#include "core/os/os.h"
#include "servers/rendering/rendering_device.h"

using namespace RendererRD;

StreamlineBackend::StreamlineBackend() {
	internal_size = Size2i(1920, 1080);
	target_size = Size2i(1920, 1080);
	initialized = false;
	context_created = false;

#ifdef STREAMLINE_ENABLED
	sl_handle = nullptr;
	sl_device = nullptr;
	sl_dlss_feature = nullptr;
	sl_reflex_feature = nullptr;
	sl_context = nullptr;
#endif
}

StreamlineBackend::~StreamlineBackend() {
	cleanup();
}

bool StreamlineBackend::is_supported() const {
	RenderingDevice *rd = RD::get_singleton();
	if (!rd) {
		return false;
	}

	String vendor = rd->get_device_vendor_name();
	if (vendor != "NVIDIA") {
		return false;
	}

#ifdef STREAMLINE_ENABLED
	return true;
#else
	return false;
#endif
}

void StreamlineBackend::ensure_context(Ref<RenderSceneBuffersRD> p_render_buffers) {
	if (!is_supported()) {
		return;
	}

	if (!initialized) {
		if (!init_streamline()) {
			WARN_PRINT("StreamlineBackend: Failed to initialize Streamline.");
			return;
		}
		initialized = true;
	}

	if (!context_created) {
		if (!create_dlss_feature()) {
			WARN_PRINT("StreamlineBackend: Failed to create DLSS feature.");
			return;
		}
		context_created = true;
	}
}

void StreamlineBackend::process(Ref<RenderSceneBuffersRD> p_render_buffers, RID p_source_texture, RID p_destination_texture) {
	if (!is_supported() || !context_created) {
		return;
	}

#ifdef STREAMLINE_ENABLED
	// Streamline DLSS dispatch would go here.
	// This uses slExecute or slDLSS* APIs.
#endif
}

void StreamlineBackend::cleanup() {
	shutdown_streamline();
	context_created = false;
	initialized = false;
}

bool StreamlineBackend::init_streamline() {
#ifdef STREAMLINE_ENABLED
	Error err = OS::get_singleton()->open_dynamic_library("sl.interposer", sl_handle);
	if (err != OK || !sl_handle) {
		WARN_PRINT("StreamlineBackend: Failed to load the Streamline interposer.");
		return false;
	}

	// Initialize Streamline (slInit and friends) once the SDK is wired in.
#endif
	return true;
}

void StreamlineBackend::shutdown_streamline() {
#ifdef STREAMLINE_ENABLED
	if (sl_handle) {
		OS::get_singleton()->close_dynamic_library(sl_handle);
		sl_handle = nullptr;
	}
	sl_device = nullptr;
	sl_dlss_feature = nullptr;
	sl_reflex_feature = nullptr;
	sl_context = nullptr;
#endif
}

bool StreamlineBackend::create_dlss_feature() {
#ifdef STREAMLINE_ENABLED
	// Create Streamline DLSS feature.
	// This would call slCreateFeature with SL_FEATURE_DLSS.
	return true;
#else
	return false;
#endif
}

void StreamlineBackend::reflex_begin_frame() {
#ifdef STREAMLINE_ENABLED
	if (sl_reflex_feature && config.enable_reflex) {
		// Reflex begin frame marker.
	}
#endif
}

void StreamlineBackend::reflex_end_frame() {
#ifdef STREAMLINE_ENABLED
	if (sl_reflex_feature && config.enable_reflex) {
		// Reflex end frame marker.
	}
#endif
}

void StreamlineBackend::reflex_set_marker(const char *p_name) {
#ifdef STREAMLINE_ENABLED
	if (sl_reflex_feature && config.enable_reflex && p_name) {
		// Reflex latency marker.
	}
#endif
}
