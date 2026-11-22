# Patch 002 - copyright_headers.py stops destroying the fork's `//` licence notice

# Status: applied
# Priority: P1 (silent loss of licence text, one command away)
# Targets: misc/scripts/copyright_headers.py
# Diff: patches/misc-scripts/002-copyright-headers-keep-slash-banner.patch

# What this file is
#   `misc/scripts/copyright_headers.py` regenerates a file's banner from its own template:
#   it skips the leading comment block and writes the template in its place. It has no
#   --dry-run. The repository has two banner shapes (docs/license-notice-consistency.md),
#   and the template is the `/* */` one - so running the tool over a file that carries the
#   `//` banner replaces 40 lines of notice with 25, deleting the derivation statement, the
#   AGPL-3.0 declaration, the Hardion copyright line and the pointer to LICENSE.txt and
#   LICENSE-ADDITIONAL-TERMS.md. Measured on copies of three such files
#   (`servers/audio/audio_effect.cpp`, `scene/audio/audio_stream_player.cpp`,
#   `servers/physics_3d/physics_server_3d.h`): -30 lines of licence notice each, exit 0,
#   no warning. 1541 files in the tree are exposed to that, and the trigger is as ordinary
#   as someone adding the usual `static-checks` workflow or tidying headers in a subsystem.

# The change, in one paragraph
#   Before doing anything with a file, look at its first line; if it opens a `//` banner
#   (`//***`), leave the file alone and continue. No other behaviour changes: the tool still
#   rewrites `/* */` banners from its template exactly as before. The test that pins this is
#   `test_generator_does_not_touch_slash_banner_files` in tests/kaizen/license_box/, which
#   runs the real script over a copy of a real Style-B file and requires the bytes to be
#   unchanged.

# Why skip rather than teach the tool the second style
#   Emitting a `//` banner from this template means the tool has to know which notice a file
#   deserves, and that is the open licensing decision, not a code question. Refusing to act
#   is the correct behaviour for a formatter in the meantime, and it is also what upstream's
#   own tool does for files whose header it cannot recognise (it aborts header magic rather
#   than guessing).

# Verification
#   - `python3 misc/scripts/copyright_headers.py` over copies of six files, three Style B and
#     three Style A (including `thirdparty/enet/enet_kaizen.cpp`): every copy byte-identical
#     to its original afterwards. The Style-A half is the useful direction of the check - the
#     template and the tree now agree exactly, because the same commit fixed the template's
#     own off-by-one box width - so "no-op" means "correct", not "did nothing".
#   - tests/kaizen/license_box/test_check_license_box.py: two tests run the real script over
#     copies of real repository files (a Style-A one must come back unchanged, a Style-B one
#     must not be rewritten), so the notice-destruction path is guarded, not just fixed.
#   - `git show HEAD:misc/scripts/copyright_headers.py` plus `patch -p1` with this diff
#     reproduces the current file byte for byte (`cmp`); patches 001 and 002 apply in either
#     order since they touch different files, verified the same way.

# Not verified, deliberately
#   - The tool still rewrites a Style-A file's whole leading comment block, so any
#     hand-written comment row that belongs to the banner (a note between the border lines)
#     is replaced by the template. That is upstream's design and is unchanged here; the
#     banner rows in the tree now match the template, so today the rewrite is a no-op, which
#     is what the check above measures. If someone adds a banner note that the template does
#     not have, `--fix` in tests/kaizen/license_box is the thing to run, not this script.
#   - No attempt is made to make the two styles agree in content. That is section 4 of
#     docs/license-notice-consistency.md, and it needs a maintainer's decision.

# Rollback: drop patches/misc-scripts/002-copyright-headers-keep-slash-banner.patch - the
# generator goes back to replacing `//` banners with the box template, and the two tests above
# go red with it.
