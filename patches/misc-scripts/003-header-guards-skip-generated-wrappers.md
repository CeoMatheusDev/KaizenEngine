# Patch 003 - header_guards.py stops rewriting generated and vendored headers

# Status: applied
# Priority: P1 (a house tool editing 21 tracked files it has no business touching)
# Depends on: patches/misc-scripts/001-header-guards-comment-banner.patch (same file; 003's
#   context lines are 001's, so apply 001 first - verified both ways below)
# Targets: misc/scripts/header_guards.py
# Diff: patches/misc-scripts/003-header-guards-skip-generated-wrappers.patch

# What this file is
#   Patch 001 fixed the *reporting* half of `header_guards.py`: 744 false `REQUIRES MANUAL
#   CHANGES` lines caused by not recognising the `//` banner. It did nothing about the other
#   half, which the same measurement had already shown: the tool also *writes*. Over the
#   1960 non-`thirdparty/` `.h`/`.hpp` files the fork tracks, run before this patch and on
#   copies so the tree was not harmed, it reports 0 flags and exit 0 - and rewrites 21
#   files. Those 21 are not "our style, their file" opinions:
#     - 17 `*-so_wrap.h` dynload wrappers (`drivers/alsa/asound-so_wrap.h`,
#       `platform/macos/*`, `platform/ios/*`, ...). Their second line literally says
#       `// This file is generated. Do not edit!`, they are produced by
#       `generate-wrapper.py 0.3` in `.h`/`.c` pairs, and upstream's own copy at `4.5`
#       carries the same `#ifndef DYLIBLOAD_WRAPPER_ASOUND` / `#define` pair. The tool
#       collapses that pair into `#pragma once` and deletes the matching `#endif`, which
#       edits one half of a generated pair and gets undone by whoever regenerates it.
#     - 4 vendored headers under `modules/mono/thirdparty/` (`coreclr_delegates.h`,
#       `hostfxr.h`, `mono_delegates.h`, `mono_types.h`), which have no banner at all and
#       were being given a `#pragma once` line that upstream does not have. They are
#       excluded by a *path* convention this repository already uses
#       (`thirdparty/` at the top level), and `modules/mono/thirdparty/` is the same case
#       one directory deeper.

# The change, in one paragraph
#   Skip a file before deciding anything about it if (a) its path contains a `thirdparty/`
#   component, or (b) one of its first six lines says `This file is generated. Do not edit!`.
#   Two `continue`s at the top of the loop, no other behaviour touched: banner recognition,
#   the guard placement check and the `#endif` bookkeeping are exactly as patch 001 left
#   them. The path rule is a substring test rather than a `startswith`, which is what makes
#   `modules/mono/thirdparty/` count; the generated rule is content-based, so a wrapper that
#   moves still declares itself.

# Verification
#   All three rows below are the same command over the same explicit file list
#   (`git ls-files '*.h' '*.hpp' | grep -v '^thirdparty/'`, 1960 paths), executed on copies
#   in a scratch mirror and then compared byte-for-byte against the originals. The tool has
#   no `--dry-run`, so this is the only way to ask the question without answering it.
#
#   | script state | REQUIRES MANUAL CHANGES | files rewritten | exit |
#   |---|---|---|---|
#   | before 001 (`518d68e3`) | 744 | 21 | 1 |
#   | 001 only (the parent commit) | 0 | 21 | 0 |
#   | 001 + 003 (here) | 0 | 0 | 0 |
#
#   - `tests/kaizen/license_box/test_check_license_box.py::test_header_guards_leaves_generated_files_alone`
#     runs the real script over copies of `drivers/alsa/asound-so_wrap.h`,
#     `drivers/pulseaudio/pulse-so_wrap.h` and `modules/mono/thirdparty/coreclr_delegates.h`
#     and requires zero output and zero bytes changed. It covers both halves of the rule:
#     the first two are caught by the generated marker, the third only by the path.
#   - Mutation-checked: with the skip removed and nothing else changed, that test fails and
#     the other 19 stay green. That is how it was written - the failure came first.
#   - The patch applies to the parent commit's blob (`git show HEAD:misc/scripts/header_guards.py`
#     + `patch -p1` + `cmp` reproduces this file exactly), and the sequence
#     `git show 518d68e3:...` + 001 + 003 reproduces it too, which is the proof of the
#     dependency ordering named above.

# Not verified, deliberately
#   - Whether `#pragma once` is *better* for the wrappers is not the question, and the
#     rewrite is not a compile break: counted on `drivers/alsa/asound-so_wrap.h`, the
#     original has 3 `#if`-family and 3 `#endif`, the rewritten copy 2 and 2, so the tool's
#     guess about which trailing `#endif` closed the guard happened to be right, and both
#     files parse. The reason to skip these files is that they are generated in `.h`/`.c`
#     pairs, say so on their second line, match upstream's copies in the line the tool wanted
#     to change, and that the `DYLIBLOAD_WRAPPER_ASOUND` macro is defined nowhere else in the
#     tree (`git grep` returns the wrapper's own two lines) - so the guard is the file's own,
#     not a build knob, and the change buys a diff that the next
#     `generate-wrapper.py` run reverts.
#   - No build was attempted for either variant: nothing here compiles the engine, so
#     "balanced, both parse" is a count of preprocessor lines, not a compiler result.
#   - The tool is still not wired into CI, and this patch does not change that. If it ever
#     is, it should run over `git ls-files` output like the measurement above, not over
#     directories: it takes a directory argument and dies with `IsADirectoryError`
#     (`python3 misc/scripts/header_guards.py servers/audio`), which is a separate
#     ergonomic defect left alone because every caller in this repository passes files.

# Rollback: drop this patch and the tool goes back to rewriting 21 tracked files while
# reporting success - which the test above catches, so a rollback shows up as a red suite.
