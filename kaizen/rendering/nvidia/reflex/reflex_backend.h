//************************************************************************//
//  reflex_backend.h                                                      //
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
#include "servers/rendering/rendering_device.h"

#ifdef REFLEX_ENABLED
	#include "sl_reflex.h"
	#include "nvReflex.h"
#endif

namespace RendererRD {

// NVIDIA Reflex integration for low-latency rendering.
// Provides Low Latency Mode, Frame Warp, and PC Latency Stats.
class ReflexBackend : public RefCounted {
public:
	struct Config {
		bool enabled = false;
		bool low_latency_mode = false;
		bool frame_warp = false;
		bool latency_stats = false;
		bool analyzer = false;
	};

	ReflexBackend();
	~ReflexBackend() override;

	bool is_supported() const { return supported; }
	bool init();
	void shutdown();

	// Frame lifecycle.
	void begin_frame();
	void end_frame();

	// Latency markers.
	void set_marker(const char *p_name);
	void un_set_marker();

	// Register/deregister render targets.
	void register_render_target(RID p_rid, const char *p_name);
	void unregister_render_target(RID p_rid);

	// Get configuration.
	const Config &get_config() const { return config; }
	void set_config(const Config &p_config) { config = p_config; }

private:
	bool supported = false;
	Config config;

#ifdef REFLEX_ENABLED
	void *reflex_handle = nullptr;
	void *nvapi_handle = nullptr;
#endif
};

} // namespace RendererRD
