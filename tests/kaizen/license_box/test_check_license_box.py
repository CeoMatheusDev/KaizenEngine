#!/usr/bin/env python3
"""Tests for check_license_box.py and for the two header tools it guards.

Run with:
    python3 tests/kaizen/license_box/test_check_license_box.py

The synthetic fixtures isolate one rule each; the last class runs the repository's real tools
over copies of real files, which is where the two defects this suite found stay fixed: a
banner box one column too wide, and the generator deleting the `//` licence notice.
"""

import contextlib
import io
import os
import shutil
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import check_license_box  # noqa: E402

REPO_ROOT = check_license_box.repo_root_from_here()
BOX = {"open": b"/*", "close": b"*/"}
SLASH = {"open": b"//", "close": b"//"}

# Recorded debt, and the number docs/license-notice-consistency.md quotes. Lower it when the
# licence-text decision is taken; never raise it without saying why in that document.
BUDGET = 2382


def border(width, style):
    return b"/" + b"*" * (width - 2) + b"/" if style is BOX else b"//" + b"*" * (width - 4) + b"//"


def row(text, width, style):
    """One banner row: `text` inside the markers, padded so the line is exactly `width`."""
    body = style["open"] + text
    padding = width - len(body) - len(style["close"])
    assert padding >= 1, f"fixture text does not fit the box: {text!r}"
    return body + b" " * padding + style["close"]


def banner(width=76, style=BOX, rows=(), trailer=(b"", b"#pragma once")):
    lines = [border(width, style)]
    lines.extend(rows)
    lines.extend(trailer)
    return b"\n".join(lines) + b"\n"


class Fixture(unittest.TestCase):
    def setUp(self):
        self.dir = tempfile.mkdtemp(prefix="license-box-")
        self.addCleanup(shutil.rmtree, self.dir, ignore_errors=True)

    def write(self, name, data):
        path = os.path.join(self.dir, name)
        with open(path, "wb") as handle:
            handle.write(data)
        return path

    def counts(self, path):
        return check_license_box.check_file(path)[:3]

    def read(self, path):
        with open(path, "rb") as handle:
            return handle.read()


class TestGeometry(Fixture):
    def test_rectangular_box_is_clean(self):
        rows = [row(b"  A note", 76, BOX), row(b"  Another line", 76, BOX)]
        self.assertEqual(self.counts(self.write("ok.cpp", banner(rows=rows))), (0, 0, 0))

    def test_wide_row_with_padding_is_a_violation(self):
        # The shape the rebrand produced: one space too many in front of the marker.
        wide = row(b"   KAIZEN ENGINE", 77, BOX)
        path = self.write("wide.cpp", banner(rows=[wide]))
        self.assertEqual(self.counts(path), (1, 0, 0))

    def test_fix_pads_the_row_and_keeps_the_closing_marker(self):
        wide = row(b"   KAIZEN ENGINE", 77, BOX)
        path = self.write("wide.cpp", banner(rows=[wide]))
        self.assertEqual(check_license_box.fix_file(path), 1)
        fixed = self.read(path).split(b"\n")[1]
        self.assertEqual(len(fixed), 76)
        # Regression for a real bug in the first --fix, which appended the *opening* marker.
        self.assertTrue(fixed.endswith(BOX["close"]), fixed)
        self.assertEqual(fixed.count(b"/*"), 1, fixed)
        self.assertEqual(self.counts(path), (0, 0, 0))

    def test_fix_only_moves_spaces(self):
        before = banner(width=60, rows=[row(b"  text", 60, BOX), row(b"  other line", 61, BOX)])
        path = self.write("mixed.cpp", before)
        original = self.read(path)
        check_license_box.fix_file(path)
        after = self.read(path)
        self.assertEqual(after.replace(b" ", b""), original.replace(b" ", b""))
        self.assertEqual(self.counts(path), (0, 0, 0))  # both rows now on the box width

    def test_narrow_row_is_a_violation_and_is_padded_out(self):
        narrow = BOX["open"] + b"  short text  " + BOX["close"]  # 22 wide, box is 76
        path = self.write("narrow.h", banner(rows=[narrow]))
        self.assertEqual(self.counts(path), (1, 0, 0))
        check_license_box.fix_file(path)
        self.assertEqual(len(self.read(path).split(b"\n")[1]), 76)

    def test_unpaddable_row_is_pending_and_is_not_touched(self):
        # Text that fills the box plus one: the only space left is the one before the marker.
        filler = b" " + b"x" * 72
        path = self.write("pending.h", banner(rows=[BOX["open"] + filler + BOX["close"]]))
        self.assertEqual(self.counts(path), (0, 1, 0))
        before = self.read(path)
        self.assertEqual(check_license_box.fix_file(path), 0)
        self.assertEqual(self.read(path), before)

    def test_tab_in_a_row_is_unsafe_and_is_not_touched(self):
        tabbed = BOX["open"] + b"\t  misaligned" + b" " * 12 + BOX["close"]
        path = self.write("tabs.cpp", banner(rows=[tabbed]))
        self.assertEqual(self.counts(path), (0, 0, 1))
        before = self.read(path)
        self.assertEqual(check_license_box.fix_file(path), 0)
        self.assertEqual(self.read(path), before)

    def test_slash_banner_uses_its_own_closing_marker(self):
        # The fork's `//` style: the closing marker is `//`, not `*/`, and the same rule applies.
        rows = [row(b" This file is part of:", 76, SLASH), row(b" KaizenEngine", 76, SLASH)]
        path = self.write("slash.cpp", banner(width=76, style=SLASH, rows=rows))
        self.assertEqual(self.counts(path), (0, 0, 0))
        path = self.write("wide_slash.cpp", banner(width=76, style=SLASH, rows=[row(b" KaizenEngine", 77, SLASH)]))
        self.assertEqual(self.counts(path), (1, 0, 0))
        check_license_box.fix_file(path)
        fixed = self.read(path).split(b"\n")[1]
        self.assertEqual(len(fixed), 76)
        self.assertTrue(fixed.endswith(SLASH["close"]), fixed)

    def test_file_without_a_banner_is_skipped(self):
        path = self.write("plain.cpp", b"// just a comment\nint main() { return 0; }\n")
        self.assertIsNone(check_license_box.banner_of(path))
        self.assertEqual(self.counts(path), (0, 0, 0))


