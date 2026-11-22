//************************************************************************//
//  renderer_scene_occlusion_cull.cpp                                     //
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
// See the files LICENSE.txt and LICENSE-ADDITIONAL-TERMS.md for details. //
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

#include "renderer_scene_occlusion_cull.h"

#include "core/config/engine.h"
#include "servers/rendering/rendering_server.h"

RendererSceneOcclusionCull *RendererSceneOcclusionCull::singleton = nullptr;

bool RendererSceneOcclusionCull::HZBuffer::occlusion_jitter_enabled = false;

void RendererSceneOcclusionCull::HZBuffer::clear() {
	if (sizes.is_empty()) {
		return; // Already cleared
	}

	data.clear();
	sizes.clear();
	mips.clear();

	debug_data.clear();
	if (debug_image.is_valid()) {
		debug_image.unref();
	}

	ERR_FAIL_NULL(RenderingServer::get_singleton());
	RS::get_singleton()->free_rid(debug_texture);
}

void RendererSceneOcclusionCull::HZBuffer::resize(const Size2i &p_size) {
	occlusion_buffer_size = p_size;

	if (p_size == Size2i()) {
		clear();
		return;
	}

	if (!sizes.is_empty() && p_size == sizes[0]) {
		return; // Size didn't change
	}

	int mip_count = 0;
	int data_size = 0;
	int w = p_size.x;
	int h = p_size.y;

	while (true) {
		data_size += h * w;

		w = MAX(1, w >> 1);
		h = MAX(1, h >> 1);

		mip_count++;

		if (w == 1U && h == 1U) {
			data_size += 1U;
			mip_count++;
			break;
		}
	}

	data.resize(data_size);
	mips.resize(mip_count);
	sizes.resize(mip_count);

	w = p_size.x;
	h = p_size.y;
	float *ptr = data.ptr();

	for (int i = 0; i < mip_count; i++) {
		sizes[i] = Size2i(w, h);
		mips[i] = ptr;

		ptr = &ptr[w * h];
		w = MAX(1, w >> 1);
		h = MAX(1, h >> 1);
	}

	for (int i = 0; i < data_size; i++) {
		data[i] = FLT_MAX;
	}

	debug_data.resize(sizes[0].x * sizes[0].y);
	if (debug_texture.is_valid()) {
		RS::get_singleton()->free_rid(debug_texture);
		debug_texture = RID();
	}
}

void RendererSceneOcclusionCull::HZBuffer::update_mips() {
	// Keep this up to date as a local to be used for occlusion timers.
	occlusion_frame = Engine::get_singleton()->get_frames_drawn();

	if (sizes.is_empty()) {
		return;
	}

	// The whole pyramid is rebuilt every frame per viewport, so this loop runs over
	// about 4/3 of the occlusion buffer's texels. That buffer is a ray budget and not
	// the screen: RendererViewport::_draw_3d() sizes it from
	// rendering/occlusion_culling/occlusion_rays_per_thread times the thread count,
	// clamped to at most one texel per 2x2 screen region. The previous level's
	// dimensions, their parity and the level pointers do not depend on the texel being
	// written, and clamping an offset to the previous level's edge only ever changes
	// the result on the last row or column, so all of it is hoisted out of the loop.
	for (uint32_t mip = 1; mip < mips.size(); mip++) {
		const int prev_w = sizes[mip - 1].width;
		const int prev_h = sizes[mip - 1].height;
		const int w = sizes[mip].width;
		const int h = sizes[mip].height;

		const float *prev = mips[mip - 1];
		float *dst = mips[mip];

		const bool odd_w = (prev_w % 2) != 0;
		const bool odd_h = (prev_h % 2) != 0;

		const int last_x = w - 1;
		const int last_y = h - 1;

		for (int y = 0; y <= last_y; y++) {
			const int prev_y = y * 2;
			const int prev_y1 = (y == last_y) ? MIN(prev_h - 1, prev_y + 1) : prev_y + 1;
			const int prev_y2 = (y == last_y) ? prev_h - 1 : prev_y + 2;

			const float *row0 = &prev[prev_y * prev_w];
			const float *row1 = &prev[prev_y1 * prev_w];
			const float *row2 = &prev[prev_y2 * prev_w];
			float *out = &dst[y * w];

			for (int x = 0; x <= last_x; x++) {
				const int prev_x = x * 2;
				const int prev_x1 = (x == last_x) ? MIN(prev_w - 1, prev_x + 1) : prev_x + 1;
				const int prev_x2 = (x == last_x) ? prev_w - 1 : prev_x + 2;

#define CHECK_OFFSET(rr, cc) max_depth = MAX(max_depth, (rr)[cc])

				float max_depth = row0[prev_x];
				CHECK_OFFSET(row1, prev_x);
				CHECK_OFFSET(row0, prev_x1);
				CHECK_OFFSET(row1, prev_x1);

				if (odd_w) {
					CHECK_OFFSET(row0, prev_x2);
					CHECK_OFFSET(row1, prev_x2);
				}

				if (odd_h) {
					CHECK_OFFSET(row2, prev_x);
					CHECK_OFFSET(row2, prev_x1);
				}

				if (odd_w && odd_h) {
					CHECK_OFFSET(row2, prev_x2);
				}
#undef CHECK_OFFSET

				out[x] = max_depth;
			}
		}
	}
}

RID RendererSceneOcclusionCull::HZBuffer::get_debug_texture() {
	if (sizes.is_empty() || sizes[0] == Size2i()) {
		return RID();
	}

	if (debug_image.is_null()) {
		debug_image.instantiate();
	}

	unsigned char *ptrw = debug_data.ptrw();
	for (int i = 0; i < debug_data.size(); i++) {
		ptrw[i] = MIN(Math::log(1.0 + mips[0][i]) / Math::log(1.0 + debug_tex_range), 1.0) * 255;
	}

	debug_image->set_data(sizes[0].x, sizes[0].y, false, Image::FORMAT_L8, debug_data);

	if (debug_texture.is_null()) {
		debug_texture = RS::get_singleton()->texture_2d_create(debug_image);
	} else {
		RenderingServer::get_singleton()->texture_2d_update(debug_texture, debug_image);
	}

	return debug_texture;
}
