//************************************************************************//
//  renderer_compositor.h                                                 //
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
// Modified by Matheus A. Santos, Hardion Labs on 2026-11-17.             //
// Modifications and additional code are licensed under the GNU Affero    //
// General Public License v3.0 (AGPL-3.0).                                //
// See the files LICENSE and LICENSE-ADDITIONAL-TERMS.md for details.     //
//                                                                        //
// Permission is hereby granted, free of charge, to any person obtaining  //
// a copy of this software and associated documentation files (the        //
// "Software"), to deal in the Software without restriction, including    //
// without limitation the rights to use, copy, modify, merge, publish,    //
// distribute, sublicense, and/or sell copies of the Software, and to     //
// permit persons to whom the Software is furnished to do so, subject to  //
// the following conditions:                                              //
//                                                                        //
// The above copyright notice and this permission notice shall be         //
// included in all copies or substantial portions of the Software.        //
//                                                                        //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        //
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     //
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. //
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   //
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   //
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE      //
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 //
//************************************************************************//

#pragma once

#include "servers/display/display_server_enums.h"
#include "servers/rendering/rendering_server_enums.h"
#include "servers/rendering/rendering_server_types.h"

class RendererCanvasRender;
class RendererSceneRender;

class RendererFog;
class RendererGI;
class RendererLightStorage;
class RendererMaterialStorage;
class RendererMeshStorage;
class RendererParticlesStorage;
class RendererTextureStorage;
class RendererUtilities;

class Image;
struct Color;

class RendererCompositor {
private:
	bool xr_enabled = false;
	static RendererCompositor *singleton;

protected:
	static RendererCompositor *(*_create_func)();
	bool back_end = false;
	static bool low_end;

public:
	static RendererCompositor *create();

	virtual RendererCanvasRender *get_canvas() = 0;
	virtual RendererSceneRender *get_scene() = 0;

	virtual RendererFog *get_fog() = 0;
	virtual RendererGI *get_gi() = 0;
	virtual RendererLightStorage *get_light_storage() = 0;
	virtual RendererMaterialStorage *get_material_storage() = 0;
	virtual RendererMeshStorage *get_mesh_storage() = 0;
	virtual RendererParticlesStorage *get_particles_storage() = 0;
	virtual RendererTextureStorage *get_texture_storage() = 0;
	virtual RendererUtilities *get_utilities() = 0;

	virtual void set_boot_image_with_stretch(const Ref<Image> &p_image, const Color &p_color, RSE::SplashStretchMode p_stretch_mode, bool p_use_filter = true) = 0;

	virtual void initialize() = 0;
	virtual void begin_frame(double frame_step) = 0;

	virtual void blit_render_targets_to_screen(DisplayServerEnums::WindowID p_screen, const RenderingServerTypes::BlitToScreen *p_render_targets, int p_amount) = 0;

	virtual bool is_opengl() = 0;
	virtual void gl_end_frame(bool p_swap_buffers) = 0;
	virtual void end_frame(bool p_present) = 0;
	virtual void finalize() = 0;
	virtual uint64_t get_frame_number() const = 0;
	virtual double get_frame_delta_time() const = 0;
	virtual double get_total_time() const = 0;
	virtual bool can_create_resources_async() const = 0;

	static bool is_low_end() { return low_end; }
	virtual bool is_xr_enabled() const;

	static RendererCompositor *get_singleton() { return singleton; }
	RendererCompositor();
	virtual ~RendererCompositor();
};
