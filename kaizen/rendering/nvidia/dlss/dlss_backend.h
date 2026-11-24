//************************************************************************//
//  dlss_backend.h                                                        //
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

#include "kaizen/rendering/upscaling/upscaling_interface.h"
#include "servers/rendering/renderer_rd/effects/dlss.h"

#ifdef NGX_DLSS_ENABLED
	#include "nvsdk_ngx.h"
	#include "nvsdk_ngx_defs.h"
	#include "nvsdk_ngx_params.h"
	#include "nvsdk_ngx_loader.h"
	#include "nvsdk_ngx_helpers.h"
	#include "nvsdk_ngx_defs_dlssd.h"
	#include "nvsdk_ngx_params_dlssd.h"
	#include "nvsdk_ngx_helpers_dlssd.h"
	#include "nvsdk_ngx_defs_dlssg.h"
	#include "nvsdk_ngx_params_dlssg.h"
	#include "nvsdk_ngx_helpers_dlssg.h"
	#include "nvsdk_ngx_vk.h"
#endif

namespace RendererRD {

// DLSS backend implementation using NVIDIA NGX/Streamline SDK.
// Supports DLSS Super Resolution, Ray Reconstruction, and Frame Generation.
class DLSSBackend : public UpscalingInterface {
public:
	enum Feature {
		FEATURE_NONE = 0,
		FEATURE_SUPER_RESOLUTION = 1 << 0,
		FEATURE_RAY_RECONSTRUCTION = 1 << 1,
		FEATURE_FRAME_GENERATION = 1 << 2,
		FEATURE_ALL = FEATURE_SUPER_RESOLUTION | FEATURE_RAY_RECONSTRUCTION | FEATURE_FRAME_GENERATION,
	};

	struct Config {
		bool enable_super_resolution = true;
		bool enable_ray_reconstruction = false;
		bool enable_frame_generation = false;
		int quality_preset = 3; // DLSS Quality preset
		bool auto_exposure = true;
		bool hdr = false;
	};

	DLSSBackend();
	~DLSSBackend() override;

	Type get_type() const override { return TYPE_DLSS; }
	Mode get_mode() const override { return MODE_TEMPORAL; }
	const char *get_label() const override { return "DLSS"; }
	bool is_supported() const override;

	void ensure_context(Ref<RenderSceneBuffersRD> p_render_buffers) override;
	void process(Ref<RenderSceneBuffersRD> p_render_buffers, RID p_source_texture, RID p_destination_texture) override;

	Size2i get_internal_size() const override { return internal_size; }
	void set_internal_size(Size2i p_size) override { internal_size = p_size; }
	Size2i get_target_size() const override { return target_size; }
	void set_target_size(Size2i p_size) override { target_size = p_size; }

	int get_quality_preset() const override { return config.quality_preset; }
	void set_quality_preset(int p_preset) override { config.quality_preset = p_preset; }

	bool requires_motion_vectors() const override { return true; }
	bool requires_depth() const override { return true; }
	bool requires_reactive() const override { return true; }

	bool supports_ray_reconstruction() const override { return true; }
	bool supports_frame_generation() const override { return true; }

	void set_ray_reconstruction(bool p_enable) override { config.enable_ray_reconstruction = p_enable; }
	void set_frame_generation(bool p_enable) override { config.enable_frame_generation = p_enable; }

	bool is_ray_reconstruction_enabled() const override { return config.enable_ray_reconstruction; }
	bool is_frame_generation_enabled() const override { return config.enable_frame_generation; }

	void cleanup() override;

	// Access to the low-level DLSSEffect (existing singleton).
	DLSSEffect *get_dlss_effect() const { return DLSSEffect::get_singleton(); }

	// NGX library handle access.
	void *get_ngx_handle() const { return ngx_handle; }

private:
	bool init_ngx();
	void shutdown_ngx();
	bool create_dlss_context();
	void destroy_dlss_context();
	void dispatch_super_resolution(Ref<RenderSceneBuffersRD> p_render_buffers, RID p_source_texture, RID p_destination_texture);
	void dispatch_ray_reconstruction(Ref<RenderSceneBuffersRD> p_render_buffers, RID p_source_texture, RID p_destination_texture);
	void dispatch_frame_generation(Ref<RenderSceneBuffersRD> p_render_buffers);

	Config config;
	Size2i internal_size;
	Size2i target_size;

	void *ngx_handle = nullptr;
	DLSSContext *dlss_sr_context = nullptr;
	DLSSContext *dlss_rr_context = nullptr;
	DLSSContext *dlss_fg_context = nullptr;

	bool context_created = false;
	bool initialized = false;
};

} // namespace RendererRD
