#!/usr/bin/env python3
"""Tests for check_report.py.

Run with:
    python3 benchmarks/tools/test_check_report.py
"""

import contextlib
import copy
import io
import json
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import check_report  # noqa: E402

SAMPLED = 3600
WARMUP = 900


def deep_merge(base, override):
    for key, value in override.items():
        if isinstance(value, dict) and isinstance(base.get(key), dict):
            deep_merge(base[key], value)
        else:
            base[key] = value
    return base


def make_report(**overrides):
    report = {
        "schema": 1,
        "engine": {"major": 4, "minor": 8, "patch": 0, "status": "beta", "hash": "abc"},
        "build": {"type": "template", "flags": [], "user_args": ["--bench-scene", "S1"]},
        "scene": {"id": "S1", "parameters": {"box_count": 400}, "content_hash": "content"},
        "preset": "Q0",
        "configuration": "D1",
        "valid": True,
        "invalid_reason": "",
        "project_settings": {
            "rendering/anti_aliasing/quality/msaa_3d": 0,
            "rendering/scaling_3d/mode": 0,
            "rendering/scaling_3d/scale": 1.0,
            "display/window/size/viewport_width": 1920,
            "display/window/size/viewport_height": 1080,
        },
        "runtime": {
            "rendering_method": "forward_plus",
            "rendering_driver": "vulkan",
            "display_server": "x11",
            "vsync_mode": 0,
            "msaa_3d": 0,
            "use_taa": False,
            "scaling_3d_mode": 0,
            "scaling_3d_scale": 1.0,
            "adapter_name": "Fake GPU",
            "adapter_vendor": "Fake Vendor",
            "api_version": "1.3",
            "window_size": [1920, 1080],
        },
        "timing": {
            "first_frame": WARMUP,
            "last_frame": WARMUP + SAMPLED - 1,
            "warmup_frames": WARMUP,
            "sampled_frames": SAMPLED,
            "pso_at_start": 12,
            "pso_in_sample": 0,
            "last_compile_frame": -1,
        },
        "pipeline_compilations": {
            "first_120": {"canvas": 0, "mesh": 0, "surface": 0, "draw": 0, "specialization": 0},
            "total": {"canvas": 0, "mesh": 0, "surface": 0, "draw": 0, "specialization": 0},
        },
        "frames": {
            "frame_us": [16000] * SAMPLED,
            "process_ms": [1.0] * SAMPLED,
            "draw_calls": [2] * SAMPLED,
            "objects": [401] * SAMPLED,
            "primitives": [4802] * SAMPLED,
            "texture_mem": [1000] * SAMPLED,
            "buffer_mem": [1000] * SAMPLED,
            "video_mem": [1000] * SAMPLED,
            "pso_compilations": [0] * SAMPLED,
            "viewport_cpu_ms": [4.0] * SAMPLED,
            "viewport_gpu_ms": [],
        },
        "summary": {
            "frames": SAMPLED,
            "fps_median": 62.5,
            "frame_ms_median": 16.0,
            "frame_ms_p95": 16.1,
            "frame_ms_p99": 16.2,
            "frame_ms_worst": 16.5,
            "frame_ms_min": 15.9,
            "hitches": 0,
            "gpu_ms_median": None,
            "cpu_ms_median": 1.0,
        },
        "frame_hash": "hash-a",
        "screenshot": "",
        "image_diff": {},
        "notes": [],
    }
    return deep_merge(report, overrides)


class CheckReportTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.addCleanup(self.directory.cleanup)

    def write(self, name, report):
        path = Path(self.directory.name) / name
        path.write_text(json.dumps(report), encoding="utf-8")
        return str(path)

    def check(self, *reports):
        paths = [self.write("report_%d.json" % index, report) for index, report in enumerate(reports)]
        _, _, failures = check_report.run(paths)
        return failures

    def test_valid_report_passes(self):
        self.assertEqual(self.check(make_report()), 0)

    def test_tools_build_fails(self):
        report = make_report(build={"type": "tools"})
        self.assertGreater(self.check(report), 0)

    def test_headless_fails(self):
        report = make_report()
        report["build"]["user_args"].append("--headless")
        self.assertGreater(self.check(report), 0)

    def test_vsync_fails(self):
        report = make_report()
        report["runtime"]["vsync_mode"] = 1
        self.assertGreater(self.check(report), 0)

    def test_pso_in_sample_fails(self):
        report = make_report()
        report["timing"]["pso_in_sample"] = 3
        self.assertGreater(self.check(report), 0)

    def test_frame_compilation_fails(self):
        report = make_report()
        report["frames"]["pso_compilations"][10] = 1
        self.assertGreater(self.check(report), 0)

    def test_missing_frame_hash_fails(self):
        report = make_report()
        report["frame_hash"] = ""
        self.assertGreater(self.check(report), 0)

    def test_sampled_frames_must_be_multiple_of_loop(self):
        report = make_report()
        report["timing"]["sampled_frames"] = 3599
        report["timing"]["last_frame"] = WARMUP + 3598
        self.assertGreater(self.check(report), 0)

    def test_invalid_report_fails(self):
        report = make_report(valid=False, invalid_reason="pipeline compilations during the sampling window (2)")
        self.assertGreater(self.check(report), 0)

    def test_s1_primitive_mismatch_fails(self):
        report = make_report()
        report["frames"]["primitives"] = [4803 + 100] * SAMPLED
        self.assertGreater(self.check(report), 0)

    def test_s1_primitive_within_tolerance_passes(self):
        report = make_report()
        report["frames"]["primitives"] = [4802 + 40] * SAMPLED
        self.assertEqual(self.check(report), 0)

    def test_s6_zero_diff_fails(self):
        report = make_report(
            scene={"id": "S6", "parameters": {"quad_count": 4, "post": {"preset": "P2"}}, "content_hash": "s6"},
            preset="P2",
            image_diff={"reference": "/tmp/p0.png", "width": 1920, "height": 1080, "mean_abs_diff": 0.0, "max_abs_diff": 0},
        )
        self.assertGreater(self.check(report), 0)

    def test_s6_nonzero_diff_passes(self):
        report = make_report(
            scene={"id": "S6", "parameters": {"quad_count": 4, "post": {"preset": "P2"}}, "content_hash": "s6"},
            preset="P2",
            image_diff={"reference": "/tmp/p0.png", "width": 1920, "height": 1080, "mean_abs_diff": 1.5, "max_abs_diff": 60},
        )
        self.assertEqual(self.check(report), 0)

    def test_gpu_unavailable_is_allowed(self):
        self.assertEqual(self.check(make_report()), 0)

    def test_gpu_values_with_null_summary_fail(self):
        report = make_report()
        report["frames"]["viewport_gpu_ms"] = [8.0] * SAMPLED
        report["summary"]["gpu_ms_median"] = None
        self.assertGreater(self.check(report), 0)

    def test_repeated_runs_with_different_hash_fail(self):
        first = make_report()
        second = make_report()
        second["frame_hash"] = "hash-b"
        self.assertGreater(self.check(first, second), 0)

    def test_repeated_runs_with_same_hash_pass(self):
        self.assertEqual(self.check(make_report(), make_report()), 0)

    def test_draw_call_drift_between_runs_fails(self):
        first = make_report()
        second = make_report()
        second["frames"]["draw_calls"] = [4] * SAMPLED
        self.assertGreater(self.check(first, second), 0)

    def test_s3b_first_120_drift_fails(self):
        first = make_report(
            scene={"id": "S3", "parameters": {"feature_variety": True, "material_count": 8}, "content_hash": "s3b"},
        )
        second = copy.deepcopy(first)
        second["pipeline_compilations"]["first_120"]["draw"] = 7
        self.assertGreater(self.check(first, second), 0)

    def test_s3b_stable_first_120_passes(self):
        first = make_report(
            scene={"id": "S3", "parameters": {"feature_variety": True, "material_count": 8}, "content_hash": "s3b"},
        )
        second = copy.deepcopy(first)
        self.assertEqual(self.check(first, second), 0)

    def test_s2_draw_call_growth_within_run_fails(self):
        report = make_report(
            scene={"id": "S2", "parameters": {"instance_count": 4096}, "content_hash": "s2"},
        )
        report["frames"]["draw_calls"] = [1] * (SAMPLED // 2) + [5] * (SAMPLED // 2)
        self.assertGreater(self.check(report), 0)

    def test_bad_json_exits_two(self):
        path = Path(self.directory.name) / "broken.json"
        path.write_text("{not json", encoding="utf-8")
        self.assertEqual(check_report.main([str(path)]), 2)

    def test_help_exits_zero(self):
        with contextlib.redirect_stdout(io.StringIO()):
            self.assertEqual(check_report.main(["--help"]), 0)

    def test_passing_report_main_exits_zero(self):
        path = self.write("ok.json", make_report())
        with contextlib.redirect_stdout(io.StringIO()):
            self.assertEqual(check_report.main([path]), 0)


if __name__ == "__main__":
    unittest.main()
