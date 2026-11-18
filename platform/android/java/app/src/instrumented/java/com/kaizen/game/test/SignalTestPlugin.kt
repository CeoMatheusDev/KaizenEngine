package com.kaizen.game.test

import org.kaizenengine.kaizen.Kaizen
import org.kaizenengine.kaizen.plugin.KaizenPlugin
import org.kaizenengine.kaizen.plugin.SignalInfo
import org.kaizenengine.kaizen.plugin.UsedByKaizen

class SignalTestPlugin(kaizen: Kaizen) : KaizenPlugin(kaizen) {

	companion object {
		private val EMISSION_TEST_SIGNAL = SignalInfo("emission_test_signal")
		private val LAUNCH_TESTS_SIGNAL = SignalInfo("launch_tests", java.lang.Boolean::class.java, String::class.java)
	}

	override fun getPluginName() = "SignalTestPlugin"

	override fun getPluginSignals(): Set<SignalInfo?> {
		return setOf(
			EMISSION_TEST_SIGNAL,
			LAUNCH_TESTS_SIGNAL
		)
	}

	@UsedByKaizen
	fun triggerTestSignal1() {
		emitSignal(EMISSION_TEST_SIGNAL)
	}

	@UsedByKaizen
	fun triggerLaunchTestSignal() {
		emitSignal(LAUNCH_TESTS_SIGNAL, true, "second message")
	}
}
