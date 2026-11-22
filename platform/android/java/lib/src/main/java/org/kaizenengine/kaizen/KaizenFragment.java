/**************************************************************************/
/*  KaizenFragment.java                                                   */
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

package org.kaizenengine.kaizen;

import org.kaizenengine.kaizen.error.Error;
import org.kaizenengine.kaizen.plugin.KaizenPlugin;
import org.kaizenengine.kaizen.utils.BenchmarkUtils;

import android.content.Context;
import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;
import android.text.TextUtils;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;

import androidx.annotation.CallSuper;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import java.util.Collections;
import java.util.List;
import java.util.Set;

/**
 * Base fragment for Android apps intending to use Kaizen for part of the app's UI.
 */
public class KaizenFragment extends Fragment implements KaizenHost {
	private static final String TAG = KaizenFragment.class.getSimpleName();

	private FrameLayout kaizenContainerLayout;

	@Nullable
	private KaizenHost parentHost;
	private Kaizen kaizen;

	@Override
	public Kaizen getKaizen() {
		return kaizen;
	}

	@Override
	public void onAttach(@NonNull Context context) {
		super.onAttach(context);
		if (getParentFragment() instanceof KaizenHost) {
			parentHost = (KaizenHost)getParentFragment();
		} else if (getActivity() instanceof KaizenHost) {
			parentHost = (KaizenHost)getActivity();
		}
	}

	@Override
	public void onDetach() {
		if (kaizenContainerLayout != null && kaizenContainerLayout.getParent() != null) {
			Log.d(TAG, "Cleaning up Kaizen container layout during detach.");
			((ViewGroup)kaizenContainerLayout.getParent()).removeView(kaizenContainerLayout);
		}

		super.onDetach();
		parentHost = null;
	}

	@CallSuper
	@Override
	public void onConfigurationChanged(Configuration newConfig) {
		super.onConfigurationChanged(newConfig);
		kaizen.onConfigurationChanged(newConfig);
	}

