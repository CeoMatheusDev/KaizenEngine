# Licence notice consistency audit

Audited 2026-11-22, offline, over the four subsystems named in the milestone-0.2 work
(collisions, NPC AI/navigation, audio) and measured tree-wide for comparison. Everything
here is a count produced by reading files in this repository, not an inference: the
measurement script is `tests/kaizen/license_box/check_license_box.py` and it is run by
`./tests/kaizen/run_tests.sh`.

This document records facts and one open decision. It does not change any licence text,
and it makes no legal claim; the differences below are stated as textual facts about the
tree, and the decision of which notice the project ships belongs to the maintainers.

## 1. Two banner styles, both current

A source file in this repository opens with one of two banners:

**Style A** - Godot's `/* */` box, brand line rewritten:

```c
/**************************************************************************/
/*  audio_bus_layout.cpp                                                  */
/**************************************************************************/
/*                         This file is part of:                          */
/*                             KAIZEN ENGINE                               */   <- 77 wide
/*                        https://godotengine.org                         */
/**************************************************************************/
/* Copyright (c) 2014-present Kaizen Engine contributors (see AUTHORS.md). */   <- 77 wide
/* Copyright (c) 2007-2014 Juan Linietsky, Ariel Manzur.                  */
   ... MIT permission text ...
```

**Style B** - a `//` box, with a derivation and AGPL statement:

```c
//************************************************************************//
//  audio_bus_layout.h                                                    //
//************************************************************************//
//                         This file is part of:                          //
//                            KaizenEngine                                //
//              https://github.com/CeoMatheusDev/Kaizen                   //
//************************************************************************//
// Copyright (c) 2007-2014 Juan Linietsky, Ariel Manzur.                  //
// Copyright (c) 2014-present Godot Engine contributors.                  //
// Copyright (c) 2026 Matheus A. Santos, Hardion Labs.                    //
...
// This file contains code derived from the Godot Engine                  //
// (https://godotengine.org), which is licensed under the MIT License.    //
// The original MIT notice is preserved below.                            //
// Modified by Matheus A. Santos, Hardion Labs on 2026-11-17.             //
// Modifications and additional code are licensed under the GNU Affero    //
// General Public License v3.0 (AGPL-3.0).                                //
// See the files LICENSE.txt and LICENSE-ADDITIONAL-TERMS.md for details. //
```

Counts (all tracked code files, extensions `.c .cpp .h .hpp .m .mm .inc .kt .java .js
.swift .glsl .metal`):

| Group | Style A | Style B |
|---|---|---|
| Whole tree | 2483 (9 of them under `thirdparty/`) | 1541 |
| Collisions (`servers/physics_*`, `modules/kaizen_physics_*`, `modules/jolt_physics`) | 166 | 78 |
| Navigation / NPC AI (`servers/navigation_*`, `modules/navigation_*`) | 78 | 20 |
| Audio (`servers/audio`, `scene/audio`, `scene/resources/audio`, `modules/interactive_music`) | 13 | 72 |

Style B exists only in `.c`, `.cpp`, `.h`. Every `.kt`, `.java`, `.js`, `.swift`, `.mm`,
`.inc` and `.glsl` file with a banner is style A - the platform glue and shader sources
never got the second pass.

## 2. What the two styles disagree about

Five textual differences, none of them cosmetic in the legal sense:

1. **Attribution line.** A rewrites the upstream copyright line's holder
   ("`Godot Engine contributors`" to "`Kaizen Engine contributors`") and keeps one line.
   B keeps the Godot line, adds a Hardion line, and states the derivation explicitly.
2. **`LICENSE-MIT` vs the headers.** `LICENSE-MIT` preserves the upstream notice verbatim
   ("Copyright (c) 2014-present Godot Engine contributors (see AUTHORS.md)."), so style A's
   headers and `LICENSE-MIT` name different holders for the same code.
3. **Licence named.** B names AGPL-3.0 for the modifications and points at `LICENSE.txt`
   and `LICENSE-ADDITIONAL-TERMS.md`; A names only the MIT text, so an A-only reader sees no
   AGPL claim at all. `LICENSE.txt` is the AGPL-3.0 text.
4. **Project URL.** A's brand line still points at `https://godotengine.org`; B points at
   the fork's repository. `CONTRIBUTING.md` documents no banner rule at all
   (`grep -niE "header|copyright|license" CONTRIBUTING.md` is empty), so nothing in the
   contributing guide disambiguates them.
5. **Age of the text.** The seven `.kt` files under `platform/android/java/editor/src/*/java/org/kaizenengine/editor/`
   carry a third variant inside style A: a 75-wide box, `Copyright (c) 2014-2022 Kaizen
   Engine contributors (cf. AUTHORS.md).` - a fixed end year and `cf.` instead of
   `see`, i.e. copied from a Godot 4.2-era header.

## 3. Geometry: the box is not rectangular, and why

Style A's box is 76 columns; every row is 76 **except**:

| Row | Files not at the box width | Cause | Fixable by padding alone? |
|---|---|---|---|
| 1 (filename) | 191 (183 at 77, 7 at 76 in the 75-wide box, 1 at 92) | `godot_` to `kaizen_` in the file's own name (+1 char); the `.js` one was re-padded wrongly | yes |
| 4 (brand) | 2483 (2476 at 77, 7 at 76 in the 75-wide box) | `GODOT ENGINE` to `KAIZEN ENGINE` (+1 char) | yes |
| 7 (contributors) | 2476 at 77 | `Godot Engine contributors` to `Kaizen Engine contributors` (+1 char) | **no** |
| 8 (contributors, the seven `.kt`) | 7 at 76 in a 75-wide box | same rename in the older box | yes |

