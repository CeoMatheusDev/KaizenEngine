/**************************************************************************/
/*  library_kaizen_audio.js                                                */
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

function KaizenSampleDriver(audioContext, positionWorkletPath) {
	/** @type {?AudioContext} */
	let ctx = audioContext;
	/** @type {?Promise} */
	let audioPositionWorkletPromise = ctx.audioWorklet ? ctx.audioWorklet.addModule(positionWorkletPath) : null;
	/** @type {!Array<AudioWorkletNode>} */
	const audioPositionWorkletNodes = [];

	/** @enum {string} */
	const LoopMode = {
		DISABLED: 'disabled',
		FORWARD: 'forward',
		BACKWARD: 'backward',
		PINGPONG: 'pingpong',
	};
	const KaizenChannel = Object.freeze({
		CHANNEL_L: 0,
		CHANNEL_R: 1,
		CHANNEL_C: 2,
		CHANNEL_LFE: 3,
		CHANNEL_SL: 4, // Surround left
		CHANNEL_SR: 5, // Surround right
		CHANNEL_SBL: 6, // Surround back left (7.1)
		CHANNEL_SBR: 7, // Surround back right (7.1)
	});
	const WebChannel = Object.freeze({
		CHANNEL_L: 0,
		CHANNEL_R: 1,
		CHANNEL_SL: 2,
		CHANNEL_SR: 3,
		CHANNEL_C: 4,
		CHANNEL_LFE: 5,
	});
	const MAX_VOLUME_CHANNELS = 8;
	const NUMBER_OF_WEB_CHANNELS = Object.keys(WebChannel).length;
	const MAX_SAFE_FLOAT32_INTEGER = 16777215;
	/**
	 * JS pair of a Kaizen AudioBus.
	 * @typedef {{
	 *   getId: function(): number,
	 *   getInputNode: function(): !AudioNode,
	 *   getOutputNode: function(): !AudioNode,
	 *   setVolumeDb: function (KaizenBus?): void,
	 *   setSend: function(KaizenBus?): void,
	 *   setMute: function(boolean): void,
	 *   setSoloMute: function(boolean): void,
	 *   clear: function(): void,
	 * }}
	 */
	var KaizenBus; // eslint-disable-line no-var, no-unassigned-vars
	/**
	 * A mixing sub-graph to control KaizenPlayback volume outputs to a KaizenBus.
	 * @typedef {{
	 *   setVolume: function(!Float32Array): void,
	 *   getInputNode: function(): !AudioNode,
	 *   getOutputNode: function(): !AudioNode,
	 *   clear: function(): void,
	 * }}
	 */
	var KaizenVolumeMixer; // eslint-disable-line no-var, no-unassigned-vars
	/**
	 * JS pair of a Kaizen AudioStream/AudioSample.
	 * @typedef {{
	 *   loopBegin: number,
	 *   loopEnd: number,
	 *   loopMode: LoopMode,
	 *   length: number,
	 *   getBuffer: function(): !AudioBuffer,
	 * }}
	 */
	var KaizenStream; // eslint-disable-line no-var, no-unassigned-vars
	/**
	 * JS pair of a Kaizen AudioStreamPlayback/AudioSamplePlayback.
	 * @typedef {{
	 *   stop: function(): void,
	 *   start: function(number): void,
	 *   pause: function(boolean): void,
	 *   setVolumes: function(Array<number>, !Float32Array),
	 *   setPitchScale: function(number): void,
	 *   getPlaybackPosition: function(): number,
	 * }}
	 */
	var KaizenPlayback; // eslint-disable-line no-var, no-unassigned-vars

	/** @type {KaizenBus?} */
	let busSolo = null;
	/** @type {Array<KaizenBus>} */
	let buses = [];
	/** @type {Map<string, KaizenStream>} */
	const streams = new Map();
	/** @type {Map<string, KaizenPlayback>} */
	const playbacks = new Map();

	/** @type {?function(number): void}*/
	let finishedCallback = null;

	/**
	 * @param {?function(number): void} callback
	 */
	function setFinishedCallback(callback) {
		finishedCallback = callback;
	}

	/**
	 * @param {number} db
	 * @return {number}
	 */
	function db_to_linear(db) {
		return Math.exp(db * 0.11512925464970228420089957273422); // eslint-disable-line no-loss-of-precision
	}

	/**
	 * @return {!KaizenBus}
	 */
	function createBus() {
		/** @type {!GainNode} */ const muteNode = ctx.createGain();
		/** @type {!GainNode} */ const gainNode = ctx.createGain();
		/** @type {!GainNode} */ const soloNode = ctx.createGain();
		gainNode.connect(soloNode).connect(muteNode);

		const bus = {};
		/** @return {!AudioNode} */ bus.getInputNode = () => gainNode;
		/** @return {!AudioNode} */ bus.getOutputNode = () => muteNode;
		/** @return {number} */
		bus.getId = function () {
			return buses.indexOf(bus);
		};
		/** @param {number} val */
		bus.setVolumeDb = function (val) {
			const linear = db_to_linear(val);
			if (isFinite(linear)) {
				gainNode.gain.value = linear;
			}
		};
		/** @param {?KaizenBus} val */
		bus.setSend = function (val) {
			if (val == null) {
				if (bus.getId() != 0) {
					KaizenRuntime.error(`Cannot set bus send to null when the bus not at index 0 (current index: ${bus.getId()})`);
					return;
				}
				bus.getOutputNode().connect(ctx.destination);
			} else {
				bus.getOutputNode().disconnect();
				bus.getOutputNode().connect(val.getInputNode());
			}
		};
		/** @param {boolean} enable */
		bus.setMute = function (enable) {
			muteNode.gain.value = enable ? 0 : 1;
		};
		/** @param {boolean} enable */
		bus.setSoloMute = function (enable) {
			soloNode.gain.value = enable ? 0 : 1;
		};
		bus.clear = function () {
			bus.getInputNode().disconnect();
			bus.getOutputNode().disconnect();
		};
		buses.push(bus);
		return bus;
	}
	/**
	 * @param {number} index
	 * @return {?KaizenBus}
	 */
	function getBus(index) {
		return buses[index] ?? null;
	}

	/**
	 * @param {number} bus
	 * @return {!KaizenVolumeMixer}
	 */
	function createVolumeMixer(bus) {
		/** @type {!ChannelSplitterNode} */ const splitter = ctx.createChannelSplitter(NUMBER_OF_WEB_CHANNELS);
		/** @type {!Array<!GainNode>} */ const channels = [];
		/** @type {!ChannelMergerNode} */ const merger = ctx.createChannelMerger(NUMBER_OF_WEB_CHANNELS);
		Object.values(WebChannel).forEach((idx) => {
			const ch = ctx.createGain();
			splitter.connect(ch, idx).connect(merger, 0, idx);
			channels[idx] = ch;
		});
		merger.connect(getBus(bus).getInputNode());

		const node = {};
		/** @return {!AudioNode} */ node.getInputNode = () => splitter;
		/** @return {!AudioNode} */ node.getOutputNode = () => merger;
		/** @param {!Float32Array} volume */
		node.setVolume = function (volume) {
			if (volume.length !== MAX_VOLUME_CHANNELS) {
				KaizenRuntime.error(`Volume length isn't "${MAX_VOLUME_CHANNELS}", is ${volume.length} instead`);
				return;
			}
			channels[WebChannel.CHANNEL_L].gain.value = volume[KaizenChannel.CHANNEL_L] ?? 0;
			channels[WebChannel.CHANNEL_R].gain.value = volume[KaizenChannel.CHANNEL_R] ?? 0;
			channels[WebChannel.CHANNEL_SL].gain.value = volume[KaizenChannel.CHANNEL_SL] ?? 0;
			channels[WebChannel.CHANNEL_SR].gain.value = volume[KaizenChannel.CHANNEL_SR] ?? 0;
			channels[WebChannel.CHANNEL_C].gain.value = volume[KaizenChannel.CHANNEL_C] ?? 0;
			channels[WebChannel.CHANNEL_LFE].gain.value = volume[KaizenChannel.CHANNEL_LFE] ?? 0;
		};
		node.clear = function () {
			splitter.disconnect();
			channels.forEach((ch) => {
				ch.disconnect();
			});
			merger.disconnect();
		};
		return node;
	}
	/**
	 * @param {string} streamId
	 * @param {!AudioBuffer} sourceBuffer
	 * @param {{
	 *   loopMode: (LoopMode|undefined),
	 *   loopBegin: (number|undefined),
	 *   loopEnd: (number|undefined),
	 * }=} options
	 * @return {!KaizenStream}
	 */
	function createStream(streamId, sourceBuffer, options = {}) {
		const stream = {
			/** @type {LoopMode} */ loopMode: options.loopMode ?? LoopMode.DISABLED,
			/** @type {number} */ loopBegin: options.loopBegin ?? 0,
			/** @type {number} */ loopEnd: options.loopEnd ?? 0,
			/** @type {number} */ length: sourceBuffer.length,
		};
		/**
		 * The same AudioBuffer can be used by multiple AudioBufferSourceNodes.
		 * @return {!AudioBuffer}
		 */
		stream.getBuffer = function () {
			return sourceBuffer;
		};
		streams.set(streamId, stream);
		return stream;
	}
	/**
	 * @param {string} id
	 * @return {!KaizenStream}
	 */
	function getStream(id) {
		return streams.get(id) ?? null;
	}
	/** @param {string} id */
	function removeStream(id) {
		streams.delete(id);
	}

	/**
	 * @param {string} playbackId
	 * @return {!KaizenPlayback}
	 */
	function createPlayback(playbackId, streamId) {
		/** @type {number} */ const sampleRate = ctx.sampleRate;
		const output = /** @struct */ {
			/** @type {!Map<number, KaizenVolumeMixer|null>} */
			mixers: new Map(),
			/** @type {!Float32Array} */
			volumes: new Float32Array(0),
		};
		const {
			/** @type {number} */ length,
			/** @type {LoopMode} */ loopMode,
			/** @type {number} */ loopBegin,
			/** @type {number} */ loopEnd,
		} = getStream(streamId);
		/** @type {boolean} */ const looping = loopMode !== LoopMode.DISABLED;
		/** @type {boolean} */ let started = false;
		/** @type {boolean} */ let paused = false;
		/** @type {number} */ let processed = 0;
		/** @type {number} */ let pitchScale = 1;
		/** @type {?AudioBufferSourceNode} */ let source = null;
		/** @type {?AudioWorkletNode} */ let worklet = null;
		/** @type {number} */ let workletId = 0;

		const node = {};
		function attachWorklet() {
			if (!source) {
				return; // Already stopped.
			}
			if (audioPositionWorkletNodes.length > 0) {
				worklet = audioPositionWorkletNodes.pop();
			} else {
				worklet = new AudioWorkletNode(/** @type {!AudioContext} */ (ctx), 'kaizen-position-reporting-processor');
			}
			const end = loopEnd ? Math.min(loopEnd, length) : length;
			worklet.port.onmessage = (event) => {
				const [id, data] = event.data;
				if (id != workletId) {
					return;
				}
				processed += data;
				if (processed > end) {
					processed -= end;
					if (looping) {
						processed += loopBegin;
					}
				}
			};
			worklet.parameters.get('id').setValueAtTime(workletId, ctx.currentTime);
			worklet.parameters.get('scale').setValueAtTime(pitchScale, ctx.currentTime);
			source.connect(worklet);
		}
		/** @param {!Array<number>} newBuses */
		function compareOutputs(newBuses) {
			const keys = Array.from(output.mixers.keys());
			if (keys.length != newBuses.length) {
				return false;
			}
			return newBuses.find((b) => !keys.includes(b)) === undefined;
		}
		/**
		 * @param {!Array<number>} newBuses
		 * @param {!Float32Array} volumes
		 */
		function setVolumes(newBuses, volumes) {
			if (!compareOutputs(newBuses)) {
				for (const mixer of output.mixers.values()) {
					if (!mixer) {
						continue;
					}
					if (source) {
						source.disconnect(mixer.getInputNode());
					}
					mixer.clear();
				}
				output.mixers.clear();
			}
			output.volumes = volumes;
			for (let idx = 0; idx < newBuses.length; idx++) {
				const bus = newBuses[idx];
				let mixer = output.mixers.get(bus);
				if (!mixer) {
					mixer = createVolumeMixer(bus);
					if (source) {
						source.connect(mixer.getInputNode());
					}
					output.mixers.set(bus, mixer);
				}
				const size = MAX_VOLUME_CHANNELS;
				mixer.setVolume(volumes.slice(idx * size, (idx * size) + size));
			}
		}
		function clearSource() {
			if (source) {
				source.onended = null;
				source.stop();
				source.disconnect();
				source = null;
			}
			output.mixers.forEach(function (mixer, key, map) {
				if (mixer) {
					if (source) {
						source.disconnect(mixer.getInputNode());
					}
					mixer.clear();
				}
				map.set(key, null);
			});
			if (worklet) {
				worklet.disconnect();
				worklet.port.onmessage = null;
				audioPositionWorkletNodes.push(worklet);
				worklet = null;
			}
			started = false;
			paused = false;
			processed = 0;
		}
		function createSource() {
			clearSource();

			// Source
			source = ctx.createBufferSource();
			source.buffer = getStream(streamId).getBuffer();
			source.onended = (_) => {
				if (paused) {
					return;
				}
				node.stop();
				if (!finishedCallback) {
					playbacks.delete(playbackId); // Cleanup after ourselves.
					return;
				}
				const cstr = KaizenRuntime.allocString(playbackId);
				finishedCallback(cstr);
				KaizenRuntime.free(cstr);
			};
			if (looping) {
				source.loop = true;
				source.loopStart = loopBegin / sampleRate;
				source.loopEnd = (loopEnd ? loopEnd : length) / sampleRate;
			}
			node.setPitchScale(pitchScale);
			node.setVolumes(Array.from(output.mixers.keys()), output.volumes);

			// Worklet
			workletId = Math.floor(Math.random() * MAX_SAFE_FLOAT32_INTEGER);
			if (audioPositionWorkletPromise) { // Unavailable outside secure contexts.
				audioPositionWorkletPromise.then(attachWorklet).catch((err) => {
					const newErr = new Error('Failed to create PositionWorklet.');
					newErr.cause = err;
					KaizenRuntime.error(newErr);
				});
			}
		}
		/** @param {number} startPosition */
		function start(startPosition) {
			if (started) {
				return;
			}
			createSource();
			processed = startPosition * sampleRate;
			source.start(0, startPosition);
			started = true;
			paused = false;
		}
		/** @param {number} newPitchScale */
		function setPitchScale(newPitchScale) {
			pitchScale = newPitchScale;
			if (!source || paused) {
				return;
			}
			source.playbackRate.value = pitchScale;
			if (worklet) {
				worklet.parameters.get('scale').setValueAtTime(pitchScale, ctx.currentTime);
			}
		};
		/** @param {boolean} enable */
		function pause(enable) {
			if (paused == enable) {
				return;
			}
			paused = enable;
			if (!started || !source) {
				return;
			}
			if (enable) {
				source.playbackRate.value = 0;
				if (worklet) {
					worklet.parameters.get('scale').setValueAtTime(0, ctx.currentTime);
				}
			} else {
				setPitchScale(pitchScale);
			}
		};
		/** @return {number} */
		function getPlaybackPosition() {
			return processed / sampleRate;
		}
		node.stop = clearSource;
		node.start = start;
		node.pause = pause;
		node.setVolumes = setVolumes;
		node.setPitchScale = setPitchScale;
		node.getPlaybackPosition = getPlaybackPosition;
		playbacks.set(playbackId, node);
		return node;
	}
	/**
	 * @param {string} id
	 * @return {?KaizenPlayback}
	 */
	function getPlayback(id) {
		return playbacks.get(id) ?? null;
	}
	/** @param {string} id */
	function removePlayback(id) {
		const playback = playbacks.get(id) ?? null;
		if (playback) {
			playback.stop();
			playbacks.delete(id);
		}
	}
	/** @param {number} count */
	function setBusCount(count) {
		if (count < 1) {
			KaizenRuntime.error('Invalid bus index, driver bug');
			return;
		}
		if (count === buses.length) {
			return;
		}
		if (count < buses.length) {
			// TODO: what to do with nodes connected to the deleted buses?
			const deletedBuses = buses.slice(count);
			for (let i = 0; i < deletedBuses.length; i++) {
				const deletedBus = deletedBuses[i];
				deletedBus.clear();
			}
			buses = buses.slice(0, count);
			return;
		}
		for (let i = buses.length; i < count; i++) {
			const bus = createBus();
			if (i === 0) {
				bus.setSend(null);
			} else {
				bus.setSend(getBus(0));
			}
		}
	}
	/**
	 * @param {number} fromIndex
	 * @param {number} toIndex
	 */
	function moveBus(fromIndex, toIndex) {
		if (fromIndex < 1 || toIndex < 1) {
			KaizenRuntime.error('Invalid bus index, driver bug');
			return;
		}
		const movedBus = getBus(fromIndex);
		if (!movedBus) {
			return;
		}
		const newBuses = buses.filter((_, i) => i !== fromIndex);
		// Inserts at index.
		newBuses.splice(toIndex - 1, 0, movedBus);
		buses = newBuses;
	}
	/** @param {number} index */
	function addBusAt(index) {
		if (index < 1) {
			KaizenRuntime.error('Invalid bus index, driver bug');
			return;
		}
		const bus = createBus();
		if (index !== bus.getId()) {
			moveBus(bus.getId(), index);
		}
		if (bus.getId() === 0) {
			bus.setSend(null);
		} else {
			bus.setSend(getBus(0));
		}
	}
	/** @param {number} index */
	function removeBus(index) {
		if (index < 1) {
			KaizenRuntime.error('Invalid bus index, driver bug');
			return;
		}
		const bus = getBus(index);
		if (!bus) {
			return;
		}
		bus.clear();
		buses = buses.filter((v) => v !== bus);
	}
	/**
	 * @param {number} busIndex
	 * @param {boolean} enable
	 */
	function setBusSolo(busIndex, enable) {
		const bus = getBus(busIndex);
		if (!bus) {
			return;
		}
		const current = busSolo;
		if (enable && current != bus) {
			busSolo = bus;
			bus.setSoloMute(false);
			buses.filter((o) => o !== bus).forEach((b) => {
				b.setSoloMute(true);
			});
		} else if (!enable && current == bus) {
			busSolo = null;
			buses.forEach((b) => {
				b.setSoloMute(false);
			});
		}
	}
	function clear() {
		audioPositionWorkletPromise = null;
		playbacks.forEach((p) => p.stop());
		playbacks.clear();
		streams.clear();
		buses.splice(0, buses.length).forEach((b) => b.clear());
		audioPositionWorkletNodes.splice(0, audioPositionWorkletNodes.length);
		ctx = null;
	}
	const driver = /** @struct */ {
		setFinishedCallback,
		clear,
		// Streams
		createStream,
		getStream,
		removeStream,
		// Playbacks
		createPlayback,
		getPlayback,
		removePlayback,
		// Buses
		getBus,
		setBusSolo,
		setBusCount,
		moveBus,
		addBusAt,
		removeBus,
		// So they survive pre-stripping.
		_: { KaizenBus, KaizenVolumeMixer, KaizenStream, KaizenPlayback },
	};
	return driver;
};

