//************************************************************************//
//  winrt_utils.h                                                         //
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

#include "winrt_defines.h"

#include "core/typedefs.h"
#include "core/variant/callable.h"
#include "core/variant/variant.h"
#include "servers/display/display_server_enums.h"

#include <windows.h>

class WinRTWindowData;
class Texture2D;

class WinRTUtils {
public:
	static bool is_api_contract_present(const String &p_contract, uint16_t p_version);
	static bool is_type_present(const String &p_type);
	static HRESULT activation_factory(const String &p_class_name, REFIID p_iid, void **p_factory);

	static bool is_initialized();

	static void init();
	static bool create_queue(const String &p_appid);
	static void destroy_queue();
	static void cleanup();

	static bool try_show_onecore_emoji_picker();
	static Vector<String> get_preferred_locales();

	static WinRTWindowData *create_wd(HWND p_window, const Callable &p_color_cb, int64_t p_window_id);
	static bool window_has_display_info(const WinRTWindowData *p_data);
	static void window_get_advanced_color_info(const WinRTWindowData *p_data, bool &r_hdr_supported, float &r_min_luminance, float &r_max_luminance, float &r_max_average_luminance, float &r_sdr_white_level);
	static void destroy_wd(WinRTWindowData *p_data);

	static DisplayServerEnums::NotificationID send_toast_notification(const String &p_title, const String &p_text, const Ref<Texture2D> &p_image, const Callable &p_callback);
	static void hide_toast_notification(DisplayServerEnums::NotificationID p_id);
};
