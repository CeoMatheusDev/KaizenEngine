//************************************************************************//
//  movie_writer.h                                                        //
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

#pragma once

#include "core/io/image.h"
#include "core/templates/local_vector.h"
#include "servers/audio/audio_server.h" // IWYU pragma: keep. Bind GDVIRTUAL0RC_REQUIRED.
#include "servers/audio/audio_server_enums.h"

class MovieWriter : public Object {
	GDCLASS(MovieWriter, Object);

	uint64_t fps = 0;
	uint64_t mix_rate = 0;
	uint32_t audio_channels = 0;

	// The output resolution, which can differ from the window size.
	// Used as a base for resizing all subsequent frames if their resolution differs.
	Vector2i movie_size;

	float cpu_time = 0.0f;
	float gpu_time = 0.0f;
	uint64_t encoding_time_usec = 0;

	String project_name;

	LocalVector<int32_t> audio_mix_buffer;

	enum {
		MAX_WRITERS = 8
	};
	static MovieWriter *writers[];
	static uint32_t writer_count;

protected:
	virtual uint32_t get_audio_mix_rate() const;
	virtual AuSE::SpeakerMode get_audio_speaker_mode() const;

	virtual Error write_begin(const Size2i &p_movie_size, uint32_t p_fps, const String &p_base_path);
	virtual Error write_frame(const Ref<Image> &p_image, const int32_t *p_audio_data);
	virtual void write_end();

	GDVIRTUAL0RC_REQUIRED(uint32_t, _get_audio_mix_rate)
	GDVIRTUAL0RC_REQUIRED(AuSE::SpeakerMode, _get_audio_speaker_mode)

	GDVIRTUAL1RC_REQUIRED(bool, _handles_file, const String &)
	GDVIRTUAL0RC_REQUIRED(Vector<String>, _get_supported_extensions)

	GDVIRTUAL3R_REQUIRED(Error, _write_begin, const Size2i &, uint32_t, const String &)
	GDVIRTUAL2R_REQUIRED(Error, _write_frame, const Ref<Image> &, GDExtensionPtr<const int32_t>)
	GDVIRTUAL0_REQUIRED(_write_end)

	static void _bind_methods();

public:
	virtual bool handles_file(const String &p_path) const;
	virtual void get_supported_extensions(List<String> *r_extensions) const;

	static void add_writer(MovieWriter *p_writer);
	static MovieWriter *find_writer_for_file(const String &p_file);

	void begin(const Size2i &p_movie_size, uint32_t p_fps, const String &p_base_path);
	void add_frame();

	static void set_extensions_hint();

	void end();
};
