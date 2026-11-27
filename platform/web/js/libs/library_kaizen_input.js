/**************************************************************************/
/*  library_kaizen_input.js                                               */
/**************************************************************************/
/*                         This file is part of:                          */
/*                             KAIZEN ENGINE                              */
/*                        https://godotengine.org                         */
/**************************************************************************/
/* Copyright (c) 2014-present Godot Engine contributors (see AUTHORS.md). */
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

/*
 * IME API helper.
 */

const KaizenIME = {
	$KaizenIME__deps: ['$KaizenRuntime', '$KaizenEventListeners'],
	$KaizenIME__postset: 'KaizenOS.atexit(function(resolve, reject) { KaizenIME.clear(); resolve(); });',
	$KaizenIME: {
		ime: null,
		active: false,
		focusTimerIntervalId: -1,

		getModifiers: function (evt) {
			return (evt.shiftKey + 0) + ((evt.altKey + 0) << 1) + ((evt.ctrlKey + 0) << 2) + ((evt.metaKey + 0) << 3);
		},

		ime_active: function (active) {
			function clearFocusTimerInterval() {
				clearInterval(KaizenIME.focusTimerIntervalId);
				KaizenIME.focusTimerIntervalId = -1;
			}

			function focusTimer() {
				if (KaizenIME.ime == null) {
					clearFocusTimerInterval();
					return;
				}
				KaizenIME.ime.focus();
			}

			if (KaizenIME.focusTimerIntervalId > -1) {
				clearFocusTimerInterval();
			}

			if (KaizenIME.ime == null) {
				return;
			}

			KaizenIME.active = active;
			if (active) {
				KaizenIME.ime.style.display = 'block';
				KaizenIME.focusTimerIntervalId = setInterval(focusTimer, 100);
			} else {
				KaizenIME.ime.style.display = 'none';
				KaizenConfig.canvas.focus();
			}
		},

		ime_position: function (x, y) {
			if (KaizenIME.ime == null) {
				return;
			}
			const canvas = KaizenConfig.canvas;
			const rect = canvas.getBoundingClientRect();
			const rw = canvas.width / rect.width;
			const rh = canvas.height / rect.height;
			const clx = (x / rw) + rect.x;
			const cly = (y / rh) + rect.y;

			KaizenIME.ime.style.left = `${clx}px`;
			KaizenIME.ime.style.top = `${cly}px`;
		},

		init: function (ime_cb, key_cb, code, key) {
			function key_event_cb(pressed, evt) {
				const modifiers = KaizenIME.getModifiers(evt);
				KaizenRuntime.stringToHeap(evt.code, code, 32);
				KaizenRuntime.stringToHeap(evt.key, key, 32);
				key_cb(pressed, evt.repeat, modifiers);
				evt.preventDefault();
			}
			function ime_event_cb(event) {
				if (KaizenIME.ime == null) {
					return;
				}
				switch (event.type) {
				case 'compositionstart':
					ime_cb(0, null);
					KaizenIME.ime.innerHTML = '';
					break;
				case 'compositionupdate': {
					const ptr = KaizenRuntime.allocString(event.data);
					ime_cb(1, ptr);
					KaizenRuntime.free(ptr);
				} break;
				case 'compositionend': {
					const ptr = KaizenRuntime.allocString(event.data);
					ime_cb(2, ptr);
					KaizenRuntime.free(ptr);
					KaizenIME.ime.innerHTML = '';
				} break;
				default:
					// Do nothing.
				}
			}

			const ime = document.createElement('div');
			ime.className = 'ime';
			ime.style.background = 'none';
			ime.style.opacity = 0.0;
			ime.style.position = 'fixed';
			ime.style.textAlign = 'left';
			ime.style.fontSize = '1px';
			ime.style.left = '0px';
			ime.style.top = '0px';
			ime.style.width = '100%';
			ime.style.height = '40px';
			ime.style.pointerEvents = 'none';
			ime.style.display = 'none';
			ime.contentEditable = 'true';

			KaizenEventListeners.add(ime, 'compositionstart', ime_event_cb, false);
			KaizenEventListeners.add(ime, 'compositionupdate', ime_event_cb, false);
			KaizenEventListeners.add(ime, 'compositionend', ime_event_cb, false);
			KaizenEventListeners.add(ime, 'keydown', key_event_cb.bind(null, 1), false);
			KaizenEventListeners.add(ime, 'keyup', key_event_cb.bind(null, 0), false);

			ime.onblur = function () {
				this.style.display = 'none';
				KaizenConfig.canvas.focus();
				KaizenIME.active = false;
			};

			KaizenConfig.canvas.parentElement.appendChild(ime);
			KaizenIME.ime = ime;
		},

		clear: function () {
			if (KaizenIME.ime == null) {
				return;
			}
			if (KaizenIME.focusTimerIntervalId > -1) {
				clearInterval(KaizenIME.focusTimerIntervalId);
				KaizenIME.focusTimerIntervalId = -1;
			}
			KaizenIME.ime.remove();
			KaizenIME.ime = null;
		},
	},
};
mergeInto(LibraryManager.library, KaizenIME);

