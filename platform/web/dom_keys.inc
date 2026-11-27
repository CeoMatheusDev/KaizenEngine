/**************************************************************************/
/*  dom_keys.inc                                                          */
/**************************************************************************/
/*                         This file is part of:                          */
/*                             KAIZEN ENGINE                              */
/*                        https://godotengine.org                         */
/**************************************************************************/
/* Copyright (c) 2014-present Kaizen Engine contributors (see AUTHORS.md). */
/* Copyright (c) 2007-2014 Juan Linietsky, Ariel Manzur.                  */
/*                                                                        */
/* Permission is hereby granted, free of charge, to any person obtaining  */
/* a copy of this software and associated documentation files (the        */
/* "Software"), to deal in the Software without restriction, including    */
/* without limitation the rights to use, copy, modify, merge, publish,    */
/* distribute, sublicense, and/or sell copies of the Software, and to     */
/* permit persons to whom the Software is furnished to do so, subject to  */
/* the following conditions:                                              */
/*                                                                        */
/* The above copyright notice and this permission notice shall be         */
/* included in all copies or substantial portions of the Software.        */
/*                                                                        */
/* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        */
/* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     */
/* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. */
/* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   */
/* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   */
/* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE      */
/* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 */
/**************************************************************************/

#include "core/os/keyboard.h"