const _KaizenAudio = {
	$KaizenAudio__deps: ['$KaizenRuntime', '$KaizenOS'],
	$KaizenAudio: {
		/** @type {AudioContext} */
		ctx: null,
		input: null,
		driver: null,
		sample_driver: null,
		interval: 0,

		KaizenSampleDriver,
		init: function (mix_rate, latency, onstatechange, onlatencyupdate) {
			const opts = {};
			// If mix_rate is 0, let the browser choose.
			if (mix_rate) {
				KaizenAudio.sampleRate = mix_rate;
				opts['sampleRate'] = mix_rate;
			}
			// Do not specify, leave 'interactive' for good performance.
			// opts['latencyHint'] = latency / 1000;
			const ctx = new (window.AudioContext || window.webkitAudioContext)(opts);
			KaizenAudio.ctx = ctx;
			ctx.onstatechange = function () {
				let state = 0;
				switch (ctx.state) {
				case 'suspended':
					state = 0;
					break;
				case 'running':
					state = 1;
					break;
				case 'closed':
					state = 2;
					break;
				default:
					// Do nothing.
				}
				onstatechange(state);
			};
			ctx.onstatechange(); // Immediately notify state.
			// Update computed latency
			KaizenAudio.interval = setInterval(function () {
				let computed_latency = 0;
				if (ctx.baseLatency) {
					computed_latency += KaizenAudio.ctx.baseLatency;
				}
				if (ctx.outputLatency) {
					computed_latency += KaizenAudio.ctx.outputLatency;
				}
				onlatencyupdate(computed_latency);
			}, 1000);
			KaizenOS.atexit(KaizenAudio.close_async);

			// Initialize sample driver.
			const path = KaizenConfig.locate_file('kaizen.audio.position.worklet.js');
			KaizenAudio.sample_driver = KaizenAudio.KaizenSampleDriver(ctx, path);

			return ctx.destination.channelCount;
		},

		create_input: function (callback) {
			if (KaizenAudio.input) {
				return 0; // Already started.
			}
			function gotMediaInput(stream) {
				try {
					KaizenAudio.input = KaizenAudio.ctx.createMediaStreamSource(stream);
					callback(KaizenAudio.input);
				} catch (e) {
					KaizenRuntime.error('Failed creating input.', e);
				}
			}
			if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
				navigator.mediaDevices.getUserMedia({
					'audio': true,
				}).then(gotMediaInput, function (e) {
					KaizenRuntime.error('Error getting user media.', e);
				});
			} else {
				if (!navigator.getUserMedia) {
					navigator.getUserMedia = navigator.webkitGetUserMedia || navigator.mozGetUserMedia;
				}
				if (!navigator.getUserMedia) {
					KaizenRuntime.error('getUserMedia not available.');
					return 1;
				}
				navigator.getUserMedia({
					'audio': true,
				}, gotMediaInput, function (e) {
					KaizenRuntime.print(e);
				});
			}
			return 0;
		},

		close_async: function (resolve, reject) {
			const ctx = KaizenAudio.ctx;
			KaizenAudio.ctx = null;
			// Audio was not initialized.
			if (!ctx) {
				resolve();
				return;
			}
			// Clear audio driver
			if (KaizenAudio.sample_driver) {
				KaizenAudio.sample_driver.clear();
				KaizenAudio.sample_driver = null;
			}
			// Remove latency callback
			if (KaizenAudio.interval) {
				clearInterval(KaizenAudio.interval);
				KaizenAudio.interval = 0;
			}
			// Disconnect input, if it was started.
			if (KaizenAudio.input) {
				KaizenAudio.input.disconnect();
				KaizenAudio.input = null;
			}
			// Disconnect output
			let closed = Promise.resolve();
			if (KaizenAudio.driver) {
				closed = KaizenAudio.driver.close();
			}
			closed.then(function () {
				return ctx.close();
			}).then(function () {
				ctx.onstatechange = null;
				resolve();
			}).catch(function (e) {
				ctx.onstatechange = null;
				KaizenRuntime.error('Error closing AudioContext', e);
				resolve();
			});
		},
	},

	kaizen_audio_is_available__sig: 'i',
	kaizen_audio_is_available__proxy: 'sync',
	kaizen_audio_is_available: function () {
		if (!(window.AudioContext || window.webkitAudioContext)) {
			return 0;
		}
		return 1;
	},

	kaizen_audio_has_worklet__proxy: 'sync',
	kaizen_audio_has_worklet__sig: 'i',
	kaizen_audio_has_worklet: function () {
		return KaizenAudio.ctx && KaizenAudio.ctx.audioWorklet ? 1 : 0;
	},

	kaizen_audio_has_script_processor__proxy: 'sync',
	kaizen_audio_has_script_processor__sig: 'i',
	kaizen_audio_has_script_processor: function () {
		return KaizenAudio.ctx && KaizenAudio.ctx.createScriptProcessor ? 1 : 0;
	},

	kaizen_audio_init__proxy: 'sync',
	kaizen_audio_init__sig: 'iiiii',
	kaizen_audio_init: function (
		p_mix_rate,
		p_latency,
		p_state_change,
		p_latency_update
	) {
		const statechange = KaizenRuntime.get_func(p_state_change);
		const latencyupdate = KaizenRuntime.get_func(p_latency_update);
		const mix_rate = KaizenRuntime.getHeapValue(p_mix_rate, 'i32');
		const channels = KaizenAudio.init(
			mix_rate,
			p_latency,
			statechange,
			latencyupdate
		);
		KaizenRuntime.setHeapValue(p_mix_rate, KaizenAudio.ctx.sampleRate, 'i32');
		return channels;
	},

	kaizen_audio_resume__proxy: 'sync',
	kaizen_audio_resume__sig: 'v',
	kaizen_audio_resume: function () {
		if (KaizenAudio.ctx && KaizenAudio.ctx.state !== 'running') {
			KaizenAudio.ctx.resume();
		}
	},

	kaizen_audio_input_start__proxy: 'sync',
	kaizen_audio_input_start__sig: 'i',
	kaizen_audio_input_start: function () {
		return KaizenAudio.create_input(function (input) {
			input.connect(KaizenAudio.driver.get_node());
		});
	},

	kaizen_audio_input_stop__proxy: 'sync',
	kaizen_audio_input_stop__sig: 'v',
	kaizen_audio_input_stop: function () {
		if (KaizenAudio.input) {
			const tracks = KaizenAudio.input['mediaStream']['getTracks']();
			for (let i = 0; i < tracks.length; i++) {
				tracks[i]['stop']();
			}
			KaizenAudio.input.disconnect();
			KaizenAudio.input = null;
		}
	},
};

