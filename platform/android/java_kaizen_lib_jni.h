//************************************************************************//
//  java_kaizen_lib_jni.h                                                 //
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

#include <android/log.h>
#include <jni.h>

// These functions can be called from within JAVA and are the means by which our JAVA implementation calls back into our C++ code.
// See java/src/org/kaizenengine/kaizen/KaizenLib.java for the JAVA side of this (yes that's why we have the long names)
extern "C" {
JNIEXPORT jboolean JNICALL Java_org_kaizenengine_kaizen_KaizenLib_initialize(JNIEnv *env, jclass clazz, jobject p_kaizen_native_bridge, jobject p_asset_manager, jobject p_kaizen_io, jobject p_net_utils, jobject p_directory_access_handler, jobject p_file_access_handler, jboolean p_use_apk_expansion);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_ondestroy(JNIEnv *env, jclass clazz);
JNIEXPORT jboolean JNICALL Java_org_kaizenengine_kaizen_KaizenLib_setup(JNIEnv *env, jclass clazz, jobjectArray p_cmdline, jobject p_kaizen_tts);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_resize(JNIEnv *env, jclass clazz, jobject p_surface, jint p_width, jint p_height);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_newcontext(JNIEnv *env, jclass clazz, jobject p_surface);
JNIEXPORT jboolean JNICALL Java_org_kaizenengine_kaizen_KaizenLib_step(JNIEnv *env, jclass clazz);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_ttsCallback(JNIEnv *env, jclass clazz, jint event, jlong id, jint pos);
JNIEXPORT jboolean JNICALL Java_org_kaizenengine_kaizen_KaizenLib_createAccessKitAdapter(JNIEnv *env, jclass clazz, jlong p_window_id);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_freeAccessKitAdapter(JNIEnv *env, jclass clazz, jlong p_window_id);
JNIEXPORT jobject JNICALL Java_org_kaizenengine_kaizen_KaizenLib_createAccessibilityNodeInfo(JNIEnv *env, jclass clazz, jlong p_window_id, jobject p_host, jint p_virtual_view_id);
JNIEXPORT jobject JNICALL Java_org_kaizenengine_kaizen_KaizenLib_findAccessibilityFocus(JNIEnv *env, jclass clazz, jlong p_window_id, jobject p_host, jint p_focus_type);
JNIEXPORT jboolean JNICALL Java_org_kaizenengine_kaizen_KaizenLib_performAccessibilityAction(JNIEnv *env, jclass clazz, jlong p_window_id, jobject p_host, jint p_virtual_view_id, jint p_action, jobject p_arguments);
JNIEXPORT jboolean JNICALL Java_org_kaizenengine_kaizen_KaizenLib_onAccessibilityHoverEvent(JNIEnv *env, jclass clazz, jlong p_window_id, jobject p_host, jint p_action, jfloat p_x, jfloat p_y);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_back(JNIEnv *env, jclass clazz);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_dispatchMouseEvent(JNIEnv *env, jclass clazz, jint p_event_type, jint p_button_mask, jfloat p_x, jfloat p_y, jfloat p_delta_x, jfloat p_delta_y, jboolean p_double_click, jboolean p_source_mouse_relative, jfloat p_pressure, jfloat p_tilt_x, jfloat p_tilt_y, jboolean p_emulated);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_dispatchTouchEvent(JNIEnv *env, jclass clazz, jint ev, jint pointer, jint pointer_count, jfloatArray positions, jboolean p_double_tap, jboolean p_long_press);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_magnify(JNIEnv *env, jclass clazz, jfloat p_x, jfloat p_y, jfloat p_factor);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_pan(JNIEnv *env, jclass clazz, jfloat p_x, jfloat p_y, jfloat p_delta_x, jfloat p_delta_y);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_key(JNIEnv *env, jclass clazz, jint p_physical_keycode, jint p_unicode, jint p_key_label, jboolean p_pressed, jboolean p_echo);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_joybutton(JNIEnv *env, jclass clazz, jint p_device, jint p_button, jboolean p_pressed);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_joyaxis(JNIEnv *env, jclass clazz, jint p_device, jint p_axis, jfloat p_value);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_joyhat(JNIEnv *env, jclass clazz, jint p_device, jint p_hat_x, jint p_hat_y);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_joyconnectionchanged(JNIEnv *env, jclass clazz, jint p_device, jboolean p_connected, jstring p_name);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_accelerometer(JNIEnv *env, jclass clazz, jfloat x, jfloat y, jfloat z);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_gravity(JNIEnv *env, jclass clazz, jfloat x, jfloat y, jfloat z);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_magnetometer(JNIEnv *env, jclass clazz, jfloat x, jfloat y, jfloat z);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_gyroscope(JNIEnv *env, jclass clazz, jfloat x, jfloat y, jfloat z);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_deviceOrientation(JNIEnv *env, jclass clazz, jfloat x, jfloat y, jfloat z, jfloat w);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_focusin(JNIEnv *env, jclass clazz);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_focusout(JNIEnv *env, jclass clazz);
JNIEXPORT jstring JNICALL Java_org_kaizenengine_kaizen_KaizenLib_getGlobal(JNIEnv *env, jclass clazz, jstring path);
JNIEXPORT jobjectArray JNICALL Java_org_kaizenengine_kaizen_KaizenLib_getRendererInfo(JNIEnv *env, jclass clazz, jboolean p_vulkan_requirements_met);
JNIEXPORT jstring JNICALL Java_org_kaizenengine_kaizen_KaizenLib_getEditorSetting(JNIEnv *env, jclass clazz, jstring p_setting_key);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_setEditorSetting(JNIEnv *env, jclass clazz, jstring p_key, jobject p_data);
JNIEXPORT jobject JNICALL Java_org_kaizenengine_kaizen_KaizenLib_getEditorProjectMetadata(JNIEnv *env, jclass clazz, jstring p_section, jstring p_key, jobject p_default_value);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_setEditorProjectMetadata(JNIEnv *env, jclass clazz, jstring p_section, jstring p_key, jobject p_data);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_setVirtualKeyboardHeight(JNIEnv *env, jclass clazz, jint p_height);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_requestPermissionResult(JNIEnv *env, jclass clazz, jstring p_permission, jboolean p_result);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_onNightModeChanged(JNIEnv *env, jclass clazz);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_hardwareKeyboardConnected(JNIEnv *env, jclass clazz, jboolean p_connected);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_filePickerCallback(JNIEnv *env, jclass clazz, jboolean p_ok, jobjectArray p_selected_paths);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_onRendererResumed(JNIEnv *env, jclass clazz);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_onRendererPaused(JNIEnv *env, jclass clazz);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_onOrientationChange(JNIEnv *env, jclass clazz, jint p_orientation);
JNIEXPORT jboolean JNICALL Java_org_kaizenengine_kaizen_KaizenLib_shouldDispatchInputToRenderThread(JNIEnv *env, jclass clazz);
JNIEXPORT jstring JNICALL Java_org_kaizenengine_kaizen_KaizenLib_getProjectResourceDir(JNIEnv *env, jclass clazz);
JNIEXPORT jboolean JNICALL Java_org_kaizenengine_kaizen_KaizenLib_isEditorHint(JNIEnv *env, jclass clazz);
JNIEXPORT jboolean JNICALL Java_org_kaizenengine_kaizen_KaizenLib_isProjectManagerHint(JNIEnv *env, jclass clazz);
JNIEXPORT jboolean JNICALL Java_org_kaizenengine_kaizen_KaizenLib_hasFeature(JNIEnv *env, jclass clazz, jstring p_feature);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_KaizenLib_onPictureInPictureModeChanged(JNIEnv *env, jclass clazz, jboolean p_is_in_picture_in_picture_mode);
}
