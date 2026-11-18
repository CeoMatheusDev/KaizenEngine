/**************************************************************************/
/*  library_kaizen_display.js                                              */
/**************************************************************************/
/*                         This file is part of:                          */
/*                             KAIZEN ENGINE                               */
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

const KaizenDisplayVK = {

	$KaizenDisplayVK__deps: ['$KaizenRuntime', '$KaizenConfig', '$KaizenEventListeners', '$KaizenInput'],
	$KaizenDisplayVK__postset: 'KaizenOS.atexit(function(resolve, reject) { KaizenDisplayVK.clear(); resolve(); });',
	$KaizenDisplayVK: {
		textinput: null,
		textarea: null,

		available: function () {
			return KaizenConfig.virtual_keyboard && 'ontouchstart' in window;
		},

		init: function (input_cb) {
			function create(what) {
				const elem = document.createElement(what);
				elem.style.display = 'none';
				elem.style.position = 'absolute';
				elem.style.zIndex = '-1';
				elem.style.background = 'transparent';
				elem.style.padding = '0px';
				elem.style.margin = '0px';
				elem.style.overflow = 'hidden';
				elem.style.width = '0px';
				elem.style.height = '0px';
				elem.style.border = '0px';
				elem.style.outline = 'none';
				elem.readonly = true;
				elem.disabled = true;
				KaizenEventListeners.add(elem, 'input', function (evt) {
					const c_str = KaizenRuntime.allocString(elem.value);
					input_cb(c_str, elem.selectionEnd);
					KaizenRuntime.free(c_str);
				}, false);
				if (what === 'input') {
					// Handling the "Enter" key.
					const onKey = (pEvent, pEventName) => {
						if (pEvent.key !== 'Enter') {
							return;
						}
						KaizenInput.onKeyEvent(pEventName === 'keydown', pEvent);
					};
					KaizenEventListeners.add(elem, 'keydown', (pEvent) => onKey(pEvent, 'keydown'), false);
					KaizenEventListeners.add(elem, 'keyup', (pEvent) => onKey(pEvent, 'keyup'), false);
				}
				KaizenEventListeners.add(elem, 'blur', function (evt) {
					elem.style.display = 'none';
					elem.readonly = true;
					elem.disabled = true;
				}, false);
				KaizenConfig.canvas.insertAdjacentElement('beforebegin', elem);
				return elem;
			}
			KaizenDisplayVK.textinput = create('input');
			KaizenDisplayVK.textarea = create('textarea');
			KaizenDisplayVK.updateSize();
		},
		show: function (text, type, start, end) {
			if (!KaizenDisplayVK.textinput || !KaizenDisplayVK.textarea) {
				return;
			}
			if (KaizenDisplayVK.textinput.style.display !== '' || KaizenDisplayVK.textarea.style.display !== '') {
				KaizenDisplayVK.hide();
			}
			KaizenDisplayVK.updateSize();

			let elem = KaizenDisplayVK.textinput;
			switch (type) {
			case 0: // DisplayServerEnums::KEYBOARD_TYPE_DEFAULT
				elem.type = 'text';
				elem.inputmode = '';
				break;
			case 1: // DisplayServerEnums::KEYBOARD_TYPE_MULTILINE
				elem = KaizenDisplayVK.textarea;
				break;
			case 2: // DisplayServerEnums::KEYBOARD_TYPE_NUMBER
				elem.type = 'text';
				elem.inputmode = 'numeric';
				break;
			case 3: // DisplayServerEnums::KEYBOARD_TYPE_NUMBER_DECIMAL
				elem.type = 'text';
				elem.inputmode = 'decimal';
				break;
			case 4: // DisplayServerEnums::KEYBOARD_TYPE_PHONE
				elem.type = 'tel';
				elem.inputmode = '';
				break;
			case 5: // DisplayServerEnums::KEYBOARD_TYPE_EMAIL_ADDRESS
				elem.type = 'email';
				elem.inputmode = '';
				break;
			case 6: // DisplayServerEnums::KEYBOARD_TYPE_PASSWORD
				elem.type = 'password';
				elem.inputmode = '';
				break;
			case 7: // DisplayServerEnums::KEYBOARD_TYPE_URL
				elem.type = 'url';
				elem.inputmode = '';
				break;
			default:
				elem.type = 'text';
				elem.inputmode = '';
				break;
			}

			elem.readonly = false;
			elem.disabled = false;
			elem.value = text;
			elem.style.display = 'block';
			elem.focus();
			elem.setSelectionRange(start, end);
		},
		hide: function () {
			if (!KaizenDisplayVK.textinput || !KaizenDisplayVK.textarea) {
				return;
			}
			[KaizenDisplayVK.textinput, KaizenDisplayVK.textarea].forEach(function (elem) {
				elem.blur();
				elem.style.display = 'none';
				elem.value = '';
			});
		},
		updateSize: function () {
			if (!KaizenDisplayVK.textinput || !KaizenDisplayVK.textarea) {
				return;
			}
			const rect = KaizenConfig.canvas.getBoundingClientRect();
			function update(elem) {
				elem.style.left = `${rect.left}px`;
				elem.style.top = `${rect.top}px`;
				elem.style.width = `${rect.width}px`;
				elem.style.height = `${rect.height}px`;
			}
			update(KaizenDisplayVK.textinput);
			update(KaizenDisplayVK.textarea);
		},
		clear: function () {
			if (KaizenDisplayVK.textinput) {
				KaizenDisplayVK.textinput.remove();
				KaizenDisplayVK.textinput = null;
			}
			if (KaizenDisplayVK.textarea) {
				KaizenDisplayVK.textarea.remove();
				KaizenDisplayVK.textarea = null;
			}
		},
	},
};
mergeInto(LibraryManager.library, KaizenDisplayVK);