autoAddDeps(_KaizenAudio, '$KaizenAudio');
mergeInto(LibraryManager.library, _KaizenAudio);

/**
 * The audio sample driver used by AudioStreamPlayer with playback_type = Sample.
 */
const _KaizenAudioSampleDriver = {
	$KaizenAudioSampleDriver__deps: ['$KaizenAudio', '$KaizenRuntime'],
	$KaizenAudioSampleDriver: {},

	kaizen_audio_sample_stream_is_registered__proxy: 'sync',
	kaizen_audio_sample_stream_is_registered__sig: 'ii',
	kaizen_audio_sample_stream_is_registered: function (p_id) {
		if (!KaizenAudio.sample_driver) {
			return 0;
		}
		return KaizenAudio.sample_driver.getStream(KaizenRuntime.parseString(p_id)) ? 1 : 0;
	},

	kaizen_audio_sample_register_stream__proxy: 'sync',
	kaizen_audio_sample_register_stream__sig: 'viiiiii',
	kaizen_audio_sample_register_stream: function (p_id, p_frames, p_frames_count, p_loop_mode, p_loop_begin, p_loop_end) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		const BYTES_PER_FLOAT32 = 4;
		const sid = KaizenRuntime.parseString(p_id);
		const loopMode = KaizenRuntime.parseString(p_loop_mode);
		const numberOfChannels = 2;
		const sampleRate = KaizenAudio.ctx.sampleRate;

		// AudioBuffer Cant's copy from a SharedArrayBuffer-backed view.
		const sliceOrSub = HEAPF32.buffer instanceof ArrayBuffer ? KaizenRuntime.heapSub : KaizenRuntime.heapSlice;
		const subLeft = sliceOrSub(HEAPF32, p_frames, p_frames_count);
		const subRight = sliceOrSub(HEAPF32, p_frames + p_frames_count * BYTES_PER_FLOAT32, p_frames_count);
		const audioBuffer = KaizenAudio.ctx.createBuffer(numberOfChannels, p_frames_count, sampleRate);
		audioBuffer.copyToChannel(subLeft, 0, 0);
		audioBuffer.copyToChannel(subRight, 1, 0);

		const opts = {
			/** @export */ loopBegin: p_loop_begin,
			/** @export */ loopEnd: p_loop_end,
			/** @export */ loopMode,
		};
		KaizenAudio.sample_driver.createStream(sid, audioBuffer, opts);
	},

	kaizen_audio_sample_unregister_stream__proxy: 'sync',
	kaizen_audio_sample_unregister_stream__sig: 'vi',
	kaizen_audio_sample_unregister_stream: function (p_id) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		KaizenAudio.sample_driver.removeStream(KaizenRuntime.parseString(p_id));
	},

	kaizen_audio_sample_start__proxy: 'sync',
	kaizen_audio_sample_start__sig: 'viiiifi',
	kaizen_audio_sample_start: function (p_playback, p_stream, p_bus, p_offset, p_pitch_scale, p_volume) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		const sid = KaizenRuntime.parseString(p_stream);
		if (!KaizenAudio.sample_driver.getStream(sid)) {
			return;
		}
		const pid = KaizenRuntime.parseString(p_playback);
		KaizenAudio.sample_driver.removePlayback(pid);
		const playback = KaizenAudio.sample_driver.createPlayback(pid, sid);
		const volume = KaizenRuntime.heapSlice(HEAPF32, p_volume, 8); // Slice (copy) since it will be stored.
		playback.setVolumes([p_bus], volume);
		playback.setPitchScale(p_pitch_scale);
		playback.start(p_offset);
	},

	kaizen_audio_sample_stop__proxy: 'sync',
	kaizen_audio_sample_stop__sig: 'vi',
	kaizen_audio_sample_stop: function (p_id) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		KaizenAudio.sample_driver.removePlayback(KaizenRuntime.parseString(p_id));
	},

	kaizen_audio_sample_set_pause__proxy: 'sync',
	kaizen_audio_sample_set_pause__sig: 'vii',
	kaizen_audio_sample_set_pause: function (p_id, p_pause) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		const playback = KaizenAudio.sample_driver.getPlayback(KaizenRuntime.parseString(p_id));
		if (playback) {
			playback.pause(Boolean(p_pause));
		}
	},

	kaizen_audio_sample_is_active__proxy: 'sync',
	kaizen_audio_sample_is_active__sig: 'ii',
	kaizen_audio_sample_is_active: function (p_id) {
		if (!KaizenAudio.sample_driver) {
			return 0;
		}
		return KaizenAudio.sample_driver.getPlayback(KaizenRuntime.parseString(p_id)) ? 1 : 0;
	},

	kaizen_audio_get_sample_playback_position__proxy: 'sync',
	kaizen_audio_get_sample_playback_position__sig: 'di',
	kaizen_audio_get_sample_playback_position: function (p_id) {
		if (!KaizenAudio.sample_driver) {
			return 0;
		}
		const playback = KaizenAudio.sample_driver.getPlayback(KaizenRuntime.parseString(p_id));
		return playback ? playback.getPlaybackPosition() : 0;
	},

	kaizen_audio_sample_update_pitch_scale__proxy: 'sync',
	kaizen_audio_sample_update_pitch_scale__sig: 'vii',
	kaizen_audio_sample_update_pitch_scale: function (p_id, p_pitch_scale) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		const playback = KaizenAudio.sample_driver.getPlayback(KaizenRuntime.parseString(p_id));
		if (playback) {
			playback.setPitchScale(p_pitch_scale);
		}
	},

	kaizen_audio_sample_set_volumes_linear__proxy: 'sync',
	kaizen_audio_sample_set_volumes_linear__sig: 'viiiii',
	kaizen_audio_sample_set_volumes_linear: function (p_id, p_buses, p_buses_size, p_volume, p_volume_size) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		const playback = KaizenAudio.sample_driver.getPlayback(KaizenRuntime.parseString(p_id));
		if (!playback) {
			return;
		}
		const busesIndexes = Array.from(KaizenRuntime.heapSub(HEAP32, p_buses, p_buses_size));
		const volumes = KaizenRuntime.heapSlice(HEAPF32, p_volume, p_volume_size); // Slice (copy) since it will be stored.
		playback.setVolumes(busesIndexes, volumes);
	},

	kaizen_audio_sample_bus_set_count__proxy: 'sync',
	kaizen_audio_sample_bus_set_count__sig: 'vi',
	kaizen_audio_sample_bus_set_count: function (p_count) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		KaizenAudio.sample_driver.setBusCount(p_count);
	},

	kaizen_audio_sample_bus_remove__proxy: 'sync',
	kaizen_audio_sample_bus_remove__sig: 'vi',
	kaizen_audio_sample_bus_remove: function (p_index) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		KaizenAudio.sample_driver.removeBus(p_index);
	},

	kaizen_audio_sample_bus_add__proxy: 'sync',
	kaizen_audio_sample_bus_add__sig: 'vi',
	kaizen_audio_sample_bus_add: function (p_index) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		KaizenAudio.sample_driver.addBusAt(p_index);
	},

	kaizen_audio_sample_bus_move__proxy: 'sync',
	kaizen_audio_sample_bus_move__sig: 'vii',
	kaizen_audio_sample_bus_move: function (p_from, p_to) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		KaizenAudio.sample_driver.moveBus(p_from, p_to);
	},

	kaizen_audio_sample_bus_set_send__proxy: 'sync',
	kaizen_audio_sample_bus_set_send__sig: 'vii',
	kaizen_audio_sample_bus_set_send: function (p_index, p_send_index) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		const bus = KaizenAudio.sample_driver.getBus(p_index);
		if (!bus) {
			return;
		}
		let target = KaizenAudio.sample_driver.getBus(p_send_index);
		if (target == null) { // Send to master.
			target = KaizenAudio.sample_driver.getBus(0);
		}
		bus.setSend(target);
	},

	kaizen_audio_sample_bus_set_volume_db__proxy: 'sync',
	kaizen_audio_sample_bus_set_volume_db__sig: 'vii',
	kaizen_audio_sample_bus_set_volume_db: function (p_bus, p_volume_db) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		const bus = KaizenAudio.sample_driver.getBus(p_bus);
		if (!bus) {
			return;
		}
		bus.setVolumeDb(p_volume_db);
	},

	kaizen_audio_sample_bus_set_solo__proxy: 'sync',
	kaizen_audio_sample_bus_set_solo__sig: 'vii',
	kaizen_audio_sample_bus_set_solo: function (p_bus, p_enable) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		KaizenAudio.sample_driver.setBusSolo(p_bus, p_enable);
	},

	kaizen_audio_sample_bus_set_mute__proxy: 'sync',
	kaizen_audio_sample_bus_set_mute__sig: 'vii',
	kaizen_audio_sample_bus_set_mute: function (p_bus, p_enable) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		const bus = KaizenAudio.sample_driver.getBus(p_bus);
		if (!bus) {
			return;
		}
		bus.setMute(Boolean(p_enable));
	},

	kaizen_audio_sample_set_finished_callback__proxy: 'sync',
	kaizen_audio_sample_set_finished_callback__sig: 'vi',
	kaizen_audio_sample_set_finished_callback: function (p_callback) {
		if (!KaizenAudio.sample_driver) {
			return;
		}
		KaizenAudio.sample_driver.setFinishedCallback(KaizenRuntime.get_func(p_callback));
	},
};

