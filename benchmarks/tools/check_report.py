#!/usr/bin/env python3
"""Validate Kaizen benchmark reports against the documented validity gate.

Usage:
    python3 benchmarks/tools/check_report.py REPORT.json[.gz] [REPORT.json[.gz] ...]

Each report is checked on its own. Reports that describe the same cell (same
scene, scene parameters, preset, configuration, engine content and runtime) are
also compared with each other, because determinism is a property of a cell, not
of a single run.

Exit codes:
    0  every checked report and cell passes
    1  at least one check failed
    2  a file could not be read or is not a benchmark report

This tool never runs the engine and never produces measurements. It only reads
the JSON written by benchmarks/rd_3d/scripts/bench_harness.gd. The rules it
enforces are the ones in docs/architecture/rendering-benchmark-scenes.md,
section 7.
"""

from __future__ import annotations

import gzip
import json
import sys

LOOP_FRAMES = 300
STABLE_WINDOW = 120
TOLERANCE = 0.01

# The only audio state a published cell may show. "Dummy" is selected by
# --audio-driver Dummy (and implied by --headless); it is not "no audio work": the dummy
# driver keeps its own thread, which wakes every buffer_frames/mix_rate (~10.7 ms at the
# default 512/48000) and mixes under the driver mutex. What a real driver adds on top is
# a *device* callback: the period is then set by the audio hardware, the thread priority by
# the backend, and an underrun turns into a burst. None of that is controlled by the
# benchmark, so it must not be free to differ between the two sides of a comparison.
AUDIO_ISOLATED_DRIVER = "Dummy"

# Stand-in for reports written before the harness recorded the audio state, so a cell key and
# any message built from it say what is missing instead of printing None.
AUDIO_NOT_RECORDED = "not recorded"

FAIL = "FAIL"
WARN = "WARN"
INFO = "INFO"


def _median(values):
    ordered = sorted(values)
    count = len(ordered)
    if count == 0:
        return 0.0
    middle = count // 2
    if count % 2 == 1:
        return float(ordered[middle])
    return (float(ordered[middle - 1]) + float(ordered[middle])) / 2.0


def _relative_spread(values):
    if not values:
        return 0.0
    low = float(min(values))
    high = float(max(values))
    reference = abs(_median(values))
    if reference == 0.0:
        return 0.0
    return (high - low) / reference


