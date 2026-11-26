/**************************************************************************/
/*  library_kaizen_os.js                                                  */
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

const IDHandler = {
	$IDHandler: {
		_last_id: 0,
		_references: {},

		get: function (p_id) {
			return IDHandler._references[p_id];
		},

		add: function (p_data) {
			const id = ++IDHandler._last_id;
			IDHandler._references[id] = p_data;
			return id;
		},

		remove: function (p_id) {
			delete IDHandler._references[p_id];
		},
	},
};

autoAddDeps(IDHandler, '$IDHandler');
mergeInto(LibraryManager.library, IDHandler);

const KaizenConfig = {
	$KaizenConfig__postset: 'Module["initConfig"] = KaizenConfig.init_config;',
	$KaizenConfig__deps: ['$KaizenRuntime'],
	$KaizenConfig: {
		canvas: null,
		locale: 'en',
		canvas_resize_policy: 2, // Adaptive
		virtual_keyboard: false,
		kaizen_pool_size: 4,
		debug_port: null,
		on_execute: null,
		on_terminate_pid: null,
		on_exit: null,
		pid: 0,

		init_config: function (p_opts) {
			KaizenConfig.canvas_resize_policy = p_opts['canvasResizePolicy'];
			KaizenConfig.canvas = p_opts['canvas'];
			KaizenConfig.locale = p_opts['locale'] || KaizenConfig.locale;
			KaizenConfig.virtual_keyboard = p_opts['virtualKeyboard'];
			KaizenConfig.kaizen_pool_size = p_opts['kaizenPoolSize'];
			KaizenConfig.debug_port = p_opts['debugPort'];
			KaizenConfig.pid = p_opts['pid'];
			KaizenConfig.on_execute = p_opts['onExecute'];
			KaizenConfig.on_terminate_pid = p_opts['onTerminatePID'];
			KaizenConfig.on_exit = p_opts['onExit'];
			if (p_opts['focusCanvas']) {
				KaizenConfig.canvas.focus();
			}
		},

		locate_file: function (file) {
			return Module['locateFile'](file);
		},

		clear: function () {
			KaizenConfig.canvas = null;
			KaizenConfig.locale = 'en';
			KaizenConfig.canvas_resize_policy = 2;
			KaizenConfig.virtual_keyboard = false;
			KaizenConfig.debug_port = null;
			KaizenConfig.on_execute = null;
			KaizenConfig.on_exit = null;
		},
	},

	kaizen_js_config_pid_get__proxy: 'sync',
	kaizen_js_config_pid_get__sig: 'i',
	kaizen_js_config_pid_get: function () {
		return KaizenConfig.pid;
	},

	kaizen_js_config_canvas_id_get__proxy: 'sync',
	kaizen_js_config_canvas_id_get__sig: 'vii',
	kaizen_js_config_canvas_id_get: function (p_ptr, p_ptr_max) {
		KaizenRuntime.stringToHeap(`#${KaizenConfig.canvas.id}`, p_ptr, p_ptr_max);
	},

	kaizen_js_config_locale_get__proxy: 'sync',
	kaizen_js_config_locale_get__sig: 'vii',
	kaizen_js_config_locale_get: function (p_ptr, p_ptr_max) {
		KaizenRuntime.stringToHeap(KaizenConfig.locale, p_ptr, p_ptr_max);
	},
};

autoAddDeps(KaizenConfig, '$KaizenConfig');
mergeInto(LibraryManager.library, KaizenConfig);