/*
 * Display server cursor helper.
 * Keeps track of cursor status and custom shapes.
 */
const KaizenDisplayCursor = {
	$KaizenDisplayCursor__deps: ['$KaizenOS', '$KaizenConfig'],
	$KaizenDisplayCursor__postset: 'KaizenOS.atexit(function(resolve, reject) { KaizenDisplayCursor.clear(); resolve(); });',
	$KaizenDisplayCursor: {
		shape: 'default',
		visible: true,
		cursors: {},
		set_style: function (style) {
			KaizenConfig.canvas.style.cursor = style;
		},
		set_shape: function (shape) {
			KaizenDisplayCursor.shape = shape;
			let css = shape;
			if (shape in KaizenDisplayCursor.cursors) {
				const c = KaizenDisplayCursor.cursors[shape];
				css = `url("${c.url}") ${c.x} ${c.y}, default`;
			}
			if (KaizenDisplayCursor.visible) {
				KaizenDisplayCursor.set_style(css);
			}
		},
		clear: function () {
			KaizenDisplayCursor.set_style('');
			KaizenDisplayCursor.shape = 'default';
			KaizenDisplayCursor.visible = true;
			Object.keys(KaizenDisplayCursor.cursors).forEach(function (key) {
				URL.revokeObjectURL(KaizenDisplayCursor.cursors[key]);
				delete KaizenDisplayCursor.cursors[key];
			});
		},
		lockPointer: function () {
			const canvas = KaizenConfig.canvas;
			if (canvas.requestPointerLock) {
				canvas.requestPointerLock();
			}
		},
		releasePointer: function () {
			if (document.exitPointerLock) {
				document.exitPointerLock();
			}
		},
		isPointerLocked: function () {
			return document.pointerLockElement === KaizenConfig.canvas;
		},
	},
};
mergeInto(LibraryManager.library, KaizenDisplayCursor);

