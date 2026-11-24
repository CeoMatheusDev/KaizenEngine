#!/usr/bin/env python3
"""Check that every licence banner in the tree is a rectangular box.

Usage:
    python3 tests/kaizen/license_box/check_license_box.py [--repo-root PATH]
        [--paths DIR ...] [--max-pending N] [--verbose] [--fix]

Why a box at all: a banner row is an individually terminated comment, one per line, and the
padding in front of each row's closing marker is the only thing holding it to the width that
the file's own border row declares. The rebrand widened two rows by one column each
("GODOT ENGINE" -> "KAIZEN ENGINE", "Godot Engine contributors" -> "Kaizen Engine
contributors") without moving a space, and renaming a module widened the filename row of
every file inside it (`godot_view.h` -> `kaizen_view.h`). Neither was visible to
`misc/scripts/copyright_headers.py`, the tool that writes these banners, because its own
template carried the same defect: generator and tree agreed on being ragged.

Two outcomes, and the difference between them is the point of the tool:

    VIOLATION  a row is off the box width and moving spaces closes the gap - a defect, and
               what --fix does.
    PENDING    a row is off the box width and the only space left is the one between the
               text and the closing marker. Removing it would read `...text.*/` and change
               licence words' context, so it needs a decision about the notice text instead
               (docs/license-notice-consistency.md, section 4). Counted against
               --max-pending, so the debt is a ratchet: paying it down passes, growing it
               fails.

UNSAFE rows (a tab or carriage return inside the line) are reported and never rewritten.

Exit codes: 0 within budget, 1 on a violation or a pending count above budget, 2 on bad
usage. Without --fix this tool never writes to a file.
"""

from __future__ import annotations

import argparse
import os
import re
import subprocess
import sys

SOURCE_EXTENSIONS = (
    ".c", ".cpp", ".h", ".hpp", ".m", ".mm", ".inc", ".kt", ".java", ".js", ".swift", ".glsl", ".metal",
)

# The two banner shapes in this repository. `close` is what ends a row, and it is *not* the
# mirror of `open` for the /* */ box - getting that wrong appends "/*" to every row, which
# is why this table is explicit and why fix_file() re-reads and compares before writing.
SHAPES = (
    {
        "name": "box",
        "border": re.compile(rb"^/[*]+/$"),
        "row": re.compile(rb"^/\*(.*)\*/$", re.S),
        "open": b"/*",
        "close": b"*/",
    },
    {
        "name": "slash",
        "border": re.compile(rb"^//[*]+//$"),
        "row": re.compile(rb"^//\s(.*)//$", re.S),
        "open": b"//",
        "close": b"//",
    },
)

MAX_BANNER_ROWS = 60


def _rows_of(line_list):
    """Return (shape, width, rows) for the banner at the head of a file, or None."""
    if not line_list:
        return None
    first = line_list[0]
    for shape in SHAPES:
        if not shape["border"].match(first):
            continue
        rows = [(0, first)]
        for index in range(1, min(len(line_list), MAX_BANNER_ROWS)):
            line = line_list[index]
            if shape["border"].match(line):
                rows.append((index, line))
                continue
            if not shape["row"].match(line):
                break
            rows.append((index, line))
        return shape, len(first), rows
    return None


def banner_of(path):
    """Return (shape, width, rows) for a bannered file, or None when it has no banner.

    Only the head of the file is read; the banner is contiguous from line 1.
    """
    with open(path, "rb") as handle:
        head = handle.read(8192)
    return _rows_of(head.split(b"\n"))


def plan_rows(shape, width, rows):
    """Yield (index, line, action, new_line) for every banner row.

    action is "ok", "pad" (spaces alone close the gap), "text" (only the single space
    between text and the closing marker is left, so it is a licence-text question) or
    "unsafe" (a tab or carriage return is in the row; left alone).
    """
    for index, line in rows:
        if shape["border"].match(line):
            yield index, line, "ok", line
            continue
        if not shape["row"].match(line):  # cannot reason about a row we cannot parse
            yield index, line, "ok", line
            continue
        if b"\r" in line or b"\t" in line:
            yield index, line, "unsafe", line
            continue
        if len(line) == width:
            yield index, line, "ok", line
            continue
        body = line[: -len(shape["close"])].rstrip(b" ")
        padding = width - len(body) - len(shape["close"])
        if padding >= 1:
            yield index, line, "pad", body + b" " * padding + shape["close"]
        else:
            yield index, line, "text", line


def check_file(path):
    """Return (violations, pending, unsafe, notes) for one file."""
    banner = banner_of(path)
    if banner is None:
        return 0, 0, 0, []
    shape, width, rows = banner
    counts = {"pad": 0, "text": 0, "unsafe": 0}
    notes = []
    for index, line, action, _new in plan_rows(shape, width, rows):
        if action == "ok":
            continue
        counts[action] += 1
        if action == "text":
            notes.append(f"line {index}: {len(line)} wide, box is {width}, only the space before the marker is left")
        elif action == "unsafe":
            notes.append(f"line {index}: {len(line)} wide, box is {width}, not touched (tab or CR in the row)")
        else:
            notes.append(f"line {index}: {len(line)} wide, box is {width}")
    return counts["pad"], counts["text"], counts["unsafe"], notes