const KaizenFS = {
	$KaizenFS__deps: ['$FS', '$IDBFS', '$KaizenRuntime'],
	$KaizenFS__postset: [
		'Module["initFS"] = KaizenFS.init;',
		'Module["copyToFS"] = KaizenFS.copy_to_fs;',
	].join(''),
	$KaizenFS: {
		// ERRNO_CODES works every odd version of emscripten, but this will break too eventually.
		ENOENT: 44,
		_idbfs: false,
		_syncing: false,
		_mount_points: [],

		is_persistent: function () {
			return KaizenFS._idbfs ? 1 : 0;
		},

		// Initialize kaizen file system, setting up persistent paths.
		// Returns a promise that resolves when the FS is ready.
		// We keep track of mount_points, so that we can properly close the IDBFS
		// since emscripten is not doing it by itself. (emscripten GH#12516).
		init: function (persistentPaths) {
			KaizenFS._idbfs = false;
			if (!Array.isArray(persistentPaths)) {
				return Promise.reject(new Error('Persistent paths must be an array'));
			}
			if (!persistentPaths.length) {
				return Promise.resolve();
			}
			KaizenFS._mount_points = persistentPaths.slice();

			function createRecursive(dir) {
				try {
					FS.stat(dir);
				} catch (e) {
					if (e.errno !== KaizenFS.ENOENT) {
						// Let mkdirTree throw in case, we cannot trust the above check.
						KaizenRuntime.error(e);
					}
					FS.mkdirTree(dir);
				}
			}

			KaizenFS._mount_points.forEach(function (path) {
				createRecursive(path);
				FS.mount(IDBFS, {}, path);
			});
			return new Promise(function (resolve, reject) {
				FS.syncfs(true, function (err) {
					if (err) {
						KaizenFS._mount_points = [];
						KaizenFS._idbfs = false;
						KaizenRuntime.print(`IndexedDB not available: ${err.message}`);
					} else {
						KaizenFS._idbfs = true;
					}
					resolve(err);
				});
			});
		},

		// Deinit kaizen file system, making sure to unmount file systems, and close IDBFS(s).
		deinit: function () {
			KaizenFS._mount_points.forEach(function (path) {
				try {
					FS.unmount(path);
				} catch (e) {
					KaizenRuntime.print('Already unmounted', e);
				}
				if (KaizenFS._idbfs && IDBFS.dbs[path]) {
					IDBFS.dbs[path].close();
					delete IDBFS.dbs[path];
				}
			});
			KaizenFS._mount_points = [];
			KaizenFS._idbfs = false;
			KaizenFS._syncing = false;
		},

		sync: function () {
			if (KaizenFS._syncing) {
				KaizenRuntime.error('Already syncing!');
				return Promise.resolve();
			}
			KaizenFS._syncing = true;
			return new Promise(function (resolve, reject) {
				FS.syncfs(false, function (error) {
					if (error) {
						KaizenRuntime.error(`Failed to save IDB file system: ${error.message}`);
					}
					KaizenFS._syncing = false;
					resolve(error);
				});
			});
		},

		// Copies a buffer to the internal file system. Creating directories recursively.
		copy_to_fs: function (path, buffer) {
			const idx = path.lastIndexOf('/');
			let dir = '/';
			if (idx > 0) {
				dir = path.slice(0, idx);
			}
			try {
				FS.stat(dir);
			} catch (e) {
				if (e.errno !== KaizenFS.ENOENT) {
					// Let mkdirTree throw in case, we cannot trust the above check.
					KaizenRuntime.error(e);
				}
				FS.mkdirTree(dir);
			}
			FS.writeFile(path, new Uint8Array(buffer));
		},
	},
};
mergeInto(LibraryManager.library, KaizenFS);

