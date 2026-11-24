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

// NVIDIA DLSS SDK (NGX) headers.
#ifdef NGX_DLSS_ENABLED
#include <nvsdk_ngx.h>
#include <nvsdk_ngx_defs.h>
#include <nvsdk_ngx_params.h>
#include <nvsdk_ngx_loader.h>
#include <nvsdk_ngx_helpers.h>
#include <nvsdk_ngx_defs_dlssd.h>
#include <nvsdk_ngx_params_dlssd.h>
#include <nvsdk_ngx_defs_dlssg.h>
#include <nvsdk_ngx_params_dlssg.h>
#include <nvsdk_ngx_vk.h>
#endif

namespace RendererRD {

class DLSSContext {
public:
	// DLSS implementation handles (NGX / Streamline handles).
	void *handle = nullptr;

	DLSSContext();
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

	bool is_supported() const;
	DLSSContext *create_context(Size2i p_internal_size, Size2i p_target_size);
	void upscale(const Parameters &p_params);

private:
	bool supported = false;
};

} // namespace RendererRD
