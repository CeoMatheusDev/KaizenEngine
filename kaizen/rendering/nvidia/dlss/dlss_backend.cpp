//************************************************************************//
//  dlss_backend.cpp                                                      //
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

#include "dlss_backend.h"

#include "core/os/os.h"
#include "servers/rendering/rendering_device.h"
#include "servers/rendering/renderer_rd/storage_rd/material_storage.h"

using namespace RendererRD;

DLSSBackend::DLSSBackend() {
	internal_size = Size2i(1920, 1080);
	target_size = Size2i(1920, 1080);
	initialized = false;
	context_created = false;

#ifdef NGX_DLSS_ENABLED
	ngx_handle = nullptr;
	dlss_sr_handle = nullptr;
	dlss_rr_handle = nullptr;
	dlss_fg_handle = nullptr;
#endif
}

DLSSBackend::~DLSSBackend() {
	cleanup();
}

bool DLSSBackend::is_supported() const {
	// Check if NVIDIA GPU is present.
	RenderingDevice *rd = RD::get_singleton();
	if (!rd) {
		return false;
	}

	String vendor = rd->get_device_vendor_name();
	if (vendor != "NVIDIA") {
		return false;
	}

	// Check if DLSS SDK is available.
#ifdef NGX_DLSS_ENABLED
	return true;
#else
	return false;
#endif
}

void DLSSBackend::ensure_context(Ref<RenderSceneBuffersRD> p_render_buffers) {
	if (!is_supported()) {
		return;
	}

	if (!initialized) {
		if (!init_ngx()) {
			WARN_PRINT("DLSSBackend: Failed to initialize NGX.");
			return;
		}
		initialized = true;
	}

	if (!context_created) {
		if (!create_dlss_context()) {
			WARN_PRINT("DLSSBackend: Failed to create DLSS context.");
			return;
		}
		context_created = true;
	}
}

void DLSSBackend::process(Ref<RenderSceneBuffersRD> p_render_buffers, RID p_source_texture, RID p_destination_texture) {
	if (!is_supported() || !context_created) {
		return;
	}

	// Dispatch super resolution.
	if (config.enable_super_resolution) {
		dispatch_super_resolution(p_render_buffers, p_source_texture, p_destination_texture);
	}

	// Dispatch ray reconstruction if enabled.
	if (config.enable_ray_reconstruction && supports_ray_reconstruction()) {
		dispatch_ray_reconstruction(p_render_buffers, p_source_texture, p_destination_texture);
	}

	// Frame generation is handled separately in the frame pipeline.
	if (config.enable_frame_generation && supports_frame_generation()) {
		dispatch_frame_generation(p_render_buffers);
	}
}

void DLSSBackend::cleanup() {
	shutdown_ngx();
	context_created = false;
	initialized = false;
}

bool DLSSBackend::init_ngx() {
#ifdef NGX_DLSS_ENABLED
	// Load the NGX/Streamline library.
	// The library path is configured via the build system.
	ngx_handle = OS::get_singleton()->load_dynamic_library("nvngx_core");
	if (!ngx_handle) {
		WARN_PRINT("DLSSBackend: Failed to load nvngx_core library.");
		return false;
	}

	// Initialize NGX.
	// Actual NGX initialization functions would be called here using
	// dlsym/GetProcAddress on the loaded library.
#endif
	return true;
}

void DLSSBackend::shutdown_ngx() {
#ifdef NGX_DLSS_ENABLED
	if (ngx_handle) {
		OS::get_singleton()->unload_dynamic_library(ngx_handle);
		ngx_handle = nullptr;
	}
	dlss_sr_handle = nullptr;
	dlss_rr_handle = nullptr;
	dlss_fg_handle = nullptr;
#endif
}