const KaizenOS = {
	$KaizenOS__deps: ['$KaizenRuntime', '$KaizenConfig', '$KaizenFS'],
	$KaizenOS__postset: [
		'Module["request_quit"] = function() { KaizenOS.request_quit() };',
		'Module["force_quit"] = function(code) { _emscripten_force_exit(code ?? 1) };',
		'Module["onExit"] = KaizenOS.cleanup;',
		'KaizenOS._fs_sync_promise = Promise.resolve();',
	].join(''),
	$KaizenOS: {
		request_quit: function () {},
		_async_cbs: [],
		_fs_sync_promise: null,

		atexit: function (p_promise_cb) {
			KaizenOS._async_cbs.push(p_promise_cb);
		},

		cleanup: function (exit_code) {
			const cb = KaizenConfig.on_exit;
			KaizenFS.deinit();
			KaizenConfig.clear();
			if (cb) {
				cb(exit_code);
			}
		},

		finish_async: function (callback) {
			KaizenOS._fs_sync_promise.then(function (err) {
				const promises = [];
				KaizenOS._async_cbs.forEach(function (cb) {
					promises.push(new Promise(cb));
				});
				return Promise.all(promises);
			}).then(function () {
				return KaizenFS.sync(); // Final FS sync.
			}).then(function (err) {
				// Always deferred.
				setTimeout(function () {
					callback();
				}, 0);
			});
		},
	},

	kaizen_js_os_finish_async__proxy: 'sync',
	kaizen_js_os_finish_async__sig: 'vi',
	kaizen_js_os_finish_async: function (p_callback) {
		const func = KaizenRuntime.get_func(p_callback);
		KaizenOS.finish_async(func);
	},

	kaizen_js_os_request_quit_cb__proxy: 'sync',
	kaizen_js_os_request_quit_cb__sig: 'vi',
	kaizen_js_os_request_quit_cb: function (p_callback) {
		KaizenOS.request_quit = KaizenRuntime.get_func(p_callback);
	},

	kaizen_js_os_fs_is_persistent__proxy: 'sync',
	kaizen_js_os_fs_is_persistent__sig: 'i',
	kaizen_js_os_fs_is_persistent: function () {
		return KaizenFS.is_persistent();
	},

	kaizen_js_os_fs_sync__proxy: 'sync',
	kaizen_js_os_fs_sync__sig: 'vi',
	kaizen_js_os_fs_sync: function (callback) {
		const func = KaizenRuntime.get_func(callback);
		KaizenOS._fs_sync_promise = KaizenFS.sync();
		KaizenOS._fs_sync_promise.then(function (err) {
			func();
		});
	},

	kaizen_js_os_has_feature__proxy: 'sync',
	kaizen_js_os_has_feature__sig: 'ii',
	kaizen_js_os_has_feature: function (p_ftr) {
		const ftr = KaizenRuntime.parseString(p_ftr);
		const ua = navigator.userAgent;
		if (ftr === 'web_macos') {
			return (ua.indexOf('Mac') !== -1) ? 1 : 0;
		}
		if (ftr === 'web_windows') {
			return (ua.indexOf('Windows') !== -1) ? 1 : 0;
		}
		if (ftr === 'web_android') {
			return (ua.indexOf('Android') !== -1) ? 1 : 0;
		}
		if (ftr === 'web_ios') {
			return ((ua.indexOf('iPhone') !== -1) || (ua.indexOf('iPad') !== -1) || (ua.indexOf('iPod') !== -1)) ? 1 : 0;
		}
		if (ftr === 'web_linuxbsd') {
			return ((ua.indexOf('CrOS') !== -1) || (ua.indexOf('BSD') !== -1) || (ua.indexOf('Linux') !== -1) || (ua.indexOf('X11') !== -1)) ? 1 : 0;
		}
		return 0;
	},

	kaizen_js_os_execute__proxy: 'sync',
	kaizen_js_os_execute__sig: 'ipp',
	kaizen_js_os_execute: function (p_path, p_args) {
		const path = KaizenRuntime.parseString(p_path);
		const json_args = KaizenRuntime.parseString(p_args);
		const args = JSON.parse(json_args);
		if (KaizenConfig.on_execute) {
			return KaizenConfig.on_execute(path, args) ?? 0;
		}
		return -1;
	},

	kaizen_js_os_kill__proxy: 'sync',
	kaizen_js_os_kill__sig: 'ii',
	kaizen_js_os_kill: function (p_pid) {
		if (!KaizenConfig.on_terminate_pid) {
			return -1;
		}
		KaizenConfig.on_terminate_pid(p_pid);
		return 0;
	},

	kaizen_js_os_shell_open__proxy: 'sync',
	kaizen_js_os_shell_open__sig: 'vi',
	kaizen_js_os_shell_open: function (p_uri) {
		window.open(KaizenRuntime.parseString(p_uri), '_blank');
	},

	kaizen_js_os_hw_concurrency_get__proxy: 'sync',
	kaizen_js_os_hw_concurrency_get__sig: 'i',
	kaizen_js_os_hw_concurrency_get: function () {
		// TODO Kaizen core needs fixing to avoid spawning too many threads (> 24).
		const concurrency = navigator.hardwareConcurrency || 1;
		return concurrency < 2 ? concurrency : 2;
	},

	kaizen_js_os_thread_pool_size_get__proxy: 'sync',
	kaizen_js_os_thread_pool_size_get__sig: 'i',
	kaizen_js_os_thread_pool_size_get: function () {
		if (typeof PThread === 'undefined') {
			// Threads aren't supported, so default to `1`.
			return 1;
		}

		return KaizenConfig.kaizen_pool_size;
	},

	kaizen_js_os_download_buffer__proxy: 'sync',
	kaizen_js_os_download_buffer__sig: 'viiii',
	kaizen_js_os_download_buffer: function (p_ptr, p_size, p_name, p_mime) {
		const buf = KaizenRuntime.heapSlice(HEAP8, p_ptr, p_size);
		const name = KaizenRuntime.parseString(p_name);
		const mime = KaizenRuntime.parseString(p_mime);
		const blob = new Blob([buf], { type: mime });
		const url = window.URL.createObjectURL(blob);
		const a = document.createElement('a');
		a.href = url;
		a.download = name;
		a.style.display = 'none';
		document.body.appendChild(a);
		a.click();
		a.remove();
		window.URL.revokeObjectURL(url);
	},
};

