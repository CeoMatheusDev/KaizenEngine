//************************************************************************//
//  ngx_backend.cpp                                                       //
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

#include "ngx_backend.h"

#include "core/os/os.h"

using namespace RendererRD;

NGXBackend::NGXBackend() {
	config.enabled = false;
	config.dlss_sr = true;
	config.dlss_rr = false;
	config.dlss_fg = false;
	config.feature_mask = 0;
	supported = false;
}

NGXBackend::~NGXBackend() {
	shutdown();
}

bool NGXBackend::init() {
	RenderingDevice *rd = RD::get_singleton();
	if (!rd) {
		return false;
	}

	String vendor = rd->get_device_vendor_name();
	if (vendor != "NVIDIA") {
		return false;
	}

#ifdef NGX_ENABLED
	Error err = OS::get_singleton()->open_dynamic_library("libnvidia-ngx-dlss.so", ngx_handle);
	if (err != OK || !ngx_handle) {
		WARN_PRINT("NGXBackend: Failed to load the NVIDIA NGX library.");
		return false;
	}

	supported = true;
	return true;
#else
	return false;
#endif
}

void NGXBackend::shutdown() {
#ifdef NGX_ENABLED
	if (ngx_handle) {
		OS::get_singleton()->close_dynamic_library(ngx_handle);
		ngx_handle = nullptr;
	}
#endif
	supported = false;
}
