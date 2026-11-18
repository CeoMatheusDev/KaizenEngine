//************************************************************************//
//  game_menu_utils_jni.h                                                 //
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

#include <jni.h>

extern "C" {
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_setSuspend(JNIEnv *env, jclass clazz, jboolean enabled);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_nextFrame(JNIEnv *env, jclass clazz);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_setNodeType(JNIEnv *env, jclass clazz, jint type);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_setSelectMode(JNIEnv *env, jclass clazz, jint mode);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_setSelectionVisible(JNIEnv *env, jclass clazz, jboolean visible);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_setSelectionAvoidLocked(JNIEnv *env, jclass clazz, jboolean enabled);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_setSelectionPreferGroup(JNIEnv *env, jclass clazz, jboolean enabled);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_setCameraOverride(JNIEnv *env, jclass clazz, jboolean enabled);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_setCameraManipulateMode(JNIEnv *env, jclass clazz, jint mode);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_resetCamera2DPosition(JNIEnv *env, jclass clazz);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_resetCamera3DPosition(JNIEnv *env, jclass clazz);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_playMainScene(JNIEnv *env, jclass clazz);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_setDebugMuteAudio(JNIEnv *env, jclass clazz, jboolean enabled);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_resetTimeScale(JNIEnv *env, jclass clazz);
JNIEXPORT void JNICALL Java_org_kaizenengine_kaizen_editor_utils_GameMenuUtils_setTimeScale(JNIEnv *env, jclass clazz, jdouble scale);
}