def _check_report(report):
    issues = []

    def fail(message):
        issues.append((FAIL, message))

    def warn(message):
        issues.append((WARN, message))

    def info(message):
        issues.append((INFO, message))

    if report.get("schema") != 1:
        fail("schema is %r, expected 1" % report.get("schema"))

    build = report.get("build") or {}
    if build.get("type") != "template":
        fail("build type is %r; published cells must come from an export template" % build.get("type"))
    arguments = list(build.get("flags") or []) + list(build.get("user_args") or [])
    if "--headless" in arguments:
        fail("run used --headless; presentation is bypassed")

    runtime = report.get("runtime") or {}
    if runtime.get("display_server") in (None, "", "headless"):
        fail("display server is %r; no rendering surface was used" % runtime.get("display_server"))
    if runtime.get("vsync_mode") != 0:
        fail("display vsync mode is %r, expected 0" % runtime.get("vsync_mode"))

    # The audio state is read from the runtime block, never from the invocation: Main::setup
    # consumes recognised engine switches (only unrecognised arguments reach main_args, and
    # --audio-driver has its own branch), so build.flags cannot show what was asked for. It is
    # a WARN and not a FAIL because the numbers are still the numbers - what is unprovable is
    # that two cells were measured under the same host conditions.
    if "audio_driver" not in runtime:
        warn("runtime does not record audio_driver; this run was measured with whatever audio "
             "state the host had, so it cannot be paired with a run that records a different one")
    elif str(runtime.get("audio_driver")) != AUDIO_ISOLATED_DRIVER:
        warn(f"audio_driver is {runtime.get('audio_driver')!r}, not {AUDIO_ISOLATED_DRIVER!r}: a "
             "live output device drives the audio mix from a hardware callback with a backend-chosen "
             "thread priority, so the mix thread competes with the render thread on host state the "
             "benchmark does not set; re-run with --audio-driver Dummy")

    if report.get("valid") is not True:
        fail("harness marked the run invalid: %s" % (report.get("invalid_reason") or "no reason given"))

    timing = report.get("timing") or {}
    sampled = timing.get("sampled_frames")
    warmup = timing.get("warmup_frames")
    first = timing.get("first_frame")
    last = timing.get("last_frame")

    if not isinstance(sampled, int) or sampled < LOOP_FRAMES:
        fail("sampled_frames is %r, expected an integer >= %d" % (sampled, LOOP_FRAMES))
    elif sampled % LOOP_FRAMES != 0:
        fail("sampled_frames is %d, not a multiple of the %d-frame loop" % (sampled, LOOP_FRAMES))

    if not isinstance(warmup, int) or warmup < LOOP_FRAMES:
        fail("warmup_frames is %r, expected an integer >= %d" % (warmup, LOOP_FRAMES))
    elif warmup % LOOP_FRAMES != 0:
        fail("warmup_frames is %d, not a multiple of the %d-frame loop" % (warmup, LOOP_FRAMES))

    if isinstance(first, int) and isinstance(last, int) and isinstance(sampled, int):
        if last != first + sampled - 1:
            fail("last_frame %d does not equal first_frame %d + sampled_frames %d - 1" % (last, first, sampled))

    if isinstance(first, int) and warmup is not None and first != warmup:
        fail("first_frame %d does not match warmup_frames %s" % (first, warmup))

    last_compile = timing.get("last_compile_frame")
    if isinstance(last_compile, int) and last_compile >= 0 and isinstance(first, int):
        if first - 1 - last_compile < STABLE_WINDOW:
            fail(
                "last pipeline compilation at frame %d is within %d frames of sampling start %d"
                % (last_compile, STABLE_WINDOW, first)
            )

    if timing.get("pso_in_sample") != 0:
        fail("pso_in_sample is %r, expected 0" % timing.get("pso_in_sample"))

    load_to_sample = timing.get("load_to_sample_ms")
    if not isinstance(load_to_sample, (int, float)) or isinstance(load_to_sample, bool):
        fail("timing.load_to_sample_ms is missing")
    elif load_to_sample <= 0:
        fail("timing.load_to_sample_ms is %r, expected a positive number" % load_to_sample)

    compilations = report.get("pipeline_compilations") or {}
    if "first_120" not in compilations or "total" not in compilations:
        fail("pipeline_compilations must contain first_120 and total")

    frames = report.get("frames") or {}
    frame_us = frames.get("frame_us")
    if not isinstance(frame_us, list):
        fail("frames.frame_us is missing")
    elif isinstance(sampled, int) and len(frame_us) != sampled:
        fail("frames.frame_us has %d entries, timing.sampled_frames is %d" % (len(frame_us), sampled))

    pso_per_frame = frames.get("pso_compilations")
    if not isinstance(pso_per_frame, list):
        fail("frames.pso_compilations is missing")
    else:
        if isinstance(sampled, int) and len(pso_per_frame) != sampled:
            fail(
                "frames.pso_compilations has %d entries, timing.sampled_frames is %d"
                % (len(pso_per_frame), sampled)
            )
        nonzero = sum(1 for value in pso_per_frame if int(value) != 0)
        if nonzero:
            fail("%d sampled frames report pipeline compilations" % nonzero)

    summary = report.get("summary") or {}
    if isinstance(sampled, int) and summary.get("frames") != sampled:
        fail("summary.frames is %r, timing.sampled_frames is %d" % (summary.get("frames"), sampled))

    for key, median_key in (
        ("viewport_gpu_ms", "gpu_ms_median"),
        ("viewport_cpu_ms", "render_cpu_ms_median"),
    ):
        values = frames.get(key)
        if not isinstance(values, list):
            continue
        if values and isinstance(sampled, int) and len(values) != sampled:
            fail(
                "frames.%s has %d entries, timing.sampled_frames is %d"
                % (key, len(values), sampled)
            )
        if not values and summary.get(median_key) is not None:
            fail("summary.%s has a value but frames.%s is empty" % (median_key, key))
        if values and summary.get(median_key) is None:
            fail("frames.%s has values but summary.%s is null" % (key, median_key))

    if not report.get("frame_hash"):
        fail("frame_hash is empty; frame determinism cannot be checked (do not use --bench-no-hash for a published cell)")

    settings = report.get("project_settings") or {}
    _compare_setting(fail, settings, runtime, "rendering/anti_aliasing/quality/msaa_3d", "msaa_3d")
    _compare_setting(fail, settings, runtime, "rendering/scaling_3d/mode", "scaling_3d_mode")
    _compare_setting(fail, settings, runtime, "rendering/scaling_3d/scale", "scaling_3d_scale")

    window_size = runtime.get("window_size")
    expected_width = settings.get("display/window/size/viewport_width")
    expected_height = settings.get("display/window/size/viewport_height")
    if isinstance(window_size, list) and len(window_size) == 2:
        if expected_width is not None and expected_height is not None:
            if window_size[0] != expected_width or window_size[1] != expected_height:
                fail(
                    "window size %dx%d differs from the project viewport %sx%s; the frame was rendered at another resolution"
                    % (window_size[0], window_size[1], expected_width, expected_height)
                )
    else:
        warn("runtime.window_size is missing or malformed")

    scene = report.get("scene") or {}
    scene_id = str(scene.get("id"))
    parameters = scene.get("parameters") or {}

    if scene_id == "S1":
        _check_s1(fail, warn, frames, parameters)
    elif scene_id == "S2":
        _check_s2(fail, warn, frames)
    elif scene_id == "S6":
        _check_s6(fail, warn, report, frames, parameters)
    elif scene_id == "S8":
        _check_s8(fail, warn, info, frames, parameters)
    elif scene_id not in ("S3", "S4", "S7"):
        info("unknown scene id %r; only the generic checks were applied" % scene_id)

    return issues