bool DLSSBackend::create_dlss_context() {
#ifdef NGX_DLSS_ENABLED
	// Create DLSS Super Resolution context via NGX.
	// This would call nvngx_dlss_create or similar NGX API.
	dlss_sr_handle = memnew(DLSSContext); // Placeholder using existing DLSSContext

	if (!dlss_sr_handle) {
		return false;
	}

	// If ray reconstruction is enabled, create RR context.
	if (config.enable_ray_reconstruction) {
		dlss_rr_handle = memnew(DLSSContext);
	}

	// If frame generation is enabled, create FG context.
	if (config.enable_frame_generation) {
		dlss_fg_handle = memnew(DLSSContext);
	}
#endif
	return true;
}

void DLSSBackend::destroy_dlss_context() {
#ifdef NGX_DLSS_ENABLED
	if (dlss_sr_handle) {
		memdelete(dlss_sr_handle);
		dlss_sr_handle = nullptr;
	}
	if (dlss_rr_handle) {
		memdelete(dlss_rr_handle);
		dlss_rr_handle = nullptr;
	}
	if (dlss_fg_handle) {
		memdelete(dlss_fg_handle);
		dlss_fg_handle = nullptr;
	}
#endif
}

void DLSSBackend::dispatch_super_resolution(Ref<RenderSceneBuffersRD> p_render_buffers, RID p_source_texture, RID p_destination_texture) {
#ifdef NGX_DLSS_ENABLED
	// Build DLSS parameters from render buffers.
	DLSSEffect::Parameters params;
	params.context = static_cast<DLSSContext *>(dlss_sr_handle);
	params.internal_size = internal_size;
	params.target_size = target_size;
	params.color = p_source_texture;
	params.output = p_destination_texture;

	// Get depth, velocity, reactive, exposure from render buffers.
	params.depth = p_render_buffers->get_depth_texture();
	params.velocity = p_render_buffers->get_velocity_texture();
	params.reactive = p_render_buffers->get_reactive_texture();
	params.exposure = p_render_buffers->get_exposure_texture();

	// Get camera parameters.
	params.z_near = p_render_buffers->get_z_near();
	params.z_far = p_render_buffers->get_z_far();
	params.fovy = p_render_buffers->get_fovy();
	params.jitter = p_render_buffers->get_jitter();
	params.delta_time = p_render_buffers->get_delta_time();
	params.sharpness = 1.0f; // Default sharpness
	params.reset_accumulation = false;
	params.reprojection = p_render_buffers->get_reprojection_matrix();

	// Call DLSS upscale via the existing DLSSEffect singleton.
	DLSSEffect::get_singleton()->upscale(params);
#else
	WARN_PRINT_ONCE("DLSSBackend: DLSS SDK not available. Cannot dispatch super resolution.");
#endif
}

void DLSSBackend::dispatch_ray_reconstruction(Ref<RenderSceneBuffersRD> p_render_buffers, RID p_source_texture, RID p_destination_texture) {
#ifdef NGX_DLSS_ENABLED
	// Ray reconstruction uses similar parameters but with ray tracing-specific inputs.
	DLSSEffect::Parameters params;
	params.context = static_cast<DLSSContext *>(dlss_rr_handle);
	params.internal_size = internal_size;
	params.target_size = target_size;
	params.color = p_source_texture;
	params.output = p_destination_texture;
	params.depth = p_render_buffers->get_depth_texture();
	params.velocity = p_render_buffers->get_velocity_texture();
	params.reactive = p_render_buffers->get_reactive_texture();

	DLSSEffect::get_singleton()->upscale(params);
#else
	WARN_PRINT_ONCE("DLSSBackend: DLSS SDK not available. Cannot dispatch ray reconstruction.");
#endif
}

void DLSSBackend::dispatch_frame_generation(Ref<RenderSceneBuffersRD> p_render_buffers) {
#ifdef NGX_DLSS_ENABLED
	// Frame Generation is dispatched separately from the main upscaling pass.
	// It generates a new frame between two rendered frames.
	if (!dlss_fg_handle) {
		return;
	}

	// FG would use the NGX DLSS-G API to generate intermediate frames.
#endif
}