Padding can only work where there is padding: rows 1 and 4 have a run of spaces before
the closing `*/`, so one can be removed. Row 7 has none - the upstream line fills the box
exactly, and the fork's longer replacement text is 77 columns of text, so the row cannot
be brought to 76 without editing words.

Total re-padded in this pass: **2710 rows across 2483 files** (2681 by the first sweep,
29 more by `--fix`, which found short rows inside Style B brand blocks, the four
`.compat.inc` cases, and two rows in this repository's own test files). The generator
template `misc/scripts/copyright_headers.py` got the same fix, which is what made the
defect invisible: its own template carried it, so generator and tree agreed on being
ragged, and no tool ever complained.

**2469 rows are left ragged on purpose** - the contributors line in Style A files (and the
equivalent line at row 8 of the seven `.kt` files, whose box is 75). Padding cannot reach
them; see section 4. The number is not free-floating: it is the budget in
`tests/kaizen/run_tests.sh` (`--max-pending 2469`) and the `BUDGET` in
`tests/kaizen/license_box/test_check_license_box.py`, so growing it fails CI and paying it
down passes with a smaller number.

## 4. The open decision

Three ways to close the last 2476 rows. All are mechanical once one is chosen, and none
changes a line of engine behaviour:

1. **Restore the upstream attribution line** in style A (`Copyright (c) 2014-present Godot
   Engine contributors (see AUTHORS.md).`). It is 76 columns, so the box becomes
   rectangular with zero padding edits, `LICENSE-MIT` and the headers agree, and the
   fork's claim stays where B puts it - in an added line rather than a rewritten one.
2. **Widen style A's box to 77.** Pure whitespace, keeps the current wording, but it adds
   a space to ~24 rows in 2476 files (~59 000 lines) for a banner, and it makes A and B
   differ in width as well as in content.
3. **Shorten the wording** (drop ` (see AUTHORS.md)`, as style B does). Rectangular at 76
   and the smallest diff, but it removes the pointer to the author list that the upstream
   notice carries.

Option 1 is the one this audit would pick on the evidence above, for the narrow reason
that it is the only one that makes the headers agree with `LICENSE-MIT` without inventing
text. It is not applied here because rewriting a copyright line is not a call to make in
a quality pass; `git show 0f541294` is the source of the current wording.

## 5. Two provenance notes from the same sweep

- `modules/interactive_music` is **not** a Kaizen-authored audio stack: upstream
  `godotengine/godot` at ref `4.5` has a module by that exact name, and the three classes
  it registers (`AudioStreamInteractive`, `AudioStreamPlaylist`, `AudioStreamSynchronized`)
  have module-local `doc_classes/` there rather than root `doc/classes/` entries, which is
  what this tree has too. Whether the file contents diverge from upstream is a separate
  measurement this audit did not perform.
- `misc/scripts/copyright_headers.py` regenerated a banner from its own `/* */` template,
  which meant running it over a Style-B file replaced 40 lines of notice with 25 and
  deleted the derivation statement, the AGPL-3.0 line and the pointer to `LICENSE.txt`
  (`-30` lines per file on the three sampled). It has no `--dry-run`. That is now
  `patches/misc-scripts/002-copyright-headers-keep-slash-banner.patch`, and the tests in
  `tests/kaizen/license_box/` run the real script over copies of real files, so both
  directions are pinned: a no-op on Style A (proof that template and tree agree) and a
  refusal on Style B.
- The physics module rename is not cosmetic, and `docs/upstream-differences.md` understated
  it: `modules/godot_physics_2d` -> `modules/kaizen_physics_2d` came with
  `GDCLASS(KaizenPhysicsServer2D, PhysicsServer2D)` and with the engine-selection string
  `"KaizenPhysics2D"` (upstream: `GodotPhysics2D`), i.e. class names and stored project
  values, not just paths. Navigation registers `"KaizenNavigation2D"`/`"KaizenNavigation3D"`
  the same way. Both were checked for leftovers this pass: no `GodotPhysics*` or
  `GodotNavigation*` string survives anywhere in code, docs, editor, tests or `*.py`, and
  each manager registers its default server, so a project carrying an unknown engine name
  falls back to the default rather than failing.
- The fork describes its base as Godot `4.8.0-beta`. Upstream has no `4.8` ref
  (`GET /repos/godotengine/godot/contents/...?ref=4.8` is 404; `4.5` and `master` are
  live), so claims about "what upstream says" in this document are made against `4.5` and
  labelled as such.

## 6. What is enforced from here

| Rule | Where |
|---|---|
| Every banner row equals the file's own border width, or is a recorded pending row | `tests/kaizen/license_box/check_license_box.py`, run by `./tests/kaizen/run_tests.sh` with `--max-pending 2469` |
| `--fix` may only move spaces | the tool re-reads the file and compares with all spaces removed before writing; a non-space difference aborts |
| The generator template matches the tree | `test_generator_is_a_noop_on_a_box_file` |
| The generator leaves Style B alone | `test_generator_does_not_touch_slash_banner_files` |
| `header_guards.py` understands both banner shapes | `test_header_guards_accepts_both_banner_styles` |
| The template has exactly one over-wide row, and it is the contributors line | `test_template_box_is_rectangular_except_the_licence_line` |

Rows are classified by whether padding can reach them, not by row index, so a file that
legitimately has a narrower box (the seven `.kt` files at 75) is checked against its own
border, and a row containing a tab or a carriage return is reported as `unsafe` and never
rewritten.