def _compare_setting(fail, settings, runtime, setting_key, runtime_key):
    if setting_key not in settings or runtime_key not in runtime:
        return
    expected = settings[setting_key]
    actual = runtime[runtime_key]
    if isinstance(expected, float) or isinstance(actual, float):
        if abs(float(expected) - float(actual)) > 1e-6:
            fail("%s is %r but the project pins %r" % (runtime_key, actual, expected))
    elif expected != actual:
        fail("%s is %r but the project pins %r" % (runtime_key, actual, expected))


def _check_s1(fail, warn, frames, parameters):
    box_count = parameters.get("box_count")
    primitives = frames.get("primitives")
    if not isinstance(box_count, int) or box_count <= 0:
        warn("S1 scene parameters do not contain box_count; the primitive check is skipped")
        return
    if not isinstance(primitives, list) or not primitives:
        fail("S1 has no primitives array")
        return
    expected = 12 * box_count + 2
    actual = _median(primitives)
    if abs(actual - expected) > expected * TOLERANCE:
        fail("S1 primitives median is %.1f, expected %.1f (12 * %d + 2)" % (actual, expected, box_count))


def _check_s2(fail, warn, frames):
    draw_calls = frames.get("draw_calls")
    if not isinstance(draw_calls, list) or not draw_calls:
        fail("S2 has no draw_calls array")
        return
    if max(draw_calls) - min(draw_calls) > 1:
        fail("S2 draw calls vary within the run (%d..%d); one MultiMesh must not change count"
             % (min(draw_calls), max(draw_calls)))
    primitives = frames.get("primitives")
    if isinstance(primitives, list) and primitives:
        if _relative_spread(primitives) > TOLERANCE:
            warn("S2 primitives vary by %.2f%% within the run" % (100.0 * _relative_spread(primitives)))