const KaizenDisplayScreen = {
	$KaizenDisplayScreen__deps: ['$KaizenConfig', '$KaizenOS', '$GL', 'emscripten_webgl_get_current_context'],
	$KaizenDisplayScreen: {
		desired_size: [0, 0],
		hidpi: true,
		getPixelRatio: function () {
			return KaizenDisplayScreen.hidpi ? window.devicePixelRatio || 1 : 1;
		},
		isFullscreen: function () {
			const elem = document.fullscreenElement || document.mozFullscreenElement
				|| document.webkitFullscreenElement || document.msFullscreenElement;
			if (elem) {
				return elem === KaizenConfig.canvas;
			}
			// But maybe knowing the element is not supported.
			return document.fullscreen || document.mozFullScreen
				|| document.webkitIsFullscreen;
		},
		hasFullscreen: function () {
			return document.fullscreenEnabled || document.mozFullScreenEnabled
				|| document.webkitFullscreenEnabled;
		},
		requestFullscreen: function () {
			if (!KaizenDisplayScreen.hasFullscreen()) {
				return 1;
			}
			const canvas = KaizenConfig.canvas;
			try {
				const promise = (canvas.requestFullscreen || canvas.msRequestFullscreen
					|| canvas.mozRequestFullScreen || canvas.mozRequestFullscreen
					|| canvas.webkitRequestFullscreen
				).call(canvas);
				// Some browsers (Safari) return undefined.
				// For the standard ones, we need to catch it.
				if (promise) {
					promise.catch(function () {
						// nothing to do.
					});
				}
			} catch (e) {
				return 1;
			}
			return 0;
		},
		exitFullscreen: function () {
			if (!KaizenDisplayScreen.isFullscreen()) {
				return 0;
			}
			try {
				const promise = document.exitFullscreen();
				if (promise) {
					promise.catch(function () {
						// nothing to do.
					});
				}
			} catch (e) {
				return 1;
			}
			return 0;
		},
		_updateGL: function () {
			const gl_context_handle = _emscripten_webgl_get_current_context();
			const gl = GL.getContext(gl_context_handle);
			if (gl) {
				GL.resizeOffscreenFramebuffer(gl);
			}
		},
		updateSize: function () {
			const isFullscreen = KaizenDisplayScreen.isFullscreen();
			const wantsFullWindow = KaizenConfig.canvas_resize_policy === 2;
			const noResize = KaizenConfig.canvas_resize_policy === 0;
			const dWidth = KaizenDisplayScreen.desired_size[0];
			const dHeight = KaizenDisplayScreen.desired_size[1];
			const canvas = KaizenConfig.canvas;
			let width = dWidth;
			let height = dHeight;
			if (noResize) {
				// Don't resize canvas, just update GL if needed.
				if (canvas.width !== width || canvas.height !== height) {
					KaizenDisplayScreen.desired_size = [canvas.width, canvas.height];
					KaizenDisplayScreen._updateGL();
					return 1;
				}
				return 0;
			}
			const scale = KaizenDisplayScreen.getPixelRatio();
			if (isFullscreen || wantsFullWindow) {
				// We need to match screen size.
				width = Math.floor(window.innerWidth * scale);
				height = Math.floor(window.innerHeight * scale);
			}
			const csw = `${Math.floor(width / scale)}px`;
			const csh = `${Math.floor(height / scale)}px`;
			if (canvas.style.width !== csw || canvas.style.height !== csh || canvas.width !== width || canvas.height !== height) {
				// Size doesn't match.
				// Resize canvas, set correct CSS pixel size, update GL.
				canvas.width = width;
				canvas.height = height;
				canvas.style.width = csw;
				canvas.style.height = csh;
				KaizenDisplayScreen._updateGL();
				return 1;
			}
			return 0;
		},
	},
};
mergeInto(LibraryManager.library, KaizenDisplayScreen);

/**
 * Display server interface.
 *
 * Exposes all the functions needed by DisplayServer implementation.
 */
