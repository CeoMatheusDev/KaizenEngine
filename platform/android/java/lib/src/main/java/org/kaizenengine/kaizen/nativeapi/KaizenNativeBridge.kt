/**************************************************************************/
/*  KaizenNativeBridge.kt                                                 */
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

package org.kaizenengine.kaizen.nativeapi

import android.accessibilityservice.AccessibilityServiceInfo
import android.annotation.SuppressLint
import android.app.AlertDialog
import android.content.Context
import android.content.DialogInterface
import android.content.Intent
import android.content.res.Configuration
import android.provider.Settings.Global
import android.os.Build
import android.os.VibrationEffect
import android.os.Vibrator
import android.view.accessibility.AccessibilityManager
import android.util.Log
import android.util.Rational
import android.util.TypedValue
import androidx.annotation.Keep
import androidx.core.net.toUri
import org.kaizenengine.kaizen.Kaizen
import org.kaizenengine.kaizen.KaizenActivity
import org.kaizenengine.kaizen.R
import org.kaizenengine.kaizen.error.Error
import org.kaizenengine.kaizen.feature.PictureInPictureProvider
import org.kaizenengine.kaizen.io.FilePicker
import org.kaizenengine.kaizen.utils.DialogUtils
import org.kaizenengine.kaizen.utils.KaizenNetUtils
import org.kaizenengine.kaizen.utils.beginBenchmarkMeasure
import org.kaizenengine.kaizen.utils.dumpBenchmark
import org.kaizenengine.kaizen.utils.endBenchmarkMeasure
import org.kaizenengine.kaizen.variant.Callable as KaizenCallable

/**
 * Holds and expose Kaizen apis to the native layer.
 *
 * All the methods in this class are accessed by the native code (java_kaizen_wrapper.h) and as such are kept private to
 * not be accessible by the rest of the java/kotlin code.
 */
@Keep
internal class KaizenNativeBridge(private val kaizen: Kaizen) {

	companion object {
		private val TAG = KaizenNativeBridge::class.java.simpleName

	}

	private val vibratorService: Vibrator? by lazy { kaizen.context.getSystemService(Context.VIBRATOR_SERVICE) as? Vibrator }

	/**
	 * Invoked on the render thread when the Kaizen setup is complete.
	 */
	private fun onKaizenSetupCompleted() {
		kaizen.onKaizenSetupCompleted()
	}

	/**
	 * Invoked on the render thread when the Kaizen main loop has started.
	 */
	private fun onKaizenMainLoopStarted() {
		kaizen.onKaizenMainLoopStarted()
	}

	/**
	 * Invoked on the render thread when the engine is about to terminate.
	 */
	private fun onKaizenTerminating() = kaizen.onKaizenTerminating()

	/**
	 * Invoked from the render thread to toggle the immersive mode.
	 */
	private fun nativeEnableImmersiveMode(enabled: Boolean) {
		kaizen.runOnHostThread {
			kaizen.enableImmersiveMode(enabled)
		}
	}

	private fun isInImmersiveMode() = kaizen.isInImmersiveMode()

	private fun isInEdgeToEdgeMode() = kaizen.isInEdgeToEdgeMode()

	private fun setKeepScreenOn(enabled: Boolean) = kaizen.setKeepScreenOn(enabled)

	private fun restart() { kaizen.primaryHost?.onKaizenRestartRequested(kaizen) }

	private fun alert(message: String, title: String) {
		kaizen.alert(message, title)
	}

	private fun forceQuit(instanceId: Int) = kaizen.forceQuit(instanceId)

	/**
	 * Returns true if screen reader is active, false otherwise.
	 */
	@Keep
	private fun isScreenReaderActive(): Boolean {
		val am = kaizen.context.getSystemService(Context.ACCESSIBILITY_SERVICE) as? AccessibilityManager
		if (am != null && am.isEnabled()) {
			val serviceInfoList = am.getEnabledAccessibilityServiceList(AccessibilityServiceInfo.FEEDBACK_SPOKEN)
			if (!serviceInfoList.isEmpty()) {
				return true
			}
		}
		return false
	}