autoAddDeps(KaizenOS, '$KaizenOS');
mergeInto(LibraryManager.library, KaizenOS);

/*
 * Kaizen event listeners.
 * Keeps track of registered event listeners so it can remove them on shutdown.
 */
const KaizenEventListeners = {
	$KaizenEventListeners__deps: ['$KaizenOS'],
	$KaizenEventListeners__postset: 'KaizenOS.atexit(function(resolve, reject) { KaizenEventListeners.clear(); resolve(); });',
	$KaizenEventListeners: {
		handlers: [],

		has: function (target, event, method, capture) {
			return KaizenEventListeners.handlers.findIndex(function (e) {
				return e.target === target && e.event === event && e.method === method && e.capture === capture;
			}) !== -1;
		},

		add: function (target, event, method, capture) {
			if (KaizenEventListeners.has(target, event, method, capture)) {
				return;
			}
			function Handler(p_target, p_event, p_method, p_capture) {
				this.target = p_target;
				this.event = p_event;
				this.method = p_method;
				this.capture = p_capture;
			}
			KaizenEventListeners.handlers.push(new Handler(target, event, method, capture));
			target.addEventListener(event, method, capture);
		},

		clear: function () {
			KaizenEventListeners.handlers.forEach(function (h) {
				h.target.removeEventListener(h.event, h.method, h.capture);
			});
			KaizenEventListeners.handlers.length = 0;
		},
	},
};
mergeInto(LibraryManager.library, KaizenEventListeners);

const KaizenPWA = {

	$KaizenPWA__deps: ['$KaizenRuntime', '$KaizenEventListeners'],
	$KaizenPWA: {
		hasUpdate: false,

		updateState: function (cb, reg) {
			if (!reg) {
				return;
			}
			if (!reg.active) {
				return;
			}
			if (reg.waiting) {
				KaizenPWA.hasUpdate = true;
				cb();
			}
			KaizenEventListeners.add(reg, 'updatefound', function () {
				const installing = reg.installing;
				KaizenEventListeners.add(installing, 'statechange', function () {
					if (installing.state === 'installed') {
						KaizenPWA.hasUpdate = true;
						cb();
					}
				});
			});
		},
	},

	kaizen_js_pwa_cb__proxy: 'sync',
	kaizen_js_pwa_cb__sig: 'vi',
	kaizen_js_pwa_cb: function (p_update_cb) {
		if ('serviceWorker' in navigator) {
			try {
				const cb = KaizenRuntime.get_func(p_update_cb);
				navigator.serviceWorker.getRegistration().then(KaizenPWA.updateState.bind(null, cb));
			} catch (e) {
				KaizenRuntime.error('Failed to assign PWA callback', e);
			}
		}
	},

	kaizen_js_pwa_update__proxy: 'sync',
	kaizen_js_pwa_update__sig: 'i',
	kaizen_js_pwa_update: function () {
		if ('serviceWorker' in navigator && KaizenPWA.hasUpdate) {
			try {
				navigator.serviceWorker.getRegistration().then(function (reg) {
					if (!reg || !reg.waiting) {
						return;
					}
					reg.waiting.postMessage('update');
				});
			} catch (e) {
				KaizenRuntime.error(e);
				return 1;
			}
			return 0;
		}
		return 1;
	},
};

autoAddDeps(KaizenPWA, '$KaizenPWA');
mergeInto(LibraryManager.library, KaizenPWA);
