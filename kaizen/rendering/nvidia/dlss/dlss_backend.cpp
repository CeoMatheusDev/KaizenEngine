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
#include "servers/rendering/renderer_rd/storage_rd/render_scene_buffers_rd.h"

using namespace RendererRD;

DLSSBackend::DLSSBackend() {
	internal_size = Size2i(1920, 1080);
	target_size = Size2i(1920, 1080);
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

	if (rd->get_device_vendor_name() != "NVIDIA") {
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
	destroy_dlss_context();
	shutdown_ngx();
	context_created = false;
	initialized = false;
}

bool DLSSBackend::init_ngx() {
#ifdef NGX_DLSS_ENABLED
	// Load the NGX library. The NGX initialization calls
	// (NVSDK_NGX_VULKAN_Init and friends) are dispatched through the
	// symbols of this library once the SDK is wired in.
	Error err = OS::get_singleton()->open_dynamic_library("libnvidia-ngx-dlss.so", ngx_handle);
	if (err != OK || !ngx_handle) {
		WARN_PRINT("DLSSBackend: Failed to load the NVIDIA NGX library.");
		return false;
	}
#endif
	return true;
}

void DLSSBackend::shutdown_ngx() {
#ifdef NGX_DLSS_ENABLED
	if (ngx_handle) {
		OS::get_singleton()->close_dynamic_library(ngx_handle);
		ngx_handle = nullptr;
	}
#endif
}

bool DLSSBackend::create_dlss_context() {
#ifdef NGX_DLSS_ENABLED
	if (!dlss_sr_context) {
		dlss_sr_context = DLSSEffect::get_singleton()->create_context(internal_size, target_size);
		if (!dlss_sr_context) {
			return false;
		}
	}

	// If ray reconstruction is enabled, create RR context.
	if (config.enable_ray_reconstruction && !dlss_rr_context) {
		dlss_rr_context = DLSSEffect::get_singleton()->create_context(internal_size, target_size);
	}

	// If frame generation is enabled, create FG context.
	if (config.enable_frame_generation && !dlss_fg_context) {
		dlss_fg_context = DLSSEffect::get_singleton()->create_context(internal_size, target_size);
	}
#endif
	return true;
}

void DLSSBackend::destroy_dlss_context() {
#ifdef NGX_DLSS_ENABLED
	if (dlss_sr_context) {
		memdelete(dlss_sr_context);
		dlss_sr_context = nullptr;
	}
	if (dlss_rr_context) {
		memdelete(dlss_rr_context);
		dlss_rr_context = nullptr;
	}
	if (dlss_fg_context) {
		memdelete(dlss_fg_context);
		dlss_fg_context = nullptr;
	}
#endif
}

void DLSSBackend::dispatch_super_resolution(Ref<RenderSceneBuffersRD> p_render_buffers, RID p_source_texture, RID p_destination_texture) {
#ifdef NGX_DLSS_ENABLED
	// Build DLSS parameters from render buffers. Camera parameters
	// (near/far, fovy, jitter, delta time, reprojection) are supplied by
	// the caller in the full pipeline (see render_forward_clustered.cpp);
	// here we only fill in what the render buffers expose directly.
	DLSSEffect::Parameters params;
	params.context = dlss_sr_context;
	params.internal_size = internal_size;
	params.target_size = target_size;
	params.color = p_source_texture;
	params.output = p_destination_texture;

	params.depth = p_render_buffers->get_depth_texture();
	params.velocity = p_render_buffers->get_velocity_buffer(false);
	params.reactive = p_render_buffers->get_internal_texture_reactive(0);
	params.sharpness = 1.0f; // Default sharpness

	// Call DLSS upscale via the existing DLSSEffect singleton.
	DLSSEffect::get_singleton()->upscale(params);
#else
	(void)p_render_buffers;
	(void)p_source_texture;
	(void)p_destination_texture;
#endif
}

void DLSSBackend::dispatch_ray_reconstruction(Ref<RenderSceneBuffersRD> p_render_buffers, RID p_source_texture, RID p_destination_texture) {
#ifdef NGX_DLSS_ENABLED
	// Ray reconstruction uses similar parameters but with ray tracing-specific inputs.
	DLSSEffect::Parameters params;
	params.context = dlss_rr_context;
	params.internal_size = internal_size;
	params.target_size = target_size;
	params.color = p_source_texture;
	params.output = p_destination_texture;
	params.depth = p_render_buffers->get_depth_texture();
	params.velocity = p_render_buffers->get_velocity_buffer(false);
	params.reactive = p_render_buffers->get_internal_texture_reactive(0);

	DLSSEffect::get_singleton()->upscale(params);
#else
	(void)p_render_buffers;
	(void)p_source_texture;
	(void)p_destination_texture;
#endif
}

void DLSSBackend::dispatch_frame_generation(Ref<RenderSceneBuffersRD> p_render_buffers) {
#ifdef NGX_DLSS_ENABLED
	// Frame Generation is dispatched separately from the main upscaling pass.
	// It generates a new frame between two rendered frames.
	if (!dlss_fg_context) {
		return;
	}

	// FG would use the NGX DLSS-G API to generate intermediate frames.
	(void)p_render_buffers;
#else
	(void)p_render_buffers;
#endif
}