autoAddDeps(_KaizenAudioSampleDriver, '$KaizenAudioSampleDriver');
mergeInto(LibraryManager.library, _KaizenAudioSampleDriver);

/**
 * The AudioWorklet API driver, used when threads are available.
 */
const KaizenAudioWorklet = {
	$KaizenAudioWorklet__deps: ['$KaizenAudio', '$KaizenConfig'],
	$KaizenAudioWorklet: {
		promise: null,
		worklet: null,
		ring_buffer: null,

		create: function (channels) {
			const path = KaizenConfig.locate_file('kaizen.audio.worklet.js');
			KaizenAudioWorklet.promise = KaizenAudio.ctx.audioWorklet
				.addModule(path)
				.then(function () {
					KaizenAudioWorklet.worklet = new AudioWorkletNode(
						KaizenAudio.ctx,
						'kaizen-processor',
						{
							outputChannelCount: [channels],
						}
					);
					return Promise.resolve();
				});
			KaizenAudio.driver = KaizenAudioWorklet;
		},

		start: function (in_buf, out_buf, state) {
			KaizenAudioWorklet.promise.then(function () {
				const node = KaizenAudioWorklet.worklet;
				node.connect(KaizenAudio.ctx.destination);
				node.port.postMessage({
					'cmd': 'start',
					'data': [state, in_buf, out_buf],
				});
				node.port.onmessage = function (event) {
					KaizenRuntime.error(event.data);
				};
			});
		},

		start_no_threads: function (
			p_out_buf,
			p_out_size,
			out_callback,
			p_in_buf,
			p_in_size,
			in_callback
		) {
			/** @constructor */
			function RingBuffer() {
				let wpos = 0;
				let rpos = 0;
				let pending_samples = 0;
				const wbuf = new Float32Array(p_out_size);

				function send(port) {
					if (pending_samples === 0) {
						return;
					}
					const buffer = KaizenRuntime.heapSub(HEAPF32, p_out_buf, p_out_size);
					const size = buffer.length;
					const tot_sent = pending_samples;
					out_callback(wpos, pending_samples);
					if (wpos + pending_samples >= size) {
						const high = size - wpos;
						wbuf.set(buffer.subarray(wpos, size));
						pending_samples -= high;
						wpos = 0;
					}
					if (pending_samples > 0) {
						wbuf.set(
							buffer.subarray(wpos, wpos + pending_samples),
							tot_sent - pending_samples
						);
					}
					port.postMessage({ 'cmd': 'chunk', 'data': wbuf.subarray(0, tot_sent) });
					wpos += pending_samples;
					pending_samples = 0;
				}
				this.receive = function (recv_buf) {
					const buffer = KaizenRuntime.heapSub(HEAPF32, p_in_buf, p_in_size);
					const from = rpos;
					let to_write = recv_buf.length;
					let high = 0;
					if (rpos + to_write >= p_in_size) {
						high = p_in_size - rpos;
						buffer.set(recv_buf.subarray(0, high), rpos);
						to_write -= high;
						rpos = 0;
					}
					if (to_write) {
						buffer.set(recv_buf.subarray(high, to_write), rpos);
					}
					in_callback(from, recv_buf.length);
					rpos += to_write;
				};
				this.consumed = function (size, port) {
					pending_samples += size;
					send(port);
				};
			}
			KaizenAudioWorklet.ring_buffer = new RingBuffer();
			KaizenAudioWorklet.promise.then(function () {
				const node = KaizenAudioWorklet.worklet;
				const buffer = KaizenRuntime.heapSlice(HEAPF32, p_out_buf, p_out_size);
				node.connect(KaizenAudio.ctx.destination);
				node.port.postMessage({
					'cmd': 'start_nothreads',
					'data': [buffer, p_in_size],
				});
				node.port.onmessage = function (event) {
					if (!KaizenAudioWorklet.worklet) {
						return;
					}
					if (event.data['cmd'] === 'read') {
						const read = event.data['data'];
						KaizenAudioWorklet.ring_buffer.consumed(
							read,
							KaizenAudioWorklet.worklet.port
						);
					} else if (event.data['cmd'] === 'input') {
						const buf = event.data['data'];
						if (buf.length > p_in_size) {
							KaizenRuntime.error('Input chunk is too big');
							return;
						}
						KaizenAudioWorklet.ring_buffer.receive(buf);
					} else {
						KaizenRuntime.error(event.data);
					}
				};
			});
		},

		get_node: function () {
			return KaizenAudioWorklet.worklet;
		},

		close: function () {
			return new Promise(function (resolve, reject) {
				if (KaizenAudioWorklet.promise === null) {
					return;
				}
				const p = KaizenAudioWorklet.promise;
				p.then(function () {
					KaizenAudioWorklet.worklet.port.postMessage({
						'cmd': 'stop',
						'data': null,
					});
					KaizenAudioWorklet.worklet.disconnect();
					KaizenAudioWorklet.worklet.port.onmessage = null;
					KaizenAudioWorklet.worklet = null;
					KaizenAudioWorklet.promise = null;
					resolve();
				}).catch(function (err) {
					// Aborted?
					KaizenRuntime.error(err);
				});
			});
		},
	},

	kaizen_audio_worklet_create__proxy: 'sync',
	kaizen_audio_worklet_create__sig: 'ii',
	kaizen_audio_worklet_create: function (channels) {
		try {
			KaizenAudioWorklet.create(channels);
		} catch (e) {
			KaizenRuntime.error('Error starting AudioDriverWorklet', e);
			return 1;
		}
		return 0;
	},

	kaizen_audio_worklet_start__proxy: 'sync',
	kaizen_audio_worklet_start__sig: 'viiiii',
	kaizen_audio_worklet_start: function (
		p_in_buf,
		p_in_size,
		p_out_buf,
		p_out_size,
		p_state
	) {
		const out_buffer = KaizenRuntime.heapSub(HEAPF32, p_out_buf, p_out_size);
		const in_buffer = KaizenRuntime.heapSub(HEAPF32, p_in_buf, p_in_size);
		const state = KaizenRuntime.heapSub(HEAP32, p_state, 4);
		KaizenAudioWorklet.start(in_buffer, out_buffer, state);
	},

	kaizen_audio_worklet_start_no_threads__proxy: 'sync',
	kaizen_audio_worklet_start_no_threads__sig: 'viiiiii',
	kaizen_audio_worklet_start_no_threads: function (
		p_out_buf,
		p_out_size,
		p_out_callback,
		p_in_buf,
		p_in_size,
		p_in_callback
	) {
		const out_callback = KaizenRuntime.get_func(p_out_callback);
		const in_callback = KaizenRuntime.get_func(p_in_callback);
		KaizenAudioWorklet.start_no_threads(
			p_out_buf,
			p_out_size,
			out_callback,
			p_in_buf,
			p_in_size,
			in_callback
		);
	},

	kaizen_audio_worklet_state_wait__sig: 'iiii',
	kaizen_audio_worklet_state_wait: function (
		p_state,
		p_idx,
		p_expected,
		p_timeout
	) {
		Atomics.wait(HEAP32, (p_state >> 2) + p_idx, p_expected, p_timeout);
		return Atomics.load(HEAP32, (p_state >> 2) + p_idx);
	},

	kaizen_audio_worklet_state_add__sig: 'iiii',
	kaizen_audio_worklet_state_add: function (p_state, p_idx, p_value) {
		return Atomics.add(HEAP32, (p_state >> 2) + p_idx, p_value);
	},

	kaizen_audio_worklet_state_get__sig: 'iii',
	kaizen_audio_worklet_state_get: function (p_state, p_idx) {
		return Atomics.load(HEAP32, (p_state >> 2) + p_idx);
	},
};

