/**************************************************************************/
/*  KaizenApp.java                                                        */
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

package com.kaizen.game;

import org.kaizenengine.kaizen.Kaizen;
import org.kaizenengine.kaizen.KaizenActivity;

import android.os.Bundle;

import androidx.activity.EdgeToEdge;
import androidx.core.splashscreen.SplashScreen;

/**
 * Template activity for Kaizen Android builds.
 * Feel free to extend and modify this class for your custom logic.
 */
public class KaizenApp extends KaizenActivity {
	private final Runnable updateWindowAppearance = () -> {
		Kaizen kaizen = getKaizen();
		if (kaizen != null) {
			kaizen.enableImmersiveMode(kaizen.isInImmersiveMode(), true);
			kaizen.enableEdgeToEdge(kaizen.isInEdgeToEdgeMode(), true);
			kaizen.setSystemBarsAppearance();
		}
	};

	@Override
	public void onCreate(Bundle savedInstanceState) {
		SplashScreen splashScreen = SplashScreen.installSplashScreen(this);
		EdgeToEdge.enable(this);
		super.onCreate(savedInstanceState);

		Kaizen kaizen = getKaizen();
		if (kaizen != null && kaizen.getDisableKaizenSplash()) {
			splashScreen.setKeepOnScreenCondition(() -> kaizen.getRunStatus() != Kaizen.RunStatus.STARTED);
		}
	}

	@Override
	public void onResume() {
		super.onResume();
		updateWindowAppearance.run();
	}

	@Override
	public void onKaizenMainLoopStarted() {
		super.onKaizenMainLoopStarted();
		runOnUiThread(updateWindowAppearance);
	}

	@Override
	public void onKaizenForceQuit(Kaizen instance) {
		if (!getPackageName().equals("com.kaizen.game.instrumented")) {
			// For instrumented builds, we disable force-quitting to allow the instrumented tests to complete
			// successfully, otherwise they fail when the process crashes.
			super.onKaizenForceQuit(instance);
		}
	}

	@Override
	protected boolean isPiPEnabled() {
		return true;
	}
}