	@CallSuper
	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		super.onActivityResult(requestCode, resultCode, data);
		kaizen.onActivityResult(requestCode, resultCode, data);
	}

	@CallSuper
	@Override
	public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
		super.onRequestPermissionsResult(requestCode, permissions, grantResults);
		kaizen.onRequestPermissionsResult(requestCode, permissions, grantResults);
	}

	@Override
	public void onCreate(Bundle icicle) {
		BenchmarkUtils.beginBenchmarkMeasure("Startup", "KaizenFragment::onCreate");
		super.onCreate(icicle);

		if (parentHost != null) {
			kaizen = parentHost.getKaizen();
		}
		if (kaizen == null) {
			kaizen = Kaizen.getInstance(requireContext());
		}
		performEngineInitialization();
		BenchmarkUtils.endBenchmarkMeasure("Startup", "KaizenFragment::onCreate");
	}

	private void performEngineInitialization() {
		try {
			if (!kaizen.initEngine(this, getCommandLine(), getHostPlugins(kaizen))) {
				throw new IllegalStateException("Unable to initialize Kaizen engine");
			}

			kaizenContainerLayout = kaizen.onInitRenderView(this);
			if (kaizenContainerLayout == null) {
				throw new IllegalStateException("Unable to initialize engine render view");
			}
		} catch (Exception e) {
			Log.e(TAG, "Engine initialization failed", e);
			final String errorMessage = TextUtils.isEmpty(e.getMessage())
					? getString(R.string.error_engine_setup_message)
					: e.getMessage();
			kaizen.alert(errorMessage, getString(R.string.text_error_title), kaizen::destroyAndKillProcess);
		}
	}

	@Override
	public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle icicle) {
		if (kaizenContainerLayout != null && kaizenContainerLayout.getParent() != null) {
			Log.w(TAG, "Kaizen container layout already has a parent, removing it.");
			((ViewGroup)kaizenContainerLayout.getParent()).removeView(kaizenContainerLayout);
		}

		return kaizenContainerLayout;
	}

	@Override
	public void onDestroy() {
		if (kaizenContainerLayout != null && kaizenContainerLayout.getParent() != null) {
			Log.w(TAG, "Removing Kaizen container layout from parent during destruction.");
			((ViewGroup)kaizenContainerLayout.getParent()).removeView(kaizenContainerLayout);
		}

		kaizen.onDestroy(this);
		super.onDestroy();
	}

	@Override
	public void onPause() {
		super.onPause();
		kaizen.onPause(this);
	}

	@Override
	public void onStop() {
		super.onStop();
		kaizen.onStop(this);
	}

	@Override
	public void onStart() {
		super.onStart();
		kaizen.onStart(this);
	}

	@Override
	public void onResume() {
		super.onResume();
		kaizen.onResume(this);
	}

	public void onBackPressed() {
		kaizen.onBackPressed();
	}

	@CallSuper
	@Override
	public List<String> getCommandLine() {
		return parentHost != null ? parentHost.getCommandLine() : Collections.emptyList();
	}

	@CallSuper
	@Override
	public void onKaizenSetupCompleted() {
		if (parentHost != null) {
			parentHost.onKaizenSetupCompleted();
		}
	}

	@CallSuper
	@Override
	public void onKaizenMainLoopStarted() {
		if (parentHost != null) {
			parentHost.onKaizenMainLoopStarted();
		}
	}

	@Override
	public void onKaizenForceQuit(Kaizen instance) {
		if (parentHost != null) {
			parentHost.onKaizenForceQuit(instance);
		}
	}

	@Override
	public boolean onKaizenForceQuit(int kaizenInstanceId) {
		return parentHost != null && parentHost.onKaizenForceQuit(kaizenInstanceId);
	}

	@Override
	public void onKaizenRestartRequested(Kaizen instance) {
		if (parentHost != null) {
			parentHost.onKaizenRestartRequested(instance);
		}
	}

	@Override
	public int onNewKaizenInstanceRequested(String[] args) {
		if (parentHost != null) {
			return parentHost.onNewKaizenInstanceRequested(args);
		}
		return -1;
	}

	@Override
	@CallSuper
	public Set<KaizenPlugin> getHostPlugins(Kaizen engine) {
		if (parentHost != null) {
			return parentHost.getHostPlugins(engine);
		}
		return Collections.emptySet();
	}

	@Override
	public Error signApk(@NonNull String inputPath, @NonNull String outputPath, @NonNull String keystorePath, @NonNull String keystoreUser, @NonNull String keystorePassword) {
		if (parentHost != null) {
			return parentHost.signApk(inputPath, outputPath, keystorePath, keystoreUser, keystorePassword);
		}
		return Error.ERR_UNAVAILABLE;
	}

	@Override
	public Error verifyApk(@NonNull String apkPath) {
		if (parentHost != null) {
			return parentHost.verifyApk(apkPath);
		}
		return Error.ERR_UNAVAILABLE;
	}

	@Override
	public boolean supportsFeature(String featureTag) {
		if (parentHost != null) {
			return parentHost.supportsFeature(featureTag);
		}
		return false;
	}

	@Override
	public void onEditorWorkspaceSelected(String workspace) {
		if (parentHost != null) {
			parentHost.onEditorWorkspaceSelected(workspace);
		}
	}

	@Override
	public void showEditorGameDock() {
		if (parentHost != null) {
			parentHost.showEditorGameDock();
		}
	}

	@Override
	public void onDistractionFreeModeChanged(Boolean enabled) {
		if (parentHost != null) {
			parentHost.onDistractionFreeModeChanged(enabled);
		}
	}

	@Override
	public BuildProvider getBuildProvider() {
		if (parentHost != null) {
			return parentHost.getBuildProvider();
		}
		return null;
	}
}
