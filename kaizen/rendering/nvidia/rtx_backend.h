//************************************************************************//
//  rtx_backend.h                                                         //
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

// NVIDIA RTX backend - wraps existing Vulkan ray tracing infrastructure
// with NVIDIA-specific optimizations and features (DLSS Ray Reconstruction, etc.)
class RTXBackend : public RefCounted {
	GDCLASS(RTXBackend, RefCounted);

public:
	struct Config {
		bool enabled = false;
		bool ray_tracing = true;
		bool ray_reconstruction = false;
		bool denoising = false;
		bool hardware_acceleration = true;
		int max_recursion_depth = 32;
	};

protected:
	static void _bind_methods();

public:
	RTXBackend();
	~RTXBackend() override;

	// Lazy singleton used by consumers (e.g. DLSSEffect) to read the RTX
	// configuration. Created on first access and destroyed at shutdown().
	static RTXBackend *get_singleton();
	// Releases the singleton. Called from the servers unregister step, while the ObjectDB is
	// still alive; the engine must drop its raw pointer to it first (see register_server_types).
	static void destroy_singleton();

	bool is_supported() const { return supported; }
	bool init();
	void shutdown();

	// Ray tracing pipeline management.
	bool is_ray_tracing_supported() const;
	bool is_ray_query_supported() const;
	bool is_hardware_acceleration_available() const;

	// Configuration.
	const Config &get_config() const { return config; }
	void set_config(const Config &p_config) { config = p_config; }

	// Loads the `rendering/ray_tracing/*` project settings into the config.
	void load_settings();

	// Feature toggles (config shortcuts).
	void set_enabled(bool p_enabled) { config.enabled = p_enabled; }
	bool is_enabled() const { return config.enabled; }
	bool is_ray_reconstruction_enabled() const { return config.ray_reconstruction; }
	void set_ray_reconstruction_enabled(bool p_enabled) { config.ray_reconstruction = p_enabled; }
	bool is_denoising_enabled() const { return config.denoising; }
	void set_denoising_enabled(bool p_enabled) { config.denoising = p_enabled; }
	bool is_hardware_acceleration_enabled() const { return config.hardware_acceleration; }
	void set_hardware_acceleration_enabled(bool p_enabled) { config.hardware_acceleration = p_enabled; }

	// Capabilities.
	uint32_t get_max_recursion_depth() const { return config.max_recursion_depth; }
	void set_max_recursion_depth(uint32_t p_depth) { config.max_recursion_depth = p_depth; }

private:
	bool supported = false;
	Config config;

	RenderingDevice *rd = nullptr;
};
