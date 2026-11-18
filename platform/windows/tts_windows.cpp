//************************************************************************//
//  tts_windows.cpp                                                       //
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

#include "tts_windows.h"

#include "tts_driver_onecore.h"
#include "tts_driver_sapi.h"

TTS_Windows *TTS_Windows::singleton = nullptr;

TTS_Windows *TTS_Windows::get_singleton() {
	return singleton;
}

bool TTS_Windows::is_speaking() const {
	if (driver) {
		return driver->is_speaking();
	}
	return false;
}

bool TTS_Windows::is_paused() const {
	if (driver) {
		return driver->is_paused();
	}
	return false;
}

Array TTS_Windows::get_voices() const {
	if (driver) {
		return driver->get_voices();
	}
	return Array();
}

void TTS_Windows::speak(const String &p_text, const String &p_voice, int p_volume, float p_pitch, float p_rate, int64_t p_utterance_id, bool p_interrupt) {
	if (driver) {
		driver->speak(p_text, p_voice, p_volume, p_pitch, p_rate, p_utterance_id, p_interrupt);
	}
}

void TTS_Windows::pause() {
	if (driver) {
		driver->pause();
	}
}

void TTS_Windows::resume() {
	if (driver) {
		driver->resume();
	}
}

void TTS_Windows::stop() {
	if (driver) {
		driver->stop();
	}
}

void TTS_Windows::process_events() {
	if (driver) {
		driver->process_events();
	}
}

TTS_Windows::TTS_Windows() {
	// Try OneCore driver.
	if (!driver) {
		driver = memnew(TTSDriverOneCore);
		if (!driver->init()) {
			memdelete(driver);
			driver = nullptr;
		}
	}
	// Try SAPI driver.
	if (!driver) {
		driver = memnew(TTSDriverSAPI);
		if (!driver->init()) {
			memdelete(driver);
			driver = nullptr;
		}
	}
}

TTS_Windows::~TTS_Windows() {
	memdelete(driver);
}