const KaizenDisplay = {
	$KaizenDisplay__deps: ['$KaizenConfig', '$KaizenRuntime', '$KaizenDisplayCursor', '$KaizenEventListeners', '$KaizenDisplayScreen', '$KaizenDisplayVK'],
	$KaizenDisplay: {
		window_icon: '',
		getDPI: function () {
			// devicePixelRatio is given in dppx
			// https://drafts.csswg.org/css-values/#resolution
			// > due to the 1:96 fixed ratio of CSS *in* to CSS *px*, 1dppx is equivalent to 96dpi.
			const dpi = Math.round(window.devicePixelRatio * 96);
			return dpi >= 96 ? dpi : 96;
		},
	},

	kaizen_js_display_is_swap_ok_cancel__proxy: 'sync',
	kaizen_js_display_is_swap_ok_cancel__sig: 'i',
	kaizen_js_display_is_swap_ok_cancel: function () {
		const win = (['Windows', 'Win64', 'Win32', 'WinCE']);
		const plat = navigator.platform || '';
		if (win.indexOf(plat) !== -1) {
			return 1;
		}
		return 0;
	},

	kaizen_js_tts_is_speaking__proxy: 'sync',
	kaizen_js_tts_is_speaking__sig: 'i',
	kaizen_js_tts_is_speaking: function () {
		return window.speechSynthesis.speaking;
	},

	kaizen_js_tts_is_paused__proxy: 'sync',
	kaizen_js_tts_is_paused__sig: 'i',
	kaizen_js_tts_is_paused: function () {
		return window.speechSynthesis.paused;
	},

	kaizen_js_tts_get_voices__proxy: 'sync',
	kaizen_js_tts_get_voices__sig: 'vi',
	kaizen_js_tts_get_voices: function (p_callback) {
		const func = KaizenRuntime.get_func(p_callback);
		try {
			const arr = [];
			const voices = window.speechSynthesis.getVoices();
			for (let i = 0; i < voices.length; i++) {
				arr.push(`${voices[i].lang};${voices[i].name}`);
			}
			const c_ptr = KaizenRuntime.allocStringArray(arr);
			func(arr.length, c_ptr);
			KaizenRuntime.freeStringArray(c_ptr, arr.length);
		} catch (e) {
			// Fail graciously.
		}
	},

	kaizen_js_tts_speak__proxy: 'sync',
	kaizen_js_tts_speak__sig: 'viiiffii',
	kaizen_js_tts_speak: function (p_text, p_voice, p_volume, p_pitch, p_rate, p_utterance_id, p_callback) {
		const func = KaizenRuntime.get_func(p_callback);

		function listener_end(evt) {
			evt.currentTarget.cb(1 /* DisplayServerEnums::TTS_UTTERANCE_ENDED */, evt.currentTarget.id, 0);
		}

		function listener_start(evt) {
			evt.currentTarget.cb(0 /* DisplayServerEnums::TTS_UTTERANCE_STARTED */, evt.currentTarget.id, 0);
		}

		function listener_error(evt) {
			evt.currentTarget.cb(2 /* DisplayServerEnums::TTS_UTTERANCE_CANCELED */, evt.currentTarget.id, 0);
		}

		function listener_bound(evt) {
			evt.currentTarget.cb(3 /* DisplayServerEnums::TTS_UTTERANCE_BOUNDARY */, evt.currentTarget.id, evt.charIndex);
		}

		const utterance = new SpeechSynthesisUtterance(KaizenRuntime.parseString(p_text));
		utterance.rate = p_rate;
		utterance.pitch = p_pitch;
		utterance.volume = p_volume / 100.0;
		utterance.addEventListener('end', listener_end);
		utterance.addEventListener('start', listener_start);
		utterance.addEventListener('error', listener_error);
		utterance.addEventListener('boundary', listener_bound);
		utterance.id = p_utterance_id;
		utterance.cb = func;
		const voice = KaizenRuntime.parseString(p_voice);
		const voices = window.speechSynthesis.getVoices();
		for (let i = 0; i < voices.length; i++) {
			if (voices[i].name === voice) {
				utterance.voice = voices[i];
				break;
			}
		}
		window.speechSynthesis.resume();
		window.speechSynthesis.speak(utterance);
	},

	kaizen_js_tts_pause__proxy: 'sync',
	kaizen_js_tts_pause__sig: 'v',
	kaizen_js_tts_pause: function () {
		window.speechSynthesis.pause();
	},

	kaizen_js_tts_resume__proxy: 'sync',
	kaizen_js_tts_resume__sig: 'v',
	kaizen_js_tts_resume: function () {
		window.speechSynthesis.resume();
	},

	kaizen_js_tts_stop__proxy: 'sync',
	kaizen_js_tts_stop__sig: 'v',
	kaizen_js_tts_stop: function () {
		window.speechSynthesis.cancel();
		window.speechSynthesis.resume();
	},

	kaizen_js_display_alert__proxy: 'sync',
	kaizen_js_display_alert__sig: 'vi',
	kaizen_js_display_alert: function (p_text) {
		window.alert(KaizenRuntime.parseString(p_text)); // eslint-disable-line no-alert
	},

	kaizen_js_display_screen_dpi_get__proxy: 'sync',
	kaizen_js_display_screen_dpi_get__sig: 'i',
	kaizen_js_display_screen_dpi_get: function () {
		return KaizenDisplay.getDPI();
	},

	kaizen_js_display_pixel_ratio_get__proxy: 'sync',
	kaizen_js_display_pixel_ratio_get__sig: 'f',
	kaizen_js_display_pixel_ratio_get: function () {
		return KaizenDisplayScreen.getPixelRatio();
	},

	kaizen_js_display_fullscreen_request__proxy: 'sync',
	kaizen_js_display_fullscreen_request__sig: 'i',
	kaizen_js_display_fullscreen_request: function () {
		return KaizenDisplayScreen.requestFullscreen();
	},

	kaizen_js_display_fullscreen_exit__proxy: 'sync',
	kaizen_js_display_fullscreen_exit__sig: 'i',
	kaizen_js_display_fullscreen_exit: function () {
		return KaizenDisplayScreen.exitFullscreen();
	},

	kaizen_js_display_desired_size_set__proxy: 'sync',
	kaizen_js_display_desired_size_set__sig: 'vii',
	kaizen_js_display_desired_size_set: function (width, height) {
		KaizenDisplayScreen.desired_size = [width, height];
		KaizenDisplayScreen.updateSize();
	},

	kaizen_js_display_size_update__proxy: 'sync',
	kaizen_js_display_size_update__sig: 'i',
	kaizen_js_display_size_update: function () {
		const updated = KaizenDisplayScreen.updateSize();
		if (updated) {
			KaizenDisplayVK.updateSize();
		}
		return updated;
	},

	kaizen_js_display_screen_size_get__proxy: 'sync',
	kaizen_js_display_screen_size_get__sig: 'vii',
	kaizen_js_display_screen_size_get: function (width, height) {
		const scale = KaizenDisplayScreen.getPixelRatio();
		KaizenRuntime.setHeapValue(width, window.screen.width * scale, 'i32');
		KaizenRuntime.setHeapValue(height, window.screen.height * scale, 'i32');
	},

	kaizen_js_display_window_size_get__proxy: 'sync',
	kaizen_js_display_window_size_get__sig: 'vii',
	kaizen_js_display_window_size_get: function (p_width, p_height) {
		KaizenRuntime.setHeapValue(p_width, KaizenConfig.canvas.width, 'i32');
		KaizenRuntime.setHeapValue(p_height, KaizenConfig.canvas.height, 'i32');
	},

	kaizen_js_display_has_webgl__proxy: 'sync',
	kaizen_js_display_has_webgl__sig: 'ii',
	kaizen_js_display_has_webgl: function (p_version) {
		if (p_version !== 1 && p_version !== 2) {
			return false;
		}
		try {
			return !!document.createElement('canvas').getContext(p_version === 2 ? 'webgl2' : 'webgl');
		} catch (e) { /* Not available */ }
		return false;
	},

	/*
	 * Canvas
	 */
	kaizen_js_display_canvas_focus__proxy: 'sync',
	kaizen_js_display_canvas_focus__sig: 'v',
	kaizen_js_display_canvas_focus: function () {
		KaizenConfig.canvas.focus();
	},

	kaizen_js_display_canvas_is_focused__proxy: 'sync',
	kaizen_js_display_canvas_is_focused__sig: 'i',
	kaizen_js_display_canvas_is_focused: function () {
		return document.activeElement === KaizenConfig.canvas;
	},

	/*
	 * Touchscreen
	 */
	kaizen_js_display_touchscreen_is_available__proxy: 'sync',
	kaizen_js_display_touchscreen_is_available__sig: 'i',
	kaizen_js_display_touchscreen_is_available: function () {
		return 'ontouchstart' in window;
	},

	/*
	 * Clipboard
	 */
	kaizen_js_display_clipboard_set__proxy: 'sync',
	kaizen_js_display_clipboard_set__sig: 'ii',
	kaizen_js_display_clipboard_set: function (p_text) {
		const text = KaizenRuntime.parseString(p_text);
		if (!navigator.clipboard || !navigator.clipboard.writeText) {
			return 1;
		}
		navigator.clipboard.writeText(text).catch(function (e) {
			// Setting OS clipboard is only possible from an input callback.
			KaizenRuntime.error('Setting OS clipboard is only possible from an input callback for the Web platform. Exception:', e);
		});
		return 0;
	},

	kaizen_js_display_clipboard_get__proxy: 'sync',
	kaizen_js_display_clipboard_get__sig: 'ii',
	kaizen_js_display_clipboard_get: function (callback) {
		const func = KaizenRuntime.get_func(callback);
		try {
			navigator.clipboard.readText().then(function (result) {
				const ptr = KaizenRuntime.allocString(result);
				func(ptr);
				KaizenRuntime.free(ptr);
			}).catch(function (e) {
				// Fail graciously.
			});
		} catch (e) {
			// Fail graciously.
		}
	},

	/*
	 * Window
	 */
	kaizen_js_display_window_title_set__proxy: 'sync',
	kaizen_js_display_window_title_set__sig: 'vi',
	kaizen_js_display_window_title_set: function (p_data) {
		document.title = KaizenRuntime.parseString(p_data);
	},

	kaizen_js_display_window_icon_set__proxy: 'sync',
	kaizen_js_display_window_icon_set__sig: 'vii',
	kaizen_js_display_window_icon_set: function (p_ptr, p_len) {
		let link = document.getElementById('-gd-engine-icon');
		const old_icon = KaizenDisplay.window_icon;
		if (p_ptr) {
			if (link === null) {
				link = document.createElement('link');
				link.rel = 'icon';
				link.id = '-gd-engine-icon';
				document.head.appendChild(link);
			}
			const png = new Blob([KaizenRuntime.heapSlice(HEAPU8, p_ptr, p_len)], { type: 'image/png' });
			KaizenDisplay.window_icon = URL.createObjectURL(png);
			link.href = KaizenDisplay.window_icon;
		} else {
			if (link) {
				link.remove();
			}
			KaizenDisplay.window_icon = null;
		}
		if (old_icon) {
			URL.revokeObjectURL(old_icon);
		}
	},

	/*
	 * Cursor
	 */
	kaizen_js_display_cursor_set_visible__proxy: 'sync',
	kaizen_js_display_cursor_set_visible__sig: 'vi',
	kaizen_js_display_cursor_set_visible: function (p_visible) {
		const visible = p_visible !== 0;
		if (visible === KaizenDisplayCursor.visible) {
			return;
		}
		KaizenDisplayCursor.visible = visible;
		if (visible) {
			KaizenDisplayCursor.set_shape(KaizenDisplayCursor.shape);
		} else {
			KaizenDisplayCursor.set_style('none');
		}
	},

	kaizen_js_display_cursor_is_hidden__proxy: 'sync',
	kaizen_js_display_cursor_is_hidden__sig: 'i',
	kaizen_js_display_cursor_is_hidden: function () {
		return !KaizenDisplayCursor.visible;
	},

	kaizen_js_display_cursor_set_shape__proxy: 'sync',
	kaizen_js_display_cursor_set_shape__sig: 'vi',
	kaizen_js_display_cursor_set_shape: function (p_string) {
		KaizenDisplayCursor.set_shape(KaizenRuntime.parseString(p_string));
	},

	kaizen_js_display_cursor_set_custom_shape__proxy: 'sync',
	kaizen_js_display_cursor_set_custom_shape__sig: 'viiiii',
	kaizen_js_display_cursor_set_custom_shape: function (p_shape, p_ptr, p_len, p_hotspot_x, p_hotspot_y) {
		const shape = KaizenRuntime.parseString(p_shape);
		const old_shape = KaizenDisplayCursor.cursors[shape];
		if (p_len > 0) {
			const png = new Blob([KaizenRuntime.heapSlice(HEAPU8, p_ptr, p_len)], { type: 'image/png' });
			const url = URL.createObjectURL(png);
			KaizenDisplayCursor.cursors[shape] = {
				url: url,
				x: p_hotspot_x,
				y: p_hotspot_y,
			};
		} else {
			delete KaizenDisplayCursor.cursors[shape];
		}
		if (shape === KaizenDisplayCursor.shape) {
			KaizenDisplayCursor.set_shape(KaizenDisplayCursor.shape);
		}
		if (old_shape) {
			URL.revokeObjectURL(old_shape.url);
		}
	},

	kaizen_js_display_cursor_lock_set__proxy: 'sync',
	kaizen_js_display_cursor_lock_set__sig: 'vi',
	kaizen_js_display_cursor_lock_set: function (p_lock) {
		if (p_lock) {
			KaizenDisplayCursor.lockPointer();
		} else {
			KaizenDisplayCursor.releasePointer();
		}
	},

	kaizen_js_display_cursor_is_locked__proxy: 'sync',
	kaizen_js_display_cursor_is_locked__sig: 'i',
	kaizen_js_display_cursor_is_locked: function () {
		return KaizenDisplayCursor.isPointerLocked() ? 1 : 0;
	},

	/*
	 * Listeners
	 */
	kaizen_js_display_fullscreen_cb__proxy: 'sync',
	kaizen_js_display_fullscreen_cb__sig: 'vi',
	kaizen_js_display_fullscreen_cb: function (callback) {
		const canvas = KaizenConfig.canvas;
		const func = KaizenRuntime.get_func(callback);
		function change_cb(evt) {
			if (evt.target === canvas) {
				func(KaizenDisplayScreen.isFullscreen());
			}
		}
		KaizenEventListeners.add(document, 'fullscreenchange', change_cb, false);
		KaizenEventListeners.add(document, 'mozfullscreenchange', change_cb, false);
		KaizenEventListeners.add(document, 'webkitfullscreenchange', change_cb, false);
	},

	kaizen_js_display_window_blur_cb__proxy: 'sync',
	kaizen_js_display_window_blur_cb__sig: 'vi',
	kaizen_js_display_window_blur_cb: function (callback) {
		const func = KaizenRuntime.get_func(callback);
		KaizenEventListeners.add(window, 'blur', function () {
			func();
		}, false);
	},

	kaizen_js_display_notification_cb__proxy: 'sync',
	kaizen_js_display_notification_cb__sig: 'viiiii',
	kaizen_js_display_notification_cb: function (callback, p_enter, p_exit, p_in, p_out) {
		const canvas = KaizenConfig.canvas;
		const func = KaizenRuntime.get_func(callback);
		const notif = [p_enter, p_exit, p_in, p_out];
		['mouseover', 'mouseleave', 'focus', 'blur'].forEach(function (evt_name, idx) {
			KaizenEventListeners.add(canvas, evt_name, function () {
				func(notif[idx]);
			}, true);
		});
	},

	kaizen_js_display_setup_canvas__proxy: 'sync',
	kaizen_js_display_setup_canvas__sig: 'viiii',
	kaizen_js_display_setup_canvas: function (p_width, p_height, p_fullscreen, p_hidpi) {
		const canvas = KaizenConfig.canvas;
		KaizenEventListeners.add(canvas, 'contextmenu', function (ev) {
			ev.preventDefault();
		}, false);
		KaizenEventListeners.add(canvas, 'webglcontextlost', function (ev) {
			alert('WebGL context lost, please reload the page'); // eslint-disable-line no-alert
			ev.preventDefault();
		}, false);
		KaizenDisplayScreen.hidpi = !!p_hidpi;
		switch (KaizenConfig.canvas_resize_policy) {
		case 0: // None
			KaizenDisplayScreen.desired_size = [canvas.width, canvas.height];
			break;
		case 1: // Project
			KaizenDisplayScreen.desired_size = [p_width, p_height];
			break;
		default: // Full window
			// Ensure we display in the right place, the size will be handled by updateSize
			canvas.style.position = 'absolute';
			canvas.style.top = 0;
			canvas.style.left = 0;
			break;
		}
		KaizenDisplayScreen.updateSize();
		if (p_fullscreen) {
			KaizenDisplayScreen.requestFullscreen();
		}
	},

	/*
	 * Virtual Keyboard
	 */
	kaizen_js_display_vk_show__proxy: 'sync',
	kaizen_js_display_vk_show__sig: 'viiii',
	kaizen_js_display_vk_show: function (p_text, p_type, p_start, p_end) {
		const text = KaizenRuntime.parseString(p_text);
		const start = p_start > 0 ? p_start : 0;
		const end = p_end > 0 ? p_end : start;
		KaizenDisplayVK.show(text, p_type, start, end);
	},

	kaizen_js_display_vk_hide__proxy: 'sync',
	kaizen_js_display_vk_hide__sig: 'v',
	kaizen_js_display_vk_hide: function () {
		KaizenDisplayVK.hide();
	},

	kaizen_js_display_vk_available__proxy: 'sync',
	kaizen_js_display_vk_available__sig: 'i',
	kaizen_js_display_vk_available: function () {
		return KaizenDisplayVK.available();
	},

	kaizen_js_display_tts_available__proxy: 'sync',
	kaizen_js_display_tts_available__sig: 'i',
	kaizen_js_display_tts_available: function () {
		return 'speechSynthesis' in window;
	},

	kaizen_js_display_vk_cb__proxy: 'sync',
	kaizen_js_display_vk_cb__sig: 'vi',
	kaizen_js_display_vk_cb: function (p_input_cb) {
		const input_cb = KaizenRuntime.get_func(p_input_cb);
		if (KaizenDisplayVK.available()) {
			KaizenDisplayVK.init(input_cb);
		}
	},
};

autoAddDeps(KaizenDisplay, '$KaizenDisplay');
mergeInto(LibraryManager.library, KaizenDisplay);
