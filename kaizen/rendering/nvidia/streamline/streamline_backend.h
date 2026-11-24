//************************************************************************//
//  streamline_backend.h                                                  //
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

#ifdef STREAMLINE_ENABLED
	#include "sl.h"
	#include "sl_core_api.h"
	#include "sl_dlss.h"
	#include "sl_reflex.h"
#endif

namespace RendererRD {

// NVIDIA Streamline backend integration.
// Streamline is NVIDIA's cross-IHV integration framework that wraps
// DLSS, Reflex, NIS, and other RTX technologies.
class StreamlineBackend : public UpscalingInterface {
public:
	struct Config {
		bool enable_dlss = true;
		bool enable_reflex = false;
		bool enable_ray_reconstruction = false;
		bool enable_frame_generation = false;
		int quality_preset = 3;
	};

	StreamlineBackend();
	~StreamlineBackend() override;

	Type get_type() const override { return TYPE_DLSS; }
	Mode get_mode() const override { return MODE_TEMPORAL; }
	const char *get_label() const override { return "Streamline/DLSS"; }
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

	// Reflex integration.
	void reflex_begin_frame();
	void reflex_end_frame();
	void reflex_set_marker(const char *p_name);

private:
	bool init_streamline();
	void shutdown_streamline();
	bool create_dlss_feature();

	Config config;
	Size2i internal_size;
	Size2i target_size;

#ifdef STREAMLINE_ENABLED
	void *sl_handle = nullptr;
	void *sl_device = nullptr;
	void *sl_dlss_feature = nullptr;
	void *sl_reflex_feature = nullptr;
	void *sl_context = nullptr;
#endif

	bool initialized = false;
	bool context_created = false;
};

} // namespace RendererRD
