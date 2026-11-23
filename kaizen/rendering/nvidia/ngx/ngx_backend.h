//************************************************************************//
//  ngx_backend.h                                                         //
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

#ifdef NGX_ENABLED
	#include "nvsdk_ngx.h"
	#include "nvsdk_ngx_core.h"
	#include "nvsdk_ngx_dlss.h"
#endif

namespace RendererRD {

// NVIDIA NGX (Neural Graphics SDK) backend.
// Provides core neural graphics infrastructure for DLSS and other AI features.
class NGXBackend : public RefCounted {
public:
	struct Config {
		bool enabled = false;
		bool dlss_sr = true;
		bool dlss_rr = false;
		bool dlss_fg = false;
		int feature_mask = 0;
	};

	NGXBackend();
	~NGXBackend() override;

	bool is_supported() const { return supported; }
	bool init();
	void shutdown();

	// Get configuration.
	const Config &get_config() const { return config; }
	void set_config(const Config &p_config) { config = p_config; }

	// NGX handle access.
	void *get_handle() const { return ngx_handle; }

private:
	bool supported = false;
	Config config;

#ifdef NGX_ENABLED
	void *ngx_handle = nullptr;
#endif
};

} // namespace RendererRD
