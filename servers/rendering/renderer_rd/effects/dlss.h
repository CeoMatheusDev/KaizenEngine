//************************************************************************//
//  dlss.h                                                                //
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

#include "core/object/class_db.h"
#include "servers/rendering/rendering_device.h"
#include "servers/rendering/renderer_rd/storage_rd/render_scene_buffers_rd.h"

// NVIDIA DLSS SDK (NGX) headers. Compile-time only: the SDK library itself is
// loaded at runtime (see dlss.cpp), so builds work without it installed.
#ifdef NGX_DLSS_ENABLED
// Vulkan headers must be included before NGX VK headers. Through the engine's
// Vulkan umbrella so volk builds define VK_NO_PROTOTYPES first.
#include <drivers/vulkan/kaizen_vulkan.h>
#include <nvsdk_ngx.h>
#include <nvsdk_ngx_defs.h>
#include <nvsdk_ngx_params.h>
#include <nvsdk_ngx_helpers.h>
#include <nvsdk_ngx_defs_dlssd.h>
#include <nvsdk_ngx_params_dlssd.h>
#include <nvsdk_ngx_defs_dlssg.h>
#include <nvsdk_ngx_params_dlssg.h>
#include <nvsdk_ngx_vk.h>
#endif

namespace RendererRD {

	// A created DLSS feature (one per viewport resolution pair). The NGX handle is
	// owned by this object: deleting the context releases the feature.
	class DLSSContext {
	public:
		// NVSDK_NGX_Handle* when created through NGX; nullptr otherwise.
		void *handle = nullptr;
		Size2i internal_size;
		Size2i target_size;
		bool created = false;
		// True when the feature was created as DLSS Ray Reconstruction (DLSSD)
		// instead of DLSS Super Resolution.
		bool ray_reconstruction = false;

	DLSSContext() {}
	~DLSSContext();
};

class DLSSEffect : public Object {
	GDCLASS(DLSSEffect, Object);

public:
	struct Parameters {
		DLSSContext *context = nullptr;
		Size2i internal_size;
		Size2i target_size;
		RID color;
		RID depth;
		RID velocity;
		RID reactive;
		RID exposure;
		RID output;
		float z_near = 0.0f;
		float z_far = 0.0f;
		float fovy = 0.0f;
		Vector2 jitter;
		float delta_time = 0.0f;
		float sharpness = 0.0f;
		bool reset_accumulation = false;
		Projection reprojection;
	};

	static DLSSEffect *get_singleton();

	DLSSEffect();
	~DLSSEffect() override;

	// True only when the SDK library loaded and NGX initialised on the active
	// Vulkan device. When false the caller must fall back (FSR2 / bilinear).
	bool is_supported() const;

	// Creates the NGX DLSS feature for the given resolution pair. Returns a
	// context whose `created` flag reports whether the SDK accepted it; the
	// context object exists either way so callers can retry after a resize.
	// `p_quality` is a RenderingServerEnums::ViewportDLSSQuality value (-1 =
	// Max Quality). `p_rr_mode` selects Ray Reconstruction: 1 = on, 0 = off,
	// -1 = follow is_ray_reconstruction_enabled(). When Ray Reconstruction is
	// requested but unavailable, Super Resolution is created instead.
	DLSSContext *create_context(Size2i p_internal_size, Size2i p_target_size, int p_quality = -1, int p_rr_mode = -1);
	void upscale(const Parameters &p_params);

	// DLSS Ray Reconstruction (DLSSD) toggle. When enabled, create_context()
	// creates the Ray Reconstruction feature instead of Super Resolution.
	// -1 (default) follows RTXBackend::is_ray_reconstruction_enabled().
	void set_ray_reconstruction(bool p_enabled) { rr_override = p_enabled ? 1 : 0; }
	bool is_ray_reconstruction_enabled() const;

private:
	bool supported = false;
	int rr_override = -1;

	bool init_sdk();
	void shutdown_sdk();

	void *library_handle = nullptr;
	void *parameters = nullptr; // NVSDK_NGX_Parameter*
	void *vulkan_instance = nullptr;
	void *vulkan_device = nullptr;
	void *command_pool = nullptr;
	void *command_buffer = nullptr;
	void *queue = nullptr;
	uint32_t queue_family = 0;
};

} // namespace RendererRD
