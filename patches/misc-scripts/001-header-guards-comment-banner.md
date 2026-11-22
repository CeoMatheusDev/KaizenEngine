# Patch 001 - header_guards.py learns the fork's `//` banner style

# Status: applied
# Priority: P2 (tooling correctness; no engine behaviour)
# Targets: misc/scripts/header_guards.py
# Diff: patches/misc-scripts/001-header-guards-comment-banner.patch

# What this file is
#   The repository ships three header tools in `misc/scripts/` and wires none of them
#   into CI (`grep -rn "copyright_headers|header_guards|file_format" .github/workflows
#   .pre-commit-config.yaml` returns nothing). Of the two that run without a build,
#   `header_guards.py` was broken against this tree: it recognises a licence banner only
#   when the file opens with `/**********`, and 1541 files in this repository open with
#   the `//**********` form instead (see docs/license-notice-consistency.md for what the
#   two styles say, which is a separate question from which one parses). For those files
#   the script concludes there is no banner, finds the legitimate `#pragma once` forty
#   lines below the top, classifies it as misplaced, and prints
#   `REQUIRES MANUAL CHANGES` for all of them.

# The change, in one paragraph
#   Two conditions, both about recognising the banner block: the opener test accepts
#   `//**********` next to `/**********`, and the "not in the header anymore" test stops
#   at the first line that starts with neither `*`, `/*` nor `//`. Nothing else moves.
#   No guard is converted, inserted or removed differently than before; the patch only
#   lets the parser find the end of a banner it previously could not see.

# Why the tool was taught rather than the tree coerced
#   Converting 1541 files to the `/* */` style would delete the derivation and AGPL
#   statement that only the `//` style carries, and converting 2483 files the other way
#   would rewrite licence text across most of the engine. Both are licensing decisions
#   for the project owner, and `docs/license-notice-consistency.md` records them as
#   open. A parser fix is free, so that is what this patch is.

# Verification
#   - Before: `python3 misc/scripts/header_guards.py $(git ls-files | grep -E '\.(h|hpp)$'
#     | grep -v ^thirdparty/)` - 1960 headers, 744 `REQUIRES MANUAL CHANGES`, exit 1.
#     Over the four subsystem areas this audit covers (servers and modules for physics,
#     navigation, audio, plus `scene/audio`, `scene/resources/audio`,
#     `modules/interactive_music`): 230 headers, 93 flagged.
#   - After: same command, 0 flagged, exit 0. The 21 files the script reports as `FIXED`
#     are the same before and after (dynload `*-so_wrap.h` wrappers and
#     `modules/mono/thirdparty/coreclr_delegates.h`, which use legacy `#ifndef` guards and
#     no banner at all) - the patch does not reach them, as it cannot: they have no banner
#     for it to misparse.
#   - `git show HEAD:misc/scripts/header_guards.py` plus `patch -p1` with this diff
#     reproduces the current file byte for byte (`cmp`). `git apply` is not a valid way to
#     run this check.

# Not verified, deliberately
#   - The script is a mutator, not a checker: there is no `--dry-run`, so running it on
#     this tree rewrites those 21 files' guards to `#pragma once` and deletes their
#     closing `#endif`. That is its designed job on legacy guards, and it is correct for
#     engine code, but 18 of the 21 are vendored or generated files. Anyone wiring this
#     into CI should run it on a copy (which is what the `tests/kaizen` suite does) or
#     exclude `thirdparty/` and `*/thirdparty/`, and should not read "exit 0" as "nothing
#     was written".
#   - The `#pragma once` conversion the script performs on those 21 files was not
#     reviewed and is not part of this patch; this patch only changes banner detection.

# Rollback: drop patches/misc-scripts/001-header-guards-comment-banner.patch - the script
# goes back to flagging 744 headers of the fork's own style, which is a false positive but
# also a free reminder that two banner styles exist until the licensing decision above is
# made.