/*
 * Gamepad API helper.
 */
const KaizenInputGamepads = {
	$KaizenInputGamepads__deps: ['$KaizenRuntime', '$KaizenEventListeners'],
	$KaizenInputGamepads: {
		samples: [],

		get_pads: function () {
			try {
				// Will throw in iframe when permission is denied.
				// Will throw/warn in the future for insecure contexts.
				// See https://github.com/w3c/gamepad/pull/120
				const pads = navigator.getGamepads();
				if (pads) {
					return pads;
				}
				return [];
			} catch (e) {
				return [];
			}
		},

		get_samples: function () {
			return KaizenInputGamepads.samples;
		},

		get_sample: function (index) {
			const samples = KaizenInputGamepads.samples;
			return index < samples.length ? samples[index] : null;
		},

		sample: function () {
			const pads = KaizenInputGamepads.get_pads();
			const samples = [];
			for (let i = 0; i < pads.length; i++) {
				const pad = pads[i];
				if (!pad) {
					samples.push(null);
					continue;
				}
				const s = {
					standard: pad.mapping === 'standard',
					buttons: [],
					axes: [],
					connected: pad.connected,
				};
				for (let b = 0; b < pad.buttons.length; b++) {
					s.buttons.push(pad.buttons[b].value);
				}
				for (let a = 0; a < pad.axes.length; a++) {
					s.axes.push(pad.axes[a]);
				}
				samples.push(s);
			}
			KaizenInputGamepads.samples = samples;
		},

		init: function (onchange) {
			KaizenInputGamepads.samples = [];
			function add(pad) {
				const guid = KaizenInputGamepads.get_guid(pad);
				const c_id = KaizenRuntime.allocString(pad.id);
				const c_guid = KaizenRuntime.allocString(guid);
				onchange(pad.index, 1, c_id, c_guid);
				KaizenRuntime.free(c_id);
				KaizenRuntime.free(c_guid);
			}
			const pads = KaizenInputGamepads.get_pads();
			for (let i = 0; i < pads.length; i++) {
				// Might be reserved space.
				if (pads[i]) {
					add(pads[i]);
				}
			}
			KaizenEventListeners.add(window, 'gamepadconnected', function (evt) {
				if (evt.gamepad) {
					add(evt.gamepad);
				}
			}, false);
			KaizenEventListeners.add(window, 'gamepaddisconnected', function (evt) {
				if (evt.gamepad) {
					onchange(evt.gamepad.index, 0);
				}
			}, false);
		},

		get_guid: function (pad) {
			if (pad.mapping) {
				return pad.mapping;
			}
			const ua = navigator.userAgent;
			let os = 'Unknown';
			if (ua.indexOf('Android') >= 0) {
				os = 'Android';
			} else if (ua.indexOf('Linux') >= 0) {
				os = 'Linux';
			} else if (ua.indexOf('iPhone') >= 0) {
				os = 'iOS';
			} else if (ua.indexOf('Macintosh') >= 0) {
				// Updated iPads will fall into this category.
				os = 'MacOSX';
			} else if (ua.indexOf('Windows') >= 0) {
				os = 'Windows';
			}

			const id = pad.id;
			// Chrom* style: NAME (Vendor: xxxx Product: xxxx).
			const exp1 = /vendor: ([0-9a-f]{4}) product: ([0-9a-f]{4})/i;
			// Firefox/Safari style (Safari may remove leading zeroes).
			const exp2 = /^([0-9a-f]+)-([0-9a-f]+)-/i;
			let vendor = '';
			let product = '';
			if (exp1.test(id)) {
				const match = exp1.exec(id);
				vendor = match[1].padStart(4, '0');
				product = match[2].padStart(4, '0');
			} else if (exp2.test(id)) {
				const match = exp2.exec(id);
				vendor = match[1].padStart(4, '0');
				product = match[2].padStart(4, '0');
			}
			if (!vendor || !product) {
				return `${os}Unknown`;
			}
			return os + vendor + product;
		},
	},
};
mergeInto(LibraryManager.library, KaizenInputGamepads);