def fix_file(path):
    """Re-pad one file's banner rows to its own box width. Return the number of rows changed.

    The file is re-read and compared with all spaces removed before anything is written, so
    a wrong marker or an unexpected byte inside a row aborts the write instead of corrupting
    a licence notice.
    """
    banner = banner_of(path)
    if banner is None:
        return 0
    shape, width, rows = banner
    with open(path, "rb") as handle:
        before = handle.read()
    lines = before.split(b"\n")
    changed = 0
    for index, line, action, new_line in plan_rows(shape, width, rows):
        if action != "pad" or new_line == line:
            continue
        lines[index] = new_line
        changed += 1
    if not changed:
        return 0
    out = b"\n".join(lines)
    if out.replace(b" ", b"") != before.replace(b" ", b""):
        raise ValueError(f"{path}: re-padding would change a non-space byte")
    with open(path, "wb") as handle:
        handle.write(out)
    return changed


def git_visible_files(root):
    """Relative paths git would show: tracked plus untracked-but-not-ignored.

    Generated artifacts (*.gen.h and friends) are gitignored build outputs whose
    banner rows are mechanical copies of the source's; scanning them reports
    generator formatting as licence violations. Returns None when git is not
    available (the walk then keeps every file).
    """
    try:
        out = subprocess.run(
            ["git", "-C", root, "ls-files", "--cached", "--others", "--exclude-standard", "-z"],
            capture_output=True,
            check=True,
        )
    except (subprocess.CalledProcessError, FileNotFoundError):
        return None
    return {p.decode("utf-8", "surrogateescape") for p in out.stdout.split(b"\0") if p}


def source_files(root, paths):
    """Every code file under `paths`, minus vendored trees and ignored artifacts."""
    selected = []
    seen = set()
    visible = git_visible_files(root)
    for base in paths:
        if os.path.isfile(base):
            if base.endswith(SOURCE_EXTENSIONS):
                selected.append(base)
            continue
        for directory, _dirs, filenames in os.walk(base):
            for name in sorted(filenames):
                if not name.endswith(SOURCE_EXTENSIONS):
                    continue
                full = os.path.join(directory, name)
                rel = os.path.relpath(full, root).replace(os.sep, "/")
                if rel.startswith("thirdparty/") or "/thirdparty/" in rel or rel in seen:
                    continue
                if visible is not None and rel not in visible:
                    continue
                seen.add(rel)
                selected.append(full)
    return selected


def fix(root, paths):
    files_changed = 0
    rows_changed = 0
    for path in source_files(root, paths):
        count = fix_file(path)
        if count:
            files_changed += 1
            rows_changed += count
    return files_changed, rows_changed


def run(root, paths, verbose=False):
    """Return (files_with_banners, rows_checked, violations, pending, unsafe, details)."""
    banners = rows_checked = violations = pending = unsafe = 0
    details = []
    for path in source_files(root, paths):
        banner = banner_of(path)
        if banner is None:
            continue
        banners += 1
        rows_checked += len(banner[2])
        file_violations, file_pending, file_unsafe, notes = check_file(path)
        violations += file_violations
        pending += file_pending
        unsafe += file_unsafe
        if (file_violations or file_unsafe) or (verbose and file_pending):
            details.append((os.path.relpath(path, root).replace(os.sep, "/"), notes))
    return banners, rows_checked, violations, pending, unsafe, details


def repo_root_from_here():
    """The repository root: this file lives in tests/kaizen/license_box/."""
    path = os.path.abspath(__file__)
    for _ in range(4):
        path = os.path.dirname(path)
    return path


def main(argv):
    parser = argparse.ArgumentParser(description="Check licence banner boxes.", add_help=True)
    parser.add_argument("--repo-root", default=repo_root_from_here())
    parser.add_argument("--paths", nargs="*", default=None, help="subtrees to check (default: the repo root)")
    parser.add_argument("--max-pending", type=int, default=0,
                        help="licence-text rows allowed before this fails (0 to demand a clean tree)")
    parser.add_argument("--verbose", action="store_true", help="also list pending rows")
    parser.add_argument("--fix", action="store_true", help="move padding so rows fit the box; never edits text")
    args = parser.parse_args(argv)

    root = os.path.abspath(args.repo_root)
    paths = [p if os.path.isabs(p) else os.path.join(root, p) for p in (args.paths or [root])]
    missing = [p for p in paths if not os.path.exists(p)]
    if missing:
        for path in missing:
            print(f"error: no such path: {path}", file=sys.stderr)
        return 2

    try:
        if args.fix:
            files_changed, rows_changed = fix(root, paths)
            print(f"license_box --fix: {files_changed} file(s), {rows_changed} row(s) re-padded")
        banners, rows_checked, violations, pending, unsafe, details = run(root, paths, args.verbose)
    except (ValueError, OSError) as error:
        print(f"error: {error}", file=sys.stderr)
        return 2

    if banners == 0:
        print("license_box: no bannered source file found - that is not a pass", file=sys.stderr)
        return 1

    for path, notes in details:
        for note in notes:
            print(f"  {path}: {note}")

    ok = violations == 0 and unsafe == 0 and pending <= args.max_pending
    verdict = "OK" if ok else "FAIL"
    print(
        f"license_box: {banners} banner(s), {rows_checked} row(s), {violations} violation(s), "
        f"{pending} pending licence-text row(s) (budget {args.max_pending}), {unsafe} unsafe row(s) - {verdict}"
    )
    if violations:
        print("  fix: python3 tests/kaizen/license_box/check_license_box.py --fix")
    if unsafe:
        print(f"  {unsafe} row(s) contain a tab or CR inside a banner: expand them by hand, this tool will not.")
    if pending > args.max_pending:
        over = pending - args.max_pending
        print(f"  {over} row(s) over budget: a banner row grew and cannot be padded back, so the "
              f"licence text itself has to change (docs/license-notice-consistency.md, section 4)")
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