def _check_s8(fail, warn, info, frames, parameters):
    """S8 is the only cell that turns occlusion culling on. Two things can make it
    meaningless without changing a single number: the engine ignoring the request,
    and a culler that is present but culls nothing."""
    requested = parameters.get("occlusion_requested")
    effective = parameters.get("occlusion_effective")
    if requested is None or effective is None:
        warn("S8 parameters do not record occlusion_requested/occlusion_effective")
    elif bool(requested) != bool(effective):
        fail(f"occlusion culling requested={bool(requested)} but the viewport reports "
             f"{bool(effective)}; the cell does not measure what its label says")

    primitives = frames.get("primitives")
    if not isinstance(primitives, list) or not primitives:
        fail("S8 has no primitives array")
    elif _median(primitives) <= 0:
        fail("S8 primitives median is 0; the scene culled everything away and timed an empty frame")

    buffer_size = parameters.get("occlusion_buffer")
    if isinstance(buffer_size, (list, tuple)) and len(buffer_size) == 2:
        texels = int(buffer_size[0]) * int(buffer_size[1])
        info(f"occlusion HZB base level {int(buffer_size[0])}x{int(buffer_size[1])} from "
             f"{parameters.get('rays_per_thread')} rays/thread x {parameters.get('processor_count')} "
             f"threads ({texels} pyramid texels); a ray budget, not the screen")


def _cell_parameters(key):
    try:
        return json.loads(key[1])
    except (TypeError, ValueError):
        return None


def _cell_audio(key):
    """The audio state of a cell, as recorded in its key."""
    return key[8]


def _s8_pair_signature(key):
    """Everything that must match for two S8 cells to be one A/B pair, or None.
    key is (scene, parameters, preset, configuration, method, driver, adapter,
    content hash, audio driver). The audio driver stays out as well, so a pair
    refused for that reason alone can be named rather than dropped in silence;
    _check_s8_pairs compares it explicitly. The parameters and the content hash deliberately stay out of
    the signature: the hash covers scene_parameters(), which is where the
    occlusion flag is recorded, so demanding it match would guarantee no pair is
    ever found. Everything that is *not* the setting under test must match."""
    if key[0] != "S8":
        return None
    parameters = _cell_parameters(key)
    if not parameters or parameters.get("variant") != "closed":
        return None
    # The ray budget has to match as well: it sizes the HZB, so a pair taken at two
    # different budgets changes two things at once and proves nothing.
    return key[2:7] + (parameters.get("instance_count"), parameters.get("rays_per_thread"))


def _check_s8_pairs(grouped, per_cell, failure_count):
    """Compare each closed-variant pair (occlusion off vs on) and report what culling
    removed. Only cells with repeated runs are paired, matching the published
    protocol; the verdict is attached to the occlusion-on cell, which is the one
    that either delivers the win or does not."""
    signatures = {}
    for key, reports in grouped.items():
        signature = _s8_pair_signature(key)
        if signature is None or len(reports) < 2:
            continue
        parameters = _cell_parameters(key) or {}
        signatures.setdefault(signature, {})[bool(parameters.get("occlusion_requested"))] = (key, reports)

    issues_by_key = {key: issues for key, _count, issues in per_cell}
    for sides in signatures.values():
        if set(sides) != {False, True}:
            continue

        a_audio = _cell_audio(sides[False][0])
        b_audio = _cell_audio(sides[True][0])
        if a_audio != b_audio:
            # Refuse the pair out loud. Silence here would read as "no data" on a directory
            # where old cells and new ones sit side by side, and a difference nobody names is a
            # difference someone will later assume was zero.
            if AUDIO_NOT_RECORDED in (a_audio, b_audio):
                level = WARN
                detail = (f"one side is {a_audio!r} and the other {b_audio!r}, so it cannot be "
                          f"shown that the audio state matched")
            else:
                level = FAIL
                detail = (f"one side ran with audio driver {a_audio!r} and the other with "
                          f"{b_audio!r}; only the dummy driver is free of a device callback")
            issues = issues_by_key.get(sides[True][0])
            message = f"S8 pair refused: {detail}"
            if issues is not None:
                issues.append((level, message))
            if level == FAIL:
                failure_count += 1
            continue

        def median_primitives(reports):
            values = []
            for report in reports:
                values.extend(((report.get("frames") or {}).get("primitives")) or [])
            return _median(values) if values else 0

        runs = sides[False][1] + sides[True][1]
        invalid = [r for r in runs if r.get("valid") is not True]
        if invalid:
            # An invalid run already FAILs with the harness's own reason. Repeating
            # it as "occulsion culled nothing" would point at the wrong cause, so say
            # the comparison was skipped and why, instead of going silent.
            issues = issues_by_key.get(sides[True][0])
            if issues is not None:
                issues.append((INFO, f"occlusion pair not compared: {len(invalid)} of {len(runs)} runs "
                                     f"are marked invalid ({invalid[0].get('invalid_reason') or 'no reason given'})"))
            continue

        off = median_primitives(sides[False][1])
        on = median_primitives(sides[True][1])
        if off <= 0 or on <= 0:
            continue
        ratio = float(on) / float(off)
        if ratio > 0.995:
            level, message = FAIL, (
                "closed variant: %d primitives with occlusion on vs %d off (%.1f%%) - nothing was "
                "culled. Either this build has no occlusion provider (the base "
                "RendererSceneOcclusionCull returns a null buffer and only warns once: "
                "modules/raycast must be built, which Web templates skip by default) or the scene "
                "failed to occlude the field. Check the build before blaming the culler."
                % (on, off, 100.0 * ratio))
            failure_count += 1
        else:
            level, message = INFO, (
                "closed variant: occlusion culling removed %.1f%% of the primitives (%d -> %d)"
                % (100.0 * (1.0 - ratio), off, on))
        issues = issues_by_key.get(sides[True][0])
        if issues is not None:
            issues.append((level, message))
    return failure_count