/*
 * Drag and drop helper.
 * This is pretty big, but basically detect dropped files on KaizenConfig.canvas,
 * process them one by one (recursively for directories), and copies them to
 * the temporary FS path '/tmp/drop-[random]/' so it can be emitted as a kaizen
 * event (that requires a string array of paths).
 *
 * NOTE: The temporary files are removed after the callback. This means that
 * deferred callbacks won't be able to access the files.
 */
const KaizenInputDragDrop = {
	$KaizenInputDragDrop__deps: ['$FS', '$KaizenFS'],
	$KaizenInputDragDrop: {
		MAX_DROP_SIZE: 200 * 1024 * 1024,
		create_drop: function () {
			const drop = {};
			drop.pending = [];
			drop.dirs = [];
			drop.files = [];
			drop.size = 0;
			drop.add_entry = function (entry, path) {
				if (entry.isDirectory) {
					drop.add_dir(entry, path);
				} else if (entry.isFile) {
					drop.add_file(entry, path);
				} else {
					KaizenRuntime.error('Unrecognized entry...', entry);
				}
			};
			drop.add_dir = function (entry, path) {
				const next = `${path}/${entry.name}`;
				drop.pending.push(() => new Promise(function (resolve, reject) {
					const reader = entry.createReader();
					reader.readEntries(function (entries) {
						drop.dirs.push(next);
						for (let i = 0; i < entries.length; i++) {
							drop.add_entry(entries[i], next);
						}
						resolve();
					});
				}));
			};
			drop.add_file = function (entry, path) {
				drop.pending.push(() => new Promise(function (resolve, reject) {
					entry.file(function (file) {
						drop.size += file.size ?? 0;
						if (drop.size > KaizenInputDragDrop.MAX_DROP_SIZE) {
							reject(new Error('Drop size limit reached.'));
							return;
						}
						const reader = new FileReader();
						reader.onload = function () {
							const f = {
								'path': `${path}/${file.name}`,
								'name': file.name,
								'type': file.type,
								'size': file.size,
								'data': reader.result,
							};
							drop.files.push(f);
							resolve();
						};
						reader.onerror = function (err) {
							reject(err);
						};
						reader.readAsArrayBuffer(file);
					}, function (err) {
						reject(err);
					});
				}));
			};
			return drop;
		},

		process: function (drop) {
			if (drop.pending.length === 0) {
				return Promise.resolve();
			}
			// Execute pending operations.
			return drop.pending.splice(0).reduce(function (a, b) {
				return a.then(b);
			}, Promise.resolve()).then(function (result) {
				// Will check if new operations have been queued or resolve.
				return KaizenInputDragDrop.process(drop);
			});
		},

		_process_event: function (ev, callback) {
			ev.preventDefault();
			const ROOT = `/tmp/drop-${parseInt(Math.random() * (1 << 30), 10)}`;
			const drop = KaizenInputDragDrop.create_drop();
			if (ev.dataTransfer.items) {
				// Use DataTransferItemList interface to access the file(s)
				for (let i = 0; i < ev.dataTransfer.items.length; i++) {
					const item = ev.dataTransfer.items[i];
					let entry = null;
					if ('getAsEntry' in item) {
						entry = item.getAsEntry();
					} else if ('webkitGetAsEntry' in item) {
						entry = item.webkitGetAsEntry();
					}
					if (entry) {
						drop.add_entry(entry, ROOT);
					}
				}
			} else {
				KaizenRuntime.error('File upload not supported');
			}
			KaizenInputDragDrop.process(drop).then(function () {
				const out = [ROOT];
				drop.dirs.forEach((path) => {
					FS.mkdirTree(path);
					// Add dirs to the string array if in the drop root.
					const idx = path.indexOf('/', ROOT.length + 1);
					if (idx === -1) {
						out.push(path);
					}
				});
				drop.files.forEach((elem) => {
					const path = elem['path'];
					KaizenFS.copy_to_fs(path, elem['data']);
					// Add file to the string array if in the drop root.
					const idx = path.indexOf('/', ROOT.length + 1);
					if (idx === -1) {
						out.push(path);
					}
				});
				callback(out);
			}).catch(function (err) {
				KaizenRuntime.error(err);
			});
		},

		handler: function (callback) {
			return function (ev) {
				KaizenInputDragDrop._process_event(ev, callback);
			};
		},
	},
};
mergeInto(LibraryManager.library, KaizenInputDragDrop);

