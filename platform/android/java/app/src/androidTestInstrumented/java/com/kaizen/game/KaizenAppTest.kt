/**************************************************************************/
/*  KaizenAppTest.kt                                                      */
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

package com.kaizen.game

import android.content.ComponentName
import android.content.Intent
import android.util.Log
import androidx.test.core.app.ActivityScenario
import androidx.test.espresso.Espresso
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.platform.app.InstrumentationRegistry
import com.kaizen.game.app.BuildConfig
import com.kaizen.game.test.KaizenAppInstrumentedTestPlugin
import org.kaizenengine.kaizen.Kaizen
import org.kaizenengine.kaizen.KaizenActivity.Companion.EXTRA_COMMAND_LINE_PARAMS
import org.kaizenengine.kaizen.plugin.KaizenPluginRegistry
import org.junit.Test
import org.junit.runner.RunWith
import kotlin.collections.contentEquals
import kotlin.test.assertEquals
import kotlin.test.assertNotNull
import kotlin.test.assertNull
import kotlin.test.assertTrue

/**
 * This instrumented test will launch the `instrumented` version of KaizenApp and run a set of tests against it.
 */
@RunWith(AndroidJUnit4::class)
class KaizenAppTest {

	companion object {
		private val TAG = KaizenAppTest::class.java.simpleName

		private const val KAIZEN_APP_LAUNCHER_CLASS_NAME = "com.kaizen.game.KaizenAppLauncher"
		private const val KAIZEN_APP_CLASS_NAME = "com.kaizen.game.KaizenApp"

		private val TEST_COMMAND_LINE_PARAMS = arrayOf("This is a test")
		private val VULKAN_RENDERER_COMMAND_LINE_PARAMS = arrayOf("--rendering-method","mobile")
	}

	private fun getTestPlugin(): KaizenAppInstrumentedTestPlugin? {
		return KaizenPluginRegistry.getPluginRegistry()
			.getPlugin("KaizenAppInstrumentedTestPlugin") as KaizenAppInstrumentedTestPlugin?
	}

	/**
	 * Runs the JavaClassWrapper tests via the KaizenAppInstrumentedTestPlugin.
	 */
	@Test
	fun runJavaClassWrapperTests() {
		ActivityScenario.launch(KaizenApp::class.java).use { scenario ->
			scenario.onActivity { activity ->
				val testPlugin = getTestPlugin()
				assertNotNull(testPlugin)

				Log.d(TAG, "Waiting for the Kaizen main loop to start...")
				testPlugin.waitForKaizenMainLoopStarted()

				Log.d(TAG, "Running JavaClassWrapper tests...")
				val result = testPlugin.runJavaClassWrapperTests()
				assertNotNull(result)
				result.exceptionOrNull()?.let { throw it }
				assertTrue(result.isSuccess)
				Log.d(TAG, "Passed ${result.getOrNull()} tests")
			}
		}
	}

	/**
	 * Runs file access related tests.
	 */
	@Test
	fun runFileAccessTests() {
		ActivityScenario.launch(KaizenApp::class.java).use { scenario ->
			scenario.onActivity { activity ->
				val testPlugin = getTestPlugin()
				assertNotNull(testPlugin)

				Log.d(TAG, "Waiting for the Kaizen main loop to start...")
				testPlugin.waitForKaizenMainLoopStarted()

				Log.d(TAG, "Running FileAccess tests...")
				val result = testPlugin.runFileAccessTests()
				assertNotNull(result)
				result.exceptionOrNull()?.let { throw it }
				assertTrue(result.isSuccess)
			}
		}
	}

	/**
	 * Runs test to validate android plugin signals.
	 */
	@Test
	fun runPluginSignalTests() {
		ActivityScenario.launch(KaizenApp::class.java).use { scenario ->
			scenario.onActivity { activity ->
				val testPlugin = getTestPlugin()
				assertNotNull(testPlugin)

				Log.d(TAG, "Waiting for the Kaizen main loop to start...")
				testPlugin.waitForKaizenMainLoopStarted()

				Log.d(TAG, "Running Android plugin signal tests...")
				val result = testPlugin.runPluginSignalTests()
				assertNotNull(result)
				result.exceptionOrNull()?.let { throw it }
				assertTrue(result.isSuccess)
			}
		}
	}

	/**
	 * Test implicit launch of the Kaizen app, and validates this resolves to the `KaizenAppLauncher` activity alias.
	 */
	@Test
	fun testImplicitKaizenAppLauncherLaunch() {
		val implicitLaunchIntent = Intent().apply {
			setPackage(BuildConfig.APPLICATION_ID)
			action = Intent.ACTION_MAIN
			addCategory(Intent.CATEGORY_LAUNCHER)
			putExtra(EXTRA_COMMAND_LINE_PARAMS, TEST_COMMAND_LINE_PARAMS)
		}
		ActivityScenario.launch<KaizenApp>(implicitLaunchIntent).use { scenario ->
			scenario.onActivity { activity ->
				assertEquals(activity.intent.component?.className, KAIZEN_APP_LAUNCHER_CLASS_NAME)

				val commandLineParams = activity.intent.getStringArrayExtra(EXTRA_COMMAND_LINE_PARAMS)
				assertNull(commandLineParams)
			}
		}
	}