autoAddDeps(KaizenAudioWorklet, '$KaizenAudioWorklet');
mergeInto(LibraryManager.library, KaizenAudioWorklet);

/*
 * The ScriptProcessorNode API, used as a fallback if AudioWorklet is not available.
 */
const KaizenAudioScript = {
	$KaizenAudioScript__deps: ['$KaizenAudio'],
	$KaizenAudioScript: {
		script: null,

		create: function (buffer_length, channel_count) {
			KaizenAudioScript.script = KaizenAudio.ctx.createScriptProcessor(
				buffer_length,
				2,
				channel_count
			);
			KaizenAudio.driver = KaizenAudioScript;
			return KaizenAudioScript.script.bufferSize;
		},

		start: function (p_in_buf, p_in_size, p_out_buf, p_out_size, onprocess) {
			KaizenAudioScript.script.onaudioprocess = function (event) {
				// Read input
				const inb = KaizenRuntime.heapSub(HEAPF32, p_in_buf, p_in_size);
				const input = event.inputBuffer;
				if (KaizenAudio.input) {
					const inlen = input.getChannelData(0).length;
					for (let ch = 0; ch < 2; ch++) {
						const data = input.getChannelData(ch);
						for (let s = 0; s < inlen; s++) {
							inb[s * 2 + ch] = data[s];
						}
					}
				}

				// Let Kaizen process the input/output.
				onprocess();

				// Write the output.
				const outb = KaizenRuntime.heapSub(HEAPF32, p_out_buf, p_out_size);
				const output = event.outputBuffer;
				const channels = output.numberOfChannels;
				for (let ch = 0; ch < channels; ch++) {
					const data = output.getChannelData(ch);
					// Loop through samples and assign computed values.
					for (let sample = 0; sample < data.length; sample++) {
						data[sample] = outb[sample * channels + ch];
					}
				}
			};
			KaizenAudioScript.script.connect(KaizenAudio.ctx.destination);
		},

		get_node: function () {
			return KaizenAudioScript.script;
		},

		close: function () {
			return new Promise(function (resolve, reject) {
				KaizenAudioScript.script.disconnect();
				KaizenAudioScript.script.onaudioprocess = function () {};
				KaizenAudioScript.script = null;
				resolve();
			});
		},
	},

	kaizen_audio_script_create__proxy: 'sync',
	kaizen_audio_script_create__sig: 'iii',
	kaizen_audio_script_create: function (buffer_length, channel_count) {
		const buf_len = KaizenRuntime.getHeapValue(buffer_length, 'i32');
		try {
			const out_len = KaizenAudioScript.create(buf_len, channel_count);
			KaizenRuntime.setHeapValue(buffer_length, out_len, 'i32');
		} catch (e) {
			KaizenRuntime.error('Error starting AudioDriverScriptProcessor', e);
			return 1;
		}
		return 0;
	},

	kaizen_audio_script_start__proxy: 'sync',
	kaizen_audio_script_start__sig: 'viiiii',
	kaizen_audio_script_start: function (
		p_in_buf,
		p_in_size,
		p_out_buf,
		p_out_size,
		p_cb
	) {
		const onprocess = KaizenRuntime.get_func(p_cb);
		KaizenAudioScript.start(
			p_in_buf,
			p_in_size,
			p_out_buf,
			p_out_size,
			onprocess
		);
	},
};

autoAddDeps(KaizenAudioScript, '$KaizenAudioScript');
mergeInto(LibraryManager.library, KaizenAudioScript);