/*
 * Kaizen exposed input functions.
 */
const KaizenInput = {
	$KaizenInput__deps: ['$KaizenRuntime', '$KaizenConfig', '$KaizenEventListeners', '$KaizenInputGamepads', '$KaizenInputDragDrop', '$KaizenIME'],
	$KaizenInput: {
		inputKeyCallback: null,
		setInputKeyData: null,

		getModifiers: function (evt) {
			return (evt.shiftKey + 0) + ((evt.altKey + 0) << 1) + ((evt.ctrlKey + 0) << 2) + ((evt.metaKey + 0) << 3);
		},

		computePosition: function (evt, rect) {
			const canvas = KaizenConfig.canvas;
			const rw = canvas.width / rect.width;
			const rh = canvas.height / rect.height;
			const x = (evt.clientX - rect.x) * rw;
			const y = (evt.clientY - rect.y) * rh;
			return [x, y];
		},

		onKeyEvent: function (pIsPressed, pEvent) {
			if (KaizenInput.inputKeyCallback == null) {
				throw new TypeError('KaizenInput.onKeyEvent(): KaizenInput.inputKeyCallback is null, cannot process key event.');
			}
			if (KaizenInput.setInputKeyData == null) {
				throw new TypeError('KaizenInput.onKeyEvent(): KaizenInput.setInputKeyData is null, cannot process key event.');
			}

			const modifiers = KaizenInput.getModifiers(pEvent);
			KaizenInput.setInputKeyData(pEvent.code, pEvent.key);
			KaizenInput.inputKeyCallback(pIsPressed ? 1 : 0, pEvent.repeat, modifiers);
			pEvent.preventDefault();
		},
	},

	/*
	 * Mouse API
	 */
	kaizen_js_input_mouse_move_cb__proxy: 'sync',
	kaizen_js_input_mouse_move_cb__sig: 'vi',
	kaizen_js_input_mouse_move_cb: function (callback) {
		const func = KaizenRuntime.get_func(callback);
		const canvas = KaizenConfig.canvas;
		function move_cb(evt) {
			const rect = canvas.getBoundingClientRect();
			const pos = KaizenInput.computePosition(evt, rect);
			// Scale movement
			const rw = canvas.width / rect.width;
			const rh = canvas.height / rect.height;
			const rel_pos_x = evt.movementX * rw;
			const rel_pos_y = evt.movementY * rh;
			const modifiers = KaizenInput.getModifiers(evt);
			func(pos[0], pos[1], rel_pos_x, rel_pos_y, modifiers, evt.pressure);
		}
		KaizenEventListeners.add(window, 'pointermove', move_cb, false);
	},

	kaizen_js_input_mouse_wheel_cb__proxy: 'sync',
	kaizen_js_input_mouse_wheel_cb__sig: 'vi',
	kaizen_js_input_mouse_wheel_cb: function (callback) {
		const func = KaizenRuntime.get_func(callback);
		function wheel_cb(evt) {
			if (func(evt.deltaMode, evt.deltaX ?? 0, evt.deltaY ?? 0)) {
				evt.preventDefault();
			}
		}
		KaizenEventListeners.add(KaizenConfig.canvas, 'wheel', wheel_cb, false);
	},

	kaizen_js_input_mouse_button_cb__proxy: 'sync',
	kaizen_js_input_mouse_button_cb__sig: 'vi',
	kaizen_js_input_mouse_button_cb: function (callback) {
		const func = KaizenRuntime.get_func(callback);
		const canvas = KaizenConfig.canvas;
		function button_cb(p_pressed, evt) {
			const rect = canvas.getBoundingClientRect();
			const pos = KaizenInput.computePosition(evt, rect);
			const modifiers = KaizenInput.getModifiers(evt);
			// Since the event is consumed, focus manually.
			// NOTE: The iframe container may not have focus yet, so focus even when already active.
			if (p_pressed) {
				KaizenConfig.canvas.focus();
			}
			if (func(p_pressed, evt.button, pos[0], pos[1], modifiers)) {
				evt.preventDefault();
			}
		}
		KaizenEventListeners.add(canvas, 'mousedown', button_cb.bind(null, 1), false);
		KaizenEventListeners.add(window, 'mouseup', button_cb.bind(null, 0), false);
	},

	/*
	 * Touch API
	 */
	kaizen_js_input_touch_cb__proxy: 'sync',
	kaizen_js_input_touch_cb__sig: 'viii',
	kaizen_js_input_touch_cb: function (callback, ids, coords) {
		const func = KaizenRuntime.get_func(callback);
		const canvas = KaizenConfig.canvas;
		function touch_cb(type, evt) {
			// Since the event is consumed, focus manually.
			// NOTE: The iframe container may not have focus yet, so focus even when already active.
			if (type === 0) {
				KaizenConfig.canvas.focus();
			}
			const rect = canvas.getBoundingClientRect();
			const touches = evt.changedTouches;
			for (let i = 0; i < touches.length; i++) {
				const touch = touches[i];
				const pos = KaizenInput.computePosition(touch, rect);
				KaizenRuntime.setHeapValue(coords + (i * 2) * 8, pos[0], 'double');
				KaizenRuntime.setHeapValue(coords + (i * 2 + 1) * 8, pos[1], 'double');
				KaizenRuntime.setHeapValue(ids + i * 4, touch.identifier, 'i32');
			}
			func(type, touches.length);
			if (evt.cancelable) {
				evt.preventDefault();
			}
		}
		KaizenEventListeners.add(canvas, 'touchstart', touch_cb.bind(null, 0), false);
		KaizenEventListeners.add(canvas, 'touchend', touch_cb.bind(null, 1), false);
		KaizenEventListeners.add(canvas, 'touchcancel', touch_cb.bind(null, 1), false);
		KaizenEventListeners.add(canvas, 'touchmove', touch_cb.bind(null, 2), false);
	},

	/*
	 * Key API
	 */
	kaizen_js_input_key_cb__proxy: 'sync',
	kaizen_js_input_key_cb__sig: 'viii',
	kaizen_js_input_key_cb: function (pCallback, pCodePtr, pKeyPtr) {
		KaizenInput.inputKeyCallback = KaizenRuntime.get_func(pCallback);
		KaizenInput.setInputKeyData = (pCode, pKey) => {
			KaizenRuntime.stringToHeap(pCode, pCodePtr, 32);
			KaizenRuntime.stringToHeap(pKey, pKeyPtr, 32);
		};
		KaizenEventListeners.add(KaizenConfig.canvas, 'keydown', KaizenInput.onKeyEvent.bind(null, true), false);
		KaizenEventListeners.add(KaizenConfig.canvas, 'keyup', KaizenInput.onKeyEvent.bind(null, false), false);
	},

	/*
	 * IME API
	 */
	kaizen_js_set_ime_active__proxy: 'sync',
	kaizen_js_set_ime_active__sig: 'vi',
	kaizen_js_set_ime_active: function (p_active) {
		KaizenIME.ime_active(p_active);
	},

	kaizen_js_set_ime_position__proxy: 'sync',
	kaizen_js_set_ime_position__sig: 'vii',
	kaizen_js_set_ime_position: function (p_x, p_y) {
		KaizenIME.ime_position(p_x, p_y);
	},

	kaizen_js_set_ime_cb__proxy: 'sync',
	kaizen_js_set_ime_cb__sig: 'viiii',
	kaizen_js_set_ime_cb: function (p_ime_cb, p_key_cb, code, key) {
		const ime_cb = KaizenRuntime.get_func(p_ime_cb);
		const key_cb = KaizenRuntime.get_func(p_key_cb);
		KaizenIME.init(ime_cb, key_cb, code, key);
	},

	kaizen_js_is_ime_focused__proxy: 'sync',
	kaizen_js_is_ime_focused__sig: 'i',
	kaizen_js_is_ime_focused: function () {
		return KaizenIME.active;
	},

	/*
	 * Gamepad API
	 */
	kaizen_js_input_gamepad_cb__proxy: 'sync',
	kaizen_js_input_gamepad_cb__sig: 'vi',
	kaizen_js_input_gamepad_cb: function (change_cb) {
		const onchange = KaizenRuntime.get_func(change_cb);
		KaizenInputGamepads.init(onchange);
	},

	kaizen_js_input_gamepad_sample_count__proxy: 'sync',
	kaizen_js_input_gamepad_sample_count__sig: 'i',
	kaizen_js_input_gamepad_sample_count: function () {
		return KaizenInputGamepads.get_samples().length;
	},

	kaizen_js_input_gamepad_sample__proxy: 'sync',
	kaizen_js_input_gamepad_sample__sig: 'i',
	kaizen_js_input_gamepad_sample: function () {
		KaizenInputGamepads.sample();
		return 0;
	},

	kaizen_js_input_gamepad_sample_get__proxy: 'sync',
	kaizen_js_input_gamepad_sample_get__sig: 'iiiiiii',
	kaizen_js_input_gamepad_sample_get: function (p_index, r_btns, r_btns_num, r_axes, r_axes_num, r_standard) {
		const sample = KaizenInputGamepads.get_sample(p_index);
		if (!sample || !sample.connected) {
			return 1;
		}
		const btns = sample.buttons;
		const btns_len = btns.length < 16 ? btns.length : 16;
		for (let i = 0; i < btns_len; i++) {
			KaizenRuntime.setHeapValue(r_btns + (i << 2), btns[i], 'float');
		}
		KaizenRuntime.setHeapValue(r_btns_num, btns_len, 'i32');
		const axes = sample.axes;
		const axes_len = axes.length < 10 ? axes.length : 10;
		for (let i = 0; i < axes_len; i++) {
			KaizenRuntime.setHeapValue(r_axes + (i << 2), axes[i], 'float');
		}
		KaizenRuntime.setHeapValue(r_axes_num, axes_len, 'i32');
		const is_standard = sample.standard ? 1 : 0;
		KaizenRuntime.setHeapValue(r_standard, is_standard, 'i32');
		return 0;
	},

	/*
	 * Drag/Drop API
	 */
	kaizen_js_input_drop_files_cb__proxy: 'sync',
	kaizen_js_input_drop_files_cb__sig: 'vi',
	kaizen_js_input_drop_files_cb: function (callback) {
		const func = KaizenRuntime.get_func(callback);
		const dropFiles = function (files) {
			const args = files || [];
			if (!args.length) {
				return;
			}
			const argc = args.length;
			const argv = KaizenRuntime.allocStringArray(args);
			func(argv, argc);
			KaizenRuntime.freeStringArray(argv, argc);
		};
		const canvas = KaizenConfig.canvas;
		KaizenEventListeners.add(canvas, 'dragover', function (ev) {
			// Prevent default behavior (which would try to open the file(s))
			ev.preventDefault();
		}, false);
		KaizenEventListeners.add(canvas, 'drop', KaizenInputDragDrop.handler(dropFiles));
	},

	/* Paste API */
	kaizen_js_input_paste_cb__proxy: 'sync',
	kaizen_js_input_paste_cb__sig: 'vi',
	kaizen_js_input_paste_cb: function (callback) {
		const func = KaizenRuntime.get_func(callback);
		KaizenEventListeners.add(window, 'paste', function (evt) {
			const text = evt.clipboardData.getData('text');
			const ptr = KaizenRuntime.allocString(text);
			func(ptr);
			KaizenRuntime.free(ptr);
		}, false);
	},

	kaizen_js_input_vibrate_handheld__proxy: 'sync',
	kaizen_js_input_vibrate_handheld__sig: 'vi',
	kaizen_js_input_vibrate_handheld: function (p_duration_ms) {
		if (typeof navigator.vibrate !== 'function') {
			KaizenRuntime.print('This browser does not support vibration.');
		} else {
			navigator.vibrate(p_duration_ms);
		}
	},
};

autoAddDeps(KaizenInput, '$KaizenInput');
mergeInto(LibraryManager.library, KaizenInput);