class TestTool(Fixture):
    def call(self, *args):
        with contextlib.redirect_stdout(io.StringIO()) as out, contextlib.redirect_stderr(io.StringIO()):
            return check_license_box.main(list(args)), out.getvalue()

    def test_budget_decides_pending_rows(self):
        filler = b" " + b"x" * 72
        pending_row = BOX["open"] + filler + BOX["close"]
        self.write("a.h", banner(rows=[pending_row]))
        self.write("b.h", banner(rows=[pending_row, pending_row]))
        files_count, _rows, violations, pending, _unsafe, _details = check_license_box.run(self.dir, [self.dir])
        self.assertEqual((files_count, violations, pending), (2, 0, 3))
        base = ["--repo-root", self.dir, "--paths", self.dir]
        self.assertEqual(self.call(*base, "--max-pending", "3")[0], 0)
        self.assertEqual(self.call(*base, "--max-pending", "2")[0], 1)

    def test_violation_fails_even_on_zero_budget(self):
        self.write("a.h", banner(rows=[row(b"  text", 77, BOX)]))
        code, out = self.call("--repo-root", self.dir, "--paths", self.dir)
        self.assertEqual(code, 1)
        self.assertIn("1 violation(s)", out)

    def test_fix_then_check_is_clean(self):
        self.write("a.h", banner(rows=[row(b"  text", 78, BOX)]))
        self.assertEqual(self.call("--repo-root", self.dir, "--paths", self.dir)[0], 1)
        code, out = self.call("--repo-root", self.dir, "--paths", self.dir, "--fix")
        self.assertEqual(code, 0, out)
        self.assertIn("1 row(s) re-padded", out)
        # Idempotence: a second --fix has nothing left to do.
        self.assertIn("0 row(s) re-padded", self.call("--repo-root", self.dir, "--paths", self.dir, "--fix")[1])

    def test_no_banner_found_is_not_a_pass(self):
        self.write("plain.cpp", b"// nothing here\n")
        code, _out = self.call("--repo-root", self.dir, "--paths", self.dir)
        self.assertEqual(code, 1)

    def test_missing_path_exits_two(self):
        self.assertEqual(self.call("--repo-root", self.dir, "--paths", "nowhere")[0], 2)


