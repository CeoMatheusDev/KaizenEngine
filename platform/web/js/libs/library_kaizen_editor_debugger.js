/**************************************************************************/
/*  library_kaizen_editor_debugger.js                                      */
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

const KaizenEditorDebugger = {
	$KaizenEditorDebugger__postset: [
		'Module["add_debugger_session"] = KaizenEditorDebugger.addSession;',
	].join(''),
	$KaizenEditorDebugger__deps: ['$KaizenDebugger', '$KaizenRuntime', '$IDHandler'],
	$KaizenEditorDebugger: {
		session_callback: null,
		addSession: function (port) {
			if (!KaizenEditorDebugger.session_callback) {
				KaizenRuntime.error('Editor debugger not bound.');
				return;
			}
			const id = IDHandler.add({
				port: port,
				active: true,
			});
			KaizenEditorDebugger.session_callback(id);
		},
	},

	kaizen_js_editor_debugger_cb__proxy: 'sync',
	kaizen_js_editor_debugger_cb__sig: 'vp',
	kaizen_js_editor_debugger_cb: function (p_cb) {
		KaizenEditorDebugger.session_callback = p_cb ? KaizenRuntime.get_func(p_cb) : null;
	},

	kaizen_js_editor_debugger_active__proxy: 'sync',
	kaizen_js_editor_debugger_active__sig: 'i',
	kaizen_js_editor_debugger_active: function () {
		return KaizenEditorDebugger.session_callback ? 1 : 0;
	},
};

autoAddDeps(KaizenEditorDebugger, '$KaizenEditorDebugger');
mergeInto(LibraryManager.library, KaizenEditorDebugger);