// See https://w3c.github.io/uievents-code/#code-value-tables
Key dom_code2kaizen_scancode(EM_UTF8 const p_code[32], EM_UTF8 const p_key[32], bool p_physical) {
#define DOM2KAIZEN(p_str, p_kaizen_code) \
	if (memcmp((const void *)p_str, (void *)(p_physical ? p_code : p_key), strlen(p_str) + 1) == 0) { \
		return Key::p_kaizen_code; \
	}

	// Numpad section.
	DOM2KAIZEN("NumLock", NUMLOCK);
	DOM2KAIZEN("Numpad0", KP_0);
	DOM2KAIZEN("Numpad1", KP_1);
	DOM2KAIZEN("Numpad2", KP_2);
	DOM2KAIZEN("Numpad3", KP_3);
	DOM2KAIZEN("Numpad4", KP_4);
	DOM2KAIZEN("Numpad5", KP_5);
	DOM2KAIZEN("Numpad6", KP_6);
	DOM2KAIZEN("Numpad7", KP_7);
	DOM2KAIZEN("Numpad8", KP_8);
	DOM2KAIZEN("Numpad9", KP_9);
	DOM2KAIZEN("NumpadAdd", KP_ADD);
	DOM2KAIZEN("NumpadBackspace", BACKSPACE);
	DOM2KAIZEN("Clear", CLEAR); // NumLock on macOS.
	DOM2KAIZEN("NumpadClear", CLEAR);
	DOM2KAIZEN("NumpadClearEntry", CLEAR);
	//DOM2KAIZEN("NumpadComma", UNKNOWN);
	DOM2KAIZEN("NumpadDecimal", KP_PERIOD);
	DOM2KAIZEN("NumpadDivide", KP_DIVIDE);
	DOM2KAIZEN("NumpadEnter", KP_ENTER);
	DOM2KAIZEN("NumpadEqual", EQUAL);
	//DOM2KAIZEN("NumpadHash", UNKNOWN);
	//DOM2KAIZEN("NumpadMemoryAdd", UNKNOWN);
	//DOM2KAIZEN("NumpadMemoryClear", UNKNOWN);
	//DOM2KAIZEN("NumpadMemoryRecall", UNKNOWN);
	//DOM2KAIZEN("NumpadMemoryStore", UNKNOWN);
	//DOM2KAIZEN("NumpadMemorySubtract", UNKNOWN);
	DOM2KAIZEN("NumpadMultiply", KP_MULTIPLY);
	DOM2KAIZEN("NumpadParenLeft", PARENLEFT);
	DOM2KAIZEN("NumpadParenRight", PARENRIGHT);
	DOM2KAIZEN("NumpadStar", KP_MULTIPLY); // or ASTERISK ?
	DOM2KAIZEN("NumpadSubtract", KP_SUBTRACT);

	// Alphanumeric section.
	DOM2KAIZEN("Backquote", QUOTELEFT);
	DOM2KAIZEN("Backslash", BACKSLASH);
	DOM2KAIZEN("BracketLeft", BRACKETLEFT);
	DOM2KAIZEN("BracketRight", BRACKETRIGHT);
	DOM2KAIZEN("Comma", COMMA);
	DOM2KAIZEN("Digit0", KEY_0);
	DOM2KAIZEN("Digit1", KEY_1);
	DOM2KAIZEN("Digit2", KEY_2);
	DOM2KAIZEN("Digit3", KEY_3);
	DOM2KAIZEN("Digit4", KEY_4);
	DOM2KAIZEN("Digit5", KEY_5);
	DOM2KAIZEN("Digit6", KEY_6);
	DOM2KAIZEN("Digit7", KEY_7);
	DOM2KAIZEN("Digit8", KEY_8);
	DOM2KAIZEN("Digit9", KEY_9);
	DOM2KAIZEN("Equal", EQUAL);
	DOM2KAIZEN("IntlBackslash", BACKSLASH);
	//DOM2KAIZEN("IntlRo", UNKNOWN);
	DOM2KAIZEN("IntlYen", YEN);

	DOM2KAIZEN("KeyA", A);
	DOM2KAIZEN("KeyB", B);
	DOM2KAIZEN("KeyC", C);
	DOM2KAIZEN("KeyD", D);
	DOM2KAIZEN("KeyE", E);
	DOM2KAIZEN("KeyF", F);
	DOM2KAIZEN("KeyG", G);
	DOM2KAIZEN("KeyH", H);
	DOM2KAIZEN("KeyI", I);
	DOM2KAIZEN("KeyJ", J);
	DOM2KAIZEN("KeyK", K);
	DOM2KAIZEN("KeyL", L);
	DOM2KAIZEN("KeyM", M);
	DOM2KAIZEN("KeyN", N);
	DOM2KAIZEN("KeyO", O);
	DOM2KAIZEN("KeyP", P);
	DOM2KAIZEN("KeyQ", Q);
	DOM2KAIZEN("KeyR", R);
	DOM2KAIZEN("KeyS", S);
	DOM2KAIZEN("KeyT", T);
	DOM2KAIZEN("KeyU", U);
	DOM2KAIZEN("KeyV", V);
	DOM2KAIZEN("KeyW", W);
	DOM2KAIZEN("KeyX", X);
	DOM2KAIZEN("KeyY", Y);
	DOM2KAIZEN("KeyZ", Z);

	DOM2KAIZEN("Minus", MINUS);
	DOM2KAIZEN("Period", PERIOD);
	DOM2KAIZEN("Quote", APOSTROPHE);
	DOM2KAIZEN("Semicolon", SEMICOLON);
	DOM2KAIZEN("Slash", SLASH);

	// Functional keys in the Alphanumeric section.
	DOM2KAIZEN("Alt", ALT);
	DOM2KAIZEN("AltLeft", ALT);
	DOM2KAIZEN("AltRight", ALT);
	DOM2KAIZEN("Backspace", BACKSPACE);
	DOM2KAIZEN("CapsLock", CAPSLOCK);
	DOM2KAIZEN("ContextMenu", MENU);
	DOM2KAIZEN("Control", CTRL);
	DOM2KAIZEN("ControlLeft", CTRL);
	DOM2KAIZEN("ControlRight", CTRL);
	DOM2KAIZEN("Enter", ENTER);
	DOM2KAIZEN("Meta", META);
	DOM2KAIZEN("MetaLeft", META);
	DOM2KAIZEN("MetaRight", META);
	DOM2KAIZEN("OSLeft", META); // Command on macOS.
	DOM2KAIZEN("OSRight", META); // Command on macOS.
	DOM2KAIZEN("Shift", SHIFT);
	DOM2KAIZEN("ShiftLeft", SHIFT);
	DOM2KAIZEN("ShiftRight", SHIFT);
	DOM2KAIZEN("Space", SPACE);
	DOM2KAIZEN("Tab", TAB);

	// ControlPad section.
	DOM2KAIZEN("Delete", KEY_DELETE);
	DOM2KAIZEN("End", END);
	DOM2KAIZEN("Help", HELP);
	DOM2KAIZEN("Home", HOME);
	DOM2KAIZEN("Insert", INSERT);
	DOM2KAIZEN("PageDown", PAGEDOWN);
	DOM2KAIZEN("PageUp", PAGEUP);

	// ArrowPad section.
	DOM2KAIZEN("ArrowDown", DOWN);
	DOM2KAIZEN("ArrowLeft", LEFT);
	DOM2KAIZEN("ArrowRight", RIGHT);
	DOM2KAIZEN("ArrowUp", UP);

	// Function section.
	DOM2KAIZEN("Escape", ESCAPE);
	DOM2KAIZEN("F1", F1);
	DOM2KAIZEN("F2", F2);
	DOM2KAIZEN("F3", F3);
	DOM2KAIZEN("F4", F4);
	DOM2KAIZEN("F5", F5);
	DOM2KAIZEN("F6", F6);
	DOM2KAIZEN("F7", F7);
	DOM2KAIZEN("F8", F8);
	DOM2KAIZEN("F9", F9);
	DOM2KAIZEN("F10", F10);
	DOM2KAIZEN("F11", F11);
	DOM2KAIZEN("F12", F12);
	//DOM2KAIZEN("Fn", UNKNOWN); // never actually fired, but included in the standard draft.
	//DOM2KAIZEN("FnLock", UNKNOWN);
	DOM2KAIZEN("PrintScreen", PRINT);
	DOM2KAIZEN("ScrollLock", SCROLLLOCK);
	DOM2KAIZEN("Pause", PAUSE);

	// Media keys section.
	DOM2KAIZEN("BrowserBack", BACK);
	DOM2KAIZEN("BrowserFavorites", FAVORITES);
	DOM2KAIZEN("BrowserForward", FORWARD);
	DOM2KAIZEN("BrowserHome", OPENURL);
	DOM2KAIZEN("BrowserRefresh", REFRESH);
	DOM2KAIZEN("BrowserSearch", SEARCH);
	DOM2KAIZEN("BrowserStop", STOP);
	//DOM2KAIZEN("Eject", UNKNOWN);
	DOM2KAIZEN("LaunchApp1", LAUNCH0);
	DOM2KAIZEN("LaunchApp2", LAUNCH1);
	DOM2KAIZEN("LaunchMail", LAUNCHMAIL);
	DOM2KAIZEN("MediaPlayPause", MEDIAPLAY);
	DOM2KAIZEN("MediaSelect", LAUNCHMEDIA);
	DOM2KAIZEN("MediaStop", MEDIASTOP);
	DOM2KAIZEN("MediaTrackNext", MEDIANEXT);
	DOM2KAIZEN("MediaTrackPrevious", MEDIAPREVIOUS);
	//DOM2KAIZEN("Power", UNKNOWN);
	//DOM2KAIZEN("Sleep", UNKNOWN);
	DOM2KAIZEN("AudioVolumeDown", VOLUMEDOWN);
	DOM2KAIZEN("AudioVolumeMute", VOLUMEMUTE);
	DOM2KAIZEN("AudioVolumeUp", VOLUMEUP);
	//DOM2KAIZEN("WakeUp", UNKNOWN);

	// Printable ASCII.
	uint8_t b0 = (uint8_t)p_key[0];
	uint8_t b1 = (uint8_t)p_key[1];
	if (b0 >= 0x20 && b0 < 0x7F) { // ASCII.
		if (b0 > 0x60 && b0 < 0x7B) { // Lowercase ASCII.
			b0 -= 32;
		}
		return (Key)b0;
	} else if (b0 == 0xC2 && b1 == 0xA5) {
		return Key::YEN;
	} else if (b0 == 0xC2 && b1 == 0xA7) {
		return Key::SECTION;
	}

	return Key::NONE;
#undef DOM2KAIZEN
}

KeyLocation dom_code2kaizen_key_location(EM_UTF8 const p_code[32]) {
#define DOM2KAIZEN(m_str, m_kaizen_code) \
	if (memcmp((const void *)m_str, (void *)p_code, strlen(m_str) + 1) == 0) { \
		return KeyLocation::m_kaizen_code; \
	}

	DOM2KAIZEN("AltLeft", LEFT);
	DOM2KAIZEN("AltRight", RIGHT);
	DOM2KAIZEN("ControlLeft", LEFT);
	DOM2KAIZEN("ControlRight", RIGHT);
	DOM2KAIZEN("MetaLeft", LEFT);
	DOM2KAIZEN("MetaRight", RIGHT);
	DOM2KAIZEN("OSLeft", LEFT);
	DOM2KAIZEN("OSRight", RIGHT);
	DOM2KAIZEN("ShiftLeft", LEFT);
	DOM2KAIZEN("ShiftRight", RIGHT);

	return KeyLocation::UNSPECIFIED;
#undef DOM2KAIZEN
}