def _check_s6(fail, warn, report, frames, parameters):
    diff = report.get("image_diff") or {}
    if not diff:
        return
    mean = diff.get("mean_abs_diff")
    if not isinstance(mean, (int, float)):
        fail("image_diff is present but has no numeric mean_abs_diff")
        return
    # The applied preset comes from the scene, not from the report label: on S6
    # --bench-preset selects it, but a run that forgot the flag still reports
    # its label and applies P0.
    post = parameters.get("post") or {}
    preset = post.get("preset") or report.get("preset")
    label = report.get("preset")
    if post.get("preset") and label != post["preset"]:
        warn("report label %r but the scene applied post preset %r" % (label, post["preset"]))
    if preset != "P0" and float(mean) == 0.0:
        fail("S6 preset %s produced an image identical to the reference; the effect did not run" % preset)
    if isinstance(diff.get("width"), int) and isinstance(diff.get("height"), int):
        if diff["width"] <= 0 or diff["height"] <= 0:
            fail("image_diff reports an empty image")
    support = post.get("support") or {}
    for effect in ("glow", "ssao", "ssil", "volumetric_fog"):
        if post.get(effect) is True and support.get(effect) is False:
            fail(
                "S6 requested %s but the renderer does not provide it; the cell is not applicable"
                % effect
            )


def _cell_key(report):
    scene = report.get("scene") or {}
    runtime = report.get("runtime") or {}
    return (
        str(scene.get("id")),
        json.dumps(scene.get("parameters") or {}, sort_keys=True),
        str(report.get("preset")),
        str(report.get("configuration")),
        str(runtime.get("rendering_method")),
        str(runtime.get("rendering_driver")),
        str(runtime.get("adapter_name")),
        str(scene.get("content_hash")),
        # Part of the cell identity: two runs that differ only in whether a live audio device
        # was mixing are not repeats of one measurement. Splitting them here is what makes the
        # pair check below able to trust a cell, and it replaces a misleading p95-spread
        # complaint with the audio message the run actually deserves.
        str(runtime.get("audio_driver") or AUDIO_NOT_RECORDED),
    )