class TestRepoAgreement(Fixture):
    """The invariants only the real tree can show."""

    def copy(self, relative):
        """Mirror the repository path inside the temp dir.

        The path is part of the input for these tools (vendored files are recognised by their
        `thirdparty/` component), so a flat basename would test something the repository never
        does - and two files sharing a basename would collide.
        """
        source = os.path.join(REPO_ROOT, relative)
        target = os.path.join(self.dir, relative)
        os.makedirs(os.path.dirname(target), exist_ok=True)
        shutil.copyfile(source, target)
        return source, target

    def run_script(self, script, *args):
        path = os.path.join(REPO_ROOT, "misc", "scripts", script)
        result = subprocess.run([sys.executable, path, *args], capture_output=True, text=True, cwd=REPO_ROOT)
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        return result

    def test_generator_is_a_noop_on_a_box_file(self):
        # True only while template and tree agree, which is what the geometry fix bought:
        # the tool reproduces the banner byte for byte.
        source, target = self.copy("core/config/engine.cpp")
        self.run_script("copyright_headers.py", target)
        self.assertEqual(self.read(target), self.read(source))

    def test_generator_does_not_touch_slash_banner_files(self):
        # Regression: the template is the /* */ box, so a `//` banner would be replaced by
        # it, deleting the derivation and AGPL notice with it.
        for relative in ("servers/audio/audio_effect.cpp", "scene/audio/audio_stream_player.cpp"):
            source, target = self.copy(relative)
            self.run_script("copyright_headers.py", target)
            self.assertEqual(self.read(target), self.read(source), f"{relative} lost its licence notice")

    def test_header_guards_accepts_both_banner_styles(self):
        copied = []
        for relative in ("servers/audio/audio_bus_layout.h", "core/config/engine.h", "servers/physics_3d/physics_server_3d.h"):
            source, target = self.copy(relative)
            copied.append((source, target))
        result = self.run_script("header_guards.py", *[target for _s, target in copied])
        self.assertNotIn("REQUIRES MANUAL CHANGES", result.stdout)
        self.assertNotIn("FIXED:", result.stdout)
        for source, target in copied:
            self.assertEqual(self.read(target), self.read(source), f"{target} was rewritten")

    def test_header_guards_leaves_generated_files_alone(self):
        # The seventeen *-so_wrap.h dynload wrappers declare "This file is generated. Do not
        # edit!" and are produced in .h/.c pairs. The tool's own rewrite used to convert their
        # guard to #pragma once and drop the matching #endif: 17 tracked files corrupted, and
        # undone by the next generation run.
        copied = []
        # coreclr_delegates.h has no banner at all and no generated-marker, so it needs the
        # thirdparty half of the rule rather than the generated half.
        for relative in ("drivers/alsa/asound-so_wrap.h", "drivers/pulseaudio/pulse-so_wrap.h",
                         "modules/mono/thirdparty/coreclr_delegates.h"):
            source, target = self.copy(relative)
            copied.append((source, target))
        result = self.run_script("header_guards.py", *[target for _s, target in copied])
        self.assertNotIn("FIXED:", result.stdout)
        self.assertNotIn("REQUIRES MANUAL CHANGES", result.stdout)
        for source, target in copied:
            self.assertEqual(self.read(target), self.read(source), f"{target} was rewritten")

    def test_template_box_is_rectangular_except_the_licence_line(self):
        with open(os.path.join(REPO_ROOT, "misc", "scripts", "copyright_headers.py"), "rb") as handle:
            lines = handle.read().split(b"\n")
        start = next(i for i, line in enumerate(lines) if check_license_box.SHAPES[0]["border"].match(line))
        width = len(lines[start])
        offenders = []
        for index in range(start, min(start + 40, len(lines))):
            if not lines[index].startswith(b"/*"):
                break
            if len(lines[index]) != width:
                offenders.append(index - start)
        # Exactly one row may exceed the box, and only because it cannot be padded: the
        # contributors line. A second offender means the banner grew again.
        self.assertEqual(offenders, [7], offenders)
        self.assertTrue(lines[start + 7].endswith(b"(see AUTHORS.md). */"), lines[start + 7])

    def test_tree_banners_are_within_budget(self):
        with contextlib.redirect_stdout(io.StringIO()) as out:
            code = check_license_box.main(["--repo-root", REPO_ROOT, "--max-pending", str(BUDGET)])
        self.assertEqual(code, 0, out.getvalue())
        self.assertIn("0 violation(s)", out.getvalue())
        self.assertIn("0 unsafe row(s)", out.getvalue())


if __name__ == "__main__":
    unittest.main()