	/**
	 * Returns true if high contrast text is enabled, false otherwise.
	 */
	@Keep
	private fun isHighContrastActive(): Boolean {
		val am = kaizen.context.getSystemService(Context.ACCESSIBILITY_SERVICE) as? AccessibilityManager
		if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.BAKLAVA && am != null && am.isEnabled()) {
			return am.isHighContrastTextEnabled()
		}
		return false
	}

	/**
	 * Returns true if animation is disabled, false otherwise.
	 */
	@Keep
	private fun isAnimationDisabled(): Boolean {
		val am = kaizen.context.getSystemService(Context.ACCESSIBILITY_SERVICE) as? AccessibilityManager
		if (am != null && am.isEnabled()) {
			return (Global.getFloat(kaizen.context.getContentResolver(), Global.ANIMATOR_DURATION_SCALE, 1.0f) == 0f
                && Global.getFloat(kaizen.context.getContentResolver(), Global.TRANSITION_ANIMATION_SCALE, 1.0f) == 0f
                && Global.getFloat(kaizen.context.getContentResolver(), Global.WINDOW_ANIMATION_SCALE, 1.0f) == 0f)
		}
		return false
	}

	/**
	 * Returns true if dark mode is supported, false otherwise.
	 */
	private fun isDarkModeSupported(): Boolean {
		return kaizen.context.resources?.configuration?.uiMode?.and(Configuration.UI_MODE_NIGHT_MASK) != Configuration.UI_MODE_NIGHT_UNDEFINED
	}

	/**
	 * Returns true if dark mode is supported and enabled, false otherwise.
	 */
	private fun isDarkMode() = kaizen.darkMode

	private fun showFilePicker(currentDirectory: String, filename: String, fileMode: Int, filters: Array<String>) {
		FilePicker.showFilePicker(kaizen.context, kaizen.getActivity(), currentDirectory, filename, fileMode, filters)
	}

	/**
	 * This method shows a dialog with multiple buttons.
	 *
	 * @param title The title of the dialog.
	 * @param message The message displayed in the dialog.
	 * @param buttons An array of button labels to display.
	 */
	private fun showDialog(title: String, message: String, buttons: Array<String>) {
		kaizen.getActivity()?.let { DialogUtils.showDialog(it, title, message, buttons) }
	}

	/**
	 * This method shows a dialog with a text input field, allowing the user to input text.
	 *
	 * @param title The title of the input dialog.
	 * @param message The message displayed in the input dialog.
	 * @param existingText The existing text that will be pre-filled in the input field.
	 */
	private fun showInputDialog(title: String, message: String, existingText: String) {
		kaizen.getActivity()?.let { DialogUtils.showInputDialog(it, title, message, existingText) }
	}

	private fun getAccentColor(): Int {
		val value = TypedValue()
		kaizen.context.theme.resolveAttribute(android.R.attr.colorAccent, value, true)
		return value.data
	}

	private fun getBaseColor(): Int {
		val value = TypedValue()
		kaizen.context.theme.resolveAttribute(android.R.attr.colorBackground, value, true)
		return value.data
	}

	private fun requestPermission(name: String?) = kaizen.requestPermission(name)

	private fun requestPermissions() = kaizen.requestPermissions()

	private fun getGrantedPermissions() = kaizen.getGrantedPermissions()

	/**
	 * Used by the native code (java_kaizen_wrapper.h) to vibrate the device.
	 * @param durationMs
	 */
	@SuppressLint("MissingPermission")
	private fun vibrate(durationMs: Int, amplitude: Int) {
		if (durationMs > 0 && kaizen.requestPermission("VIBRATE")) {
			try {
				if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
					if (amplitude <= -1) {
						vibratorService?.vibrate(
							VibrationEffect.createOneShot(
								durationMs.toLong(),
								VibrationEffect.DEFAULT_AMPLITUDE
							)
						)
					} else {
						vibratorService?.vibrate(
							VibrationEffect.createOneShot(
								durationMs.toLong(),
								amplitude
							)
						)
					}
				} else {
					// deprecated in API 26
					vibratorService?.vibrate(durationMs.toLong())
				}
			} catch (e: SecurityException) {
				Log.w(
					TAG,
					"SecurityException: VIBRATE permission not found. Make sure it is declared in the manifest or enabled in the export preset."
				)
			}
		}
	}

	/**
	 * Internal method used to query whether the host or the registered plugins supports a given feature.
	 *
	 * This is invoked by the native code, and should not be confused with [hasFeature] which is the Android version of
	 * https://docs.godotengine.org/en/stable/classes/class_os.html#class-os-method-has-feature
	 */
	private fun checkInternalFeatureSupport(feature: String): Boolean {
		if (kaizen.primaryHost?.supportsFeature(feature) == true) {
			return true
		}

		for (plugin in kaizen.pluginRegistry.allPlugins) {
			if (plugin.supportsFeature(feature)) {
				return true
			}
		}
		return false
	}

	/**
	 * Get the list of gdextension modules to register.
	 */
	private fun getGDExtensionConfigFiles(): Array<String> {
		val configFiles = mutableSetOf<String>()
		for (plugin in kaizen.pluginRegistry.allPlugins) {
			configFiles.addAll(plugin.pluginGDExtensionLibrariesPaths)
		}

		return configFiles.toTypedArray()
	}

	private fun getCACertificates(): String {
		return KaizenNetUtils.getCACertificates()
	}

	private fun getActivity() = kaizen.getActivity()

	private fun getRenderView() = kaizen.renderView

	private fun getClipboard() = kaizen.getClipboard()

	private fun setClipboard(text: String) = kaizen.setClipboard(text)

	private fun hasClipboard() = kaizen.hasClipboard()

	private fun setWindowColor(color: String) = kaizen.setWindowColor(color)

	/**
	 * Used by the native code (java_kaizen_wrapper.h) to access the input fallback mapping.
	 * @return The input fallback mapping for the current XR mode.
	 */
	private fun getInputFallbackMapping(): String? {
		return kaizen.xrMode.inputFallbackMapping
	}

	private fun initInputDevices() {
		kaizen.kaizenInputHandler.initInputDevices()
	}

	private fun createNewKaizenInstance(args: Array<String>): Int {
		return kaizen.primaryHost?.onNewKaizenInstanceRequested(args) ?: -1
	}

	private fun nativeBeginBenchmarkMeasure(scope: String, label: String) {
		beginBenchmarkMeasure(scope, label)
	}

	private fun nativeEndBenchmarkMeasure(scope: String, label: String) {
		endBenchmarkMeasure(scope, label)
	}

	private fun nativeDumpBenchmark(benchmarkFile: String) {
		dumpBenchmark(kaizen.fileAccessHandler, benchmarkFile)
	}

	private fun nativeSignApk(
		inputPath: String,
		outputPath: String,
		keystorePath: String,
		keystoreUser: String,
		keystorePassword: String
	): Int {
		val signResult = kaizen.primaryHost?.signApk(inputPath, outputPath, keystorePath, keystoreUser, keystorePassword)
			?: org.kaizenengine.kaizen.error.Error.ERR_UNAVAILABLE
		return signResult.toNativeValue()
	}

	private fun nativeVerifyApk(apkPath: String): Int {
		val verifyResult = kaizen.primaryHost?.verifyApk(apkPath) ?: Error.ERR_UNAVAILABLE
		return verifyResult.toNativeValue()
	}

	private fun nativeOnEditorWorkspaceSelected(workspace: String) {
		kaizen.primaryHost?.onEditorWorkspaceSelected(workspace)
	}

	private fun nativeShowEditorGameDock() {
		kaizen.primaryHost?.showEditorGameDock()
	}

	private fun nativeOnDistractionFreeModeChanged(enabled: Boolean) {
		kaizen.primaryHost?.onDistractionFreeModeChanged(enabled)
	}

	private fun nativeBuildEnvConnect(callback: KaizenCallable): Boolean {
		try {
			val buildProvider = kaizen.primaryHost?.buildProvider
			val success = buildProvider?.buildEnvConnect(callback) ?: false
			if (!success) {
				val activity = kaizen.getActivity() ?: return false
				kaizen.runOnHostThread {
					val builder = AlertDialog.Builder(activity)
						.setMessage(activity.getString(R.string.gabe_connection_error_message))
						.setTitle(activity.getString(R.string.gabe_connection_error_title))
						.setCancelable(false)
						.setPositiveButton(activity.getString(R.string.dialog_download)) { dialog: DialogInterface, _: Int ->
							val intent = Intent(Intent.ACTION_VIEW, "https://godotengine.org/download/android#gabe".toUri())
							intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
							activity.startActivity(intent)
							dialog.cancel()
						}
						.setNegativeButton(activity.getString(R.string.dialog_cancel)) { dialog: DialogInterface, _: Int ->
							dialog.dismiss()
						}
					val dialog = builder.create()
					dialog.show()
				}
			}
			return success
		} catch (e: Exception) {
			Log.e(TAG, "Unable to connect to build environment", e)
			return false
		}
	}

	private fun nativeBuildEnvDisconnect() {
		try {
			val buildProvider = kaizen.primaryHost?.buildProvider
			buildProvider?.buildEnvDisconnect()
		} catch (e: Exception) {
			Log.e(TAG, "Unable to disconnect from build environment", e)
		}
	}

	private fun nativeBuildEnvExecute(
		buildTool: String,
		arguments: Array<String>,
		projectPath: String,
		buildDir: String,
		outputCallback: KaizenCallable,
		resultCallback: KaizenCallable
	): Int {
		try {
			val buildProvider = kaizen.primaryHost?.buildProvider
			return buildProvider?.buildEnvExecute(
				buildTool,
				arguments,
				projectPath,
				buildDir,
				outputCallback,
				resultCallback
			) ?: -1
		} catch (e: Exception) {
			Log.e(TAG, "Unable to execute Gradle command in build environment", e);
			return -1
		}
	}

	private fun nativeBuildEnvCancel(jobId: Int) {
		try {
			val buildProvider = kaizen.primaryHost?.buildProvider
			buildProvider?.buildEnvCancel(jobId)
		} catch (e: Exception) {
			Log.e(TAG, "Unable to cancel command in build environment", e)
		}
	}

	private fun nativeBuildEnvCleanProject(projectPath: String, buildDir: String, callback: KaizenCallable) {
		try {
			val buildProvider = kaizen.primaryHost?.buildProvider
			buildProvider?.buildEnvCleanProject(projectPath, buildDir, callback)
		} catch (e: Exception) {
			Log.e(TAG, "Unable to clean project in build environment", e)
		}
	}

	private fun nativeIsPiPModeSupported(): Boolean {
		val hostActivity = kaizen.getActivity()
		if (hostActivity is PictureInPictureProvider) {
			return hostActivity.isPiPModeSupported()
		}
		return false
	}

	private fun nativeIsInPiPMode(): Boolean {
		val hostActivity = kaizen.getActivity()
		if (hostActivity is KaizenActivity) {
			return hostActivity.isInPictureInPictureMode
		}
		return false
	}

	private fun nativeEnterPiPMode() {
		val hostActivity = kaizen.getActivity()
		if (hostActivity is PictureInPictureProvider) {
			kaizen.runOnHostThread {
				hostActivity.enterPiPMode()
			}
		}
	}

	private fun nativeSetPiPModeAspectRatio(numerator: Int, denominator: Int) {
		val hostActivity = kaizen.getActivity()
		if (hostActivity is KaizenActivity) {
			kaizen.runOnHostThread {
				hostActivity.updatePiPParams(aspectRatio = Rational(numerator, denominator))
			}
		}
	}

	private fun nativeSetAutoEnterPiPModeOnBackground(autoEnterPiPOnBackground: Boolean) {
		val hostActivity = kaizen.getActivity()
		if (hostActivity is KaizenActivity) {
			kaizen.runOnHostThread {
				hostActivity.updatePiPParams(enableAutoEnter = autoEnterPiPOnBackground)
			}
		}
	}
}
