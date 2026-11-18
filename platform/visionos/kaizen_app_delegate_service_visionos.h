//************************************************************************//
//  kaizen_app_delegate_service_visionos.h                                //
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

#import "render_mode_visionos.h"

#import "drivers/apple_embedded/kaizen_app_delegate_service_apple_embedded.h"

#import <CompositorServices/CompositorServices.h>

typedef NS_ENUM(NSInteger, GDTRenderMode) {
	GDTRenderModeWindowed = RenderModeVisionOS::WINDOWED,
	GDTRenderModeCompositorServices = RenderModeVisionOS::COMPOSITOR_SERVICES
};

typedef NS_ENUM(NSInteger, GDTImmersionStyle) {
	GDTImmersionStyleFull,
	GDTImmersionStyleMixed,
	GDTImmersionStyleProgressive
};

typedef NS_ENUM(NSInteger, GDTVisibility) {
	GDTVisibilityAutomatic,
	GDTVisibilityVisible,
	GDTVisibilityHidden
};

@interface GDTAppDelegateServiceVisionOS : GDTAppDelegateService

@property(class, nonatomic, assign) GDTRenderMode renderMode;
@property(class, nonatomic, weak, nullable) cp_layer_renderer_t layerRenderer;
@property(class, nonatomic, strong, nullable) cp_layer_renderer_capabilities_t layerRendererCapabilities;

@property(class, nonatomic, assign, readonly) BOOL isDynamicRenderQualityEnabled;
@property(class, nonatomic, assign, readonly) float maxRenderQuality;

@property(class, nonatomic, assign, readonly) GDTVisibility initialUpperLimbVisibility;
@property(class, nonatomic, assign, readonly) GDTVisibility initialPersistentSystemOverlays;

@property(class, nonatomic, assign) GDTImmersionStyle immersionStyle;

@property(class, nonatomic, assign) GDTVisibility upperLimbVisibility;

@property(class, nonatomic, assign) GDTVisibility persistentSystemOverlays;

@end