def _check_cell(reports):
    issues = []
    scene = reports[0].get("scene") or {}
    scene_id = str(scene.get("id"))
    parameters = scene.get("parameters") or {}

    hashes = {report.get("frame_hash") for report in reports}
    if len(hashes) > 1:
        issues.append((FAIL, "frame_hash differs between repeated runs of the same cell: %s" % sorted(hashes)))

    draw_medians = []
    for report in reports:
        draw_calls = (report.get("frames") or {}).get("draw_calls")
        if isinstance(draw_calls, list) and draw_calls:
            draw_medians.append(_median(draw_calls))
    if len(draw_medians) > 1 and max(draw_medians) - min(draw_medians) > 1:
        issues.append((FAIL, "draw-call median differs between repeated runs: %s" % sorted(draw_medians)))

    if scene_id == "S3" and parameters.get("feature_variety") is True:
        counts = [
            json.dumps(((report.get("pipeline_compilations") or {}).get("first_120") or {}), sort_keys=True)
            for report in reports
        ]
        if len(set(counts)) > 1:
            issues.append((FAIL, "S3b first_120 pipeline compilations differ between repeated runs: %s" % counts))
        else:
            issues.append((INFO, "S3b first_120 is stable: %s" % counts[0]))

    gpu_values = []
    for report in reports:
        summary = report.get("summary") or {}
        if isinstance(summary.get("gpu_ms_median"), (int, float)):
            gpu_values.append(float(summary["gpu_ms_median"]))
    if len(gpu_values) > 1:
        issues.append((INFO, "GPU median spread across runs: %.2f%%" % (100.0 * _relative_spread(gpu_values))))

    return issues


def run(paths):
    """Return (per_report, per_cell, failure_count)."""
    loaded = []
    for path in paths:
        try:
            if str(path).endswith(".gz"):
                with gzip.open(path, "rt", encoding="utf-8") as handle:
                    report = json.load(handle)
            else:
                with open(path, "r", encoding="utf-8") as handle:
                    report = json.load(handle)
        except OSError as error:
            raise ValueError("%s: %s" % (path, error))
        except EOFError as error:
            raise ValueError("%s: truncated gzip stream (%s)" % (path, error))
        except json.JSONDecodeError as error:
            raise ValueError("%s: not valid JSON (%s)" % (path, error))
        if not isinstance(report, dict):
            raise ValueError("%s: the top-level value is not an object" % path)
        loaded.append((str(path), report))

    per_report = []
    failure_count = 0
    for path, report in loaded:
        issues = _check_report(report)
        per_report.append((path, issues))
        failure_count += sum(1 for level, _ in issues if level == FAIL)

    grouped = {}
    for path, report in loaded:
        grouped.setdefault(_cell_key(report), []).append(report)

    per_cell = []
    for key, reports in grouped.items():
        if len(reports) < 2:
            continue
        issues = _check_cell(reports)
        per_cell.append((key, len(reports), issues))
        failure_count += sum(1 for level, _ in issues if level == FAIL)

    failure_count = _check_s8_pairs(grouped, per_cell, failure_count)

    return per_report, per_cell, failure_count


def _print_issues(issues, indent="    "):
    for level, message in issues:
        print("%s%-4s %s" % (indent, level, message))


def main(argv):
    if not argv or argv[0] in ("-h", "--help"):
        print(__doc__.strip())
        return 0 if argv else 2

    try:
        per_report, per_cell, failure_count = run(argv)
    except ValueError as error:
        print("error: %s" % error, file=sys.stderr)
        return 2

    for path, issues in per_report:
        failed = any(level == FAIL for level, _ in issues)
        print("%s  %s" % (path, FAIL if failed else "PASS"))
        _print_issues(issues)

    for key, count, issues in per_cell:
        failed = any(level == FAIL for level, _ in issues)
        scene_id = key[0]
        preset = key[2]
        configuration = key[3]
        print("cell %s/%s/%s (%d runs)  %s" % (scene_id, preset, configuration, count, FAIL if failed else "PASS"))
        _print_issues(issues)

    total_failures = sum(1 for _, issues in per_report for level, _ in issues if level == FAIL)
    total_failures += sum(1 for _, _, issues in per_cell for level, _ in issues if level == FAIL)
    print(
        "%d report(s), %d repeated cell(s), %d failure(s)"
        % (len(per_report), len(per_cell), total_failures)
    )
    return 0 if total_failures == 0 else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