	/**
	 * Test explicit launch of the Kaizen app via its activity-alias launcher, and validates it resolves properly.
	 */
	@Test
	fun testExplicitKaizenAppLauncherLaunch() {
		val explicitIntent = Intent().apply {
			component = ComponentName(BuildConfig.APPLICATION_ID, KAIZEN_APP_LAUNCHER_CLASS_NAME)
			putExtra(EXTRA_COMMAND_LINE_PARAMS, TEST_COMMAND_LINE_PARAMS)
		}
		ActivityScenario.launch<KaizenApp>(explicitIntent).use { scenario ->
			scenario.onActivity { activity ->
				assertEquals(activity.intent.component?.className, KAIZEN_APP_LAUNCHER_CLASS_NAME)

				val commandLineParams = activity.intent.getStringArrayExtra(EXTRA_COMMAND_LINE_PARAMS)
				assertNull(commandLineParams)
			}
		}
	}

	/**
	 * Test explicit launch of the `KaizenApp` activity.
	 */
	@Test
	fun testExplicitKaizenAppLaunch() {
		val explicitIntent = Intent().apply {
			component = ComponentName(BuildConfig.APPLICATION_ID, KAIZEN_APP_CLASS_NAME)
			putExtra(EXTRA_COMMAND_LINE_PARAMS, TEST_COMMAND_LINE_PARAMS)
		}
		ActivityScenario.launch<KaizenApp>(explicitIntent).use { scenario ->
			scenario.onActivity { activity ->
				assertEquals(activity.intent.component?.className, KAIZEN_APP_CLASS_NAME)

				val commandLineParams = activity.intent.getStringArrayExtra(EXTRA_COMMAND_LINE_PARAMS)
				assertNotNull(commandLineParams)
				assertTrue(commandLineParams.contentEquals(TEST_COMMAND_LINE_PARAMS))
			}
		}
	}

	/**
	 * Test that launching using the Vulkan renderer doesn't crash the app.
	 */
	@Test
	fun testVulkanKaizenAppLaunch() {
		val appIntent = Intent().apply {
			component = ComponentName(BuildConfig.APPLICATION_ID, KAIZEN_APP_CLASS_NAME)
			putExtra(EXTRA_COMMAND_LINE_PARAMS, VULKAN_RENDERER_COMMAND_LINE_PARAMS)
		}
		ActivityScenario.launch<KaizenApp>(appIntent).use { scenario ->
			val testPlugin = getTestPlugin()
			assertNotNull(testPlugin)

			Log.d(TAG, "Waiting for the Kaizen main loop to start...")
			testPlugin.waitForKaizenMainLoopStarted()

			scenario.onActivity { activity ->
				assertEquals(activity.intent.component?.className, KAIZEN_APP_CLASS_NAME)

				val commandLineParams = activity.intent.getStringArrayExtra(EXTRA_COMMAND_LINE_PARAMS)
				assertNotNull(commandLineParams)
				assertTrue(commandLineParams.contentEquals(VULKAN_RENDERER_COMMAND_LINE_PARAMS))
			}
		}
	}

	/**
	 * Validate that the back press does not quit the game when 'quit_on_go_back' is disabled.
	 */
	@Test
	fun testGameNotQuittingOnBackPress() {
		ActivityScenario.launch(KaizenApp::class.java).use { scenario ->
			val testPlugin = getTestPlugin()
			assertNotNull(testPlugin)

			Log.d(TAG, "Waiting for the Kaizen main loop to start...")
			testPlugin.waitForKaizenMainLoopStarted()

			// Disable 'quit_on_go_back'.
			testPlugin.updateQuitOnGoBack(false)

			// Trigger the back press event.
			Espresso.pressBackUnconditionally()

			Log.d(TAG, "Waiting for the engine to terminate...")
			testPlugin.waitForEngineTermination(5_000L)

			val kaizen = Kaizen.getInstance(InstrumentationRegistry.getInstrumentation().targetContext)
			assertTrue { kaizen.runStatus != Kaizen.RunStatus.TERMINATING }
		}
	}

	/**
	 * Validate that the back press event quits the game when 'quit_on_go_back' is enabled.
	 */
	@Test
	fun testGameQuittingOnBackPress() {
		ActivityScenario.launch(KaizenApp::class.java).use { scenario ->
			val testPlugin = getTestPlugin()
			assertNotNull(testPlugin)

			Log.d(TAG, "Waiting for the Kaizen main loop to start...")
			testPlugin.waitForKaizenMainLoopStarted()

			// Enable 'quit_on_go_back'.
			testPlugin.updateQuitOnGoBack(true)

			// Trigger the back press event.
			Espresso.pressBackUnconditionally()

			Log.d(TAG, "Waiting for the engine to terminate...")
			testPlugin.waitForEngineTermination(5_000L)

			val kaizen = Kaizen.getInstance(InstrumentationRegistry.getInstrumentation().targetContext)
			assertTrue { kaizen.runStatus == Kaizen.RunStatus.TERMINATING }
		}
	}
}
