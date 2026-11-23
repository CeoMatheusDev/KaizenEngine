//************************************************************************//
//  upscaling_interface.h                                                 //
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

class RenderSceneBuffersRD;

// Unified upscaling interface supporting DLSS, FSR2, FSR, MetalFX, etc.
// All upscaling technologies implement this interface to provide a
// consistent API to the rendering pipeline.
class UpscalingInterface : public RefCounted {
public:
	enum Type {
		TYPE_NONE,
		TYPE_FSR,
		TYPE_FSR2,
		TYPE_DLSS,
		TYPE_METALFX_SPATIAL,
		TYPE_METALFX_TEMPORAL,
		TYPE_MAX,
	};

	enum Mode {
		MODE_NONE,
		MODE_SPATIAL,
		MODE_TEMPORAL,
		MODE_MAX,
	};

	// Returns the upscaler type.
	virtual Type get_type() const = 0;

	// Returns the upscaling mode (spatial/temporal).
	virtual Mode get_mode() const = 0;

	// Returns a human-readable label for debugging/timestamps.
	virtual const char *get_label() const = 0;

	// Returns true if this upscaler is supported on the current hardware.
	virtual bool is_supported() const = 0;

	// Ensures the upscaler context is created/initialized.
	virtual void ensure_context(Ref<RenderSceneBuffersRD> p_render_buffers) = 0;

	// Process/upscale from source to destination texture.
	virtual void process(Ref<RenderSceneBuffersRD> p_render_buffers, RID p_source_texture, RID p_destination_texture) = 0;

	// Returns the internal rendering size (pre-upscale).
	virtual Size2i get_internal_size() const = 0;

	// Sets the internal rendering size.
	virtual void set_internal_size(Size2i p_size) = 0;

	// Returns the target display size.
	virtual Size2i get_target_size() const = 0;

	// Sets the target display size.
	virtual void set_target_size(Size2i p_size) = 0;

	// Returns the quality preset (0 = lowest, higher = better quality).
	virtual int get_quality_preset() const = 0;

	// Sets the quality preset.
	virtual void set_quality_preset(int p_preset) = 0;

	// Returns true if the upscaler requires motion vectors.
	virtual bool requires_motion_vectors() const = 0;

	// Returns true if the upscaler requires depth buffer.
	virtual bool requires_depth() const = 0;

	// Returns true if the upscaler requires reactive mask.
	virtual bool requires_reactive() const = 0;

	// Returns true if the upscaler supports ray reconstruction.
	virtual bool supports_ray_reconstruction() const = 0;

	// Returns true if the upscaler supports frame generation.
	virtual bool supports_frame_generation() const = 0;

	// Enable/disable ray reconstruction.
	virtual void set_ray_reconstruction(bool p_enable) = 0;

	// Enable/disable frame generation.
	virtual void set_frame_generation(bool p_enable) = 0;

	// Returns true if ray reconstruction is enabled.
	virtual bool is_ray_reconstruction_enabled() const = 0;

	// Returns true if frame generation is enabled.
	virtual bool is_frame_generation_enabled() const = 0;

	// Clean up resources.
	virtual void cleanup() = 0;

protected:
	UpscalingInterface() = default;
	virtual ~UpscalingInterface() = default;
};
