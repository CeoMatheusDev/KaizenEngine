#!/usr/bin/env python3
"""Lift the CPU-side shadow kernel and the shader's own filter constants into one header.

Usage:
    python3 extract_source_bodies.py --repo-root PATH --out FILE

Two jobs, and both are the reason this runs before the test is compiled.

(1) The body of `get_vogel_disk()` is copied verbatim out of
    `servers/rendering/renderer_rd/renderer_scene_render_rd.cpp`. A kernel that a test carries
    as its own copy is a kernel that can drift away from the engine unnoticed, so the engine
    source is the only place it is written down. If the function moves or changes shape, this
    step fails instead of quietly testing a remembered table.

(2) The numbers the shadow filter test would otherwise hard-code are parsed out of the three
    sources that define them: the sample counts the engine uploads (renderer_scene_render_rd.cpp),
    the kernel array size the UBO declares (scene_data_inc.glsl), and the blocker-search stride
    rule and its threshold (scene_forward_lights_inc.glsl). The test then measures what the
    engine actually does, and a change in any of the three either moves the numbers with it or
    trips the consistency checks emitted here.
"""

from __future__ import annotations

import argparse
import os
import re
import sys

SOURCE_KERNEL = "servers/rendering/renderer_rd/renderer_scene_render_rd.cpp"
SOURCE_UBO = "servers/rendering/renderer_rd/shaders/scene_data_inc.glsl"
SOURCE_SHADER = "servers/rendering/renderer_rd/shaders/scene_forward_lights_inc.glsl"

KERNEL_ARRAYS = (
    "directional_penumbra_shadow_kernel",
    "directional_soft_shadow_kernel",
    "penumbra_shadow_kernel",
    "soft_shadow_kernel",
)

# `penumbra_shadow_samples = 24;` and `directional_penumbra_shadow_samples = 24;` inside the two
# quality switches. Matching both prefixes on purpose: the positional and the directional filter
# are set in separate functions and the test sweeps the union.
SAMPLE_COUNT = re.compile(r"^\s*(?:directional_)?(?:penumbra|soft)_shadow_samples\s*=\s*(\d+);", re.M)

# `vec4 penumbra_shadow_kernel[32];`
UBO_ARRAY = re.compile(r"vec4\s+(\w*shadow_kernel)\s*\[(\d+)\]\s*;")

# `const uint blocker_stride = directional_penumbra_samples > 4 ? 2u : 1u;`
STRIDE_RULE = re.compile(r"blocker_stride\s*=\s*\w+\s*>\s*(\d+)\s*\?\s*(\d+)u\s*:\s*(\d+)u")


def read(path):
    with open(path, "r", encoding="utf-8") as handle:
        return handle.read()


def extract_body(text, source_path):
    start = re.search(r"^\s*(?:static\s+)?void\s+get_vogel_disk\s*\(", text, re.M)
    if start is None:
        raise SystemExit(
            f"error: `get_vogel_disk` not found in {source_path}.\n"
            "       The engine function moved or changed signature; update extract_source_bodies.py\n"
            "       and the note in patches/renderer-rd/001-shadow-pcss-blocker-and-pcf-kernel.md together."
        )
    open_at = text.index("{", start.start())
    depth = 0
    for index in range(open_at, len(text)):
        if text[index] == "{":
            depth += 1
        elif text[index] == "}":
            depth -= 1
            if depth == 0:
                return text[start.start() : index + 1]
    raise SystemExit("error: unbalanced braces in get_vogel_disk")


def main(argv=None):
    parser = argparse.ArgumentParser(description="Generate the shadow filter test header.")
    parser.add_argument("--repo-root", required=True)
    parser.add_argument("--out", required=True)
    args = parser.parse_args(argv)

    paths = {name: os.path.join(args.repo_root, name) for name in (SOURCE_KERNEL, SOURCE_UBO, SOURCE_SHADER)}
    for name, path in paths.items():
        if not os.path.exists(path):
            raise SystemExit(f"error: {name} not found (bad --repo-root?)")

    kernel_text = read(paths[SOURCE_KERNEL])
    ubo_text = read(paths[SOURCE_UBO])
    shader_text = read(paths[SOURCE_SHADER])

    body = extract_body(kernel_text, SOURCE_KERNEL)

    counts = sorted({int(m) for m in SAMPLE_COUNT.findall(kernel_text)})
    if not counts:
        raise SystemExit(f"error: no shadow sample counts parsed from {SOURCE_KERNEL}")
    if counts[0] < 0 or counts[-1] > 256:
        raise SystemExit(f"error: implausible shadow sample counts parsed: {counts}")

    arrays = dict((name, int(size)) for name, size in UBO_ARRAY.findall(ubo_text))
    missing = [name for name in KERNEL_ARRAYS if name not in arrays]
    if missing:
        raise SystemExit("error: kernel arrays not found in {}: {}".format(SOURCE_UBO, ", ".join(missing)))
    sizes = {arrays[name] for name in KERNEL_ARRAYS}
    if len(sizes) != 1:
        raise SystemExit(f"error: the four shadow kernels do not share an array size: {arrays}")
    capacity = sizes.pop()

    rules = STRIDE_RULE.findall(shader_text)
    if not rules:
        raise SystemExit(
            f"error: no blocker_stride rule parsed from {SOURCE_SHADER}.\n"
            "       The stride the test measures was removed or rewritten; update the test and\n"
            "       patches/renderer-rd/001-shadow-pcss-blocker-and-pcf-kernel.md together."
        )
    if len({tuple(r) for r in rules}) != 1:
        raise SystemExit(f"error: the blocker searches disagree about the stride rule: {sorted(set(rules))}")
    threshold, stride_when_over, stride_other = (int(v) for v in rules[0])

    if counts[-1] > capacity:
        raise SystemExit(
            f"error: {SOURCE_KERNEL} uploads {counts[-1]} samples but {SOURCE_UBO} holds "
            f"{capacity} - the shader would read past the array."
        )

    facts = [
        "// --- parsed facts ---",
        f"#define SHADOW_UBO_KERNEL_SLOTS {capacity} // vec4 rows in {SOURCE_UBO}",
        f"#define SHADOW_STRIDE_THRESHOLD {threshold} // {SOURCE_SHADER}",
        f"#define SHADOW_STRIDE {stride_when_over} // applied above the threshold",
        f"#define SHADOW_STRIDE_BELOW {stride_other} // at or below it",
        f"#define SHADOW_STRIDED_LOOPS {len(rules)} // blocker searches using the rule",
        "static const int SHADOW_SETTING_SAMPLES[] = {{ {} }};".format(", ".join(str(c) for c in counts)),
        f"#define SHADOW_SETTING_SAMPLE_COUNT {len(counts)}",
        f"#define SHADOW_MAX_SAMPLES {counts[-1]}",
    ]
    header = [
        "// Generated by tests/kaizen/shadow_filter/extract_source_bodies.py - do not edit.",
        "// The kernel body is the engine's, verbatim; the numbers are parsed from the three",
        "// sources that define them, so the test follows the engine instead of restating it.",
        "#pragma once",
        "",
        "#include <cmath>",
        "#include <cstddef>",
        "",
        f"// --- {SOURCE_KERNEL} ---",
        "namespace Math {",
        "inline float sqrt(float p_v) { return std::sqrt(p_v); }",
        "inline float cos(float p_v) { return std::cos(p_v); }",
        "inline float sin(float p_v) { return std::sin(p_v); }",
        "} // namespace Math",
        "",
        body,
        "",
    ]
    with open(args.out, "w", encoding="utf-8") as handle:
        handle.write("\n".join(header + facts) + "\n")

    print(
        f"extracted get_vogel_disk + parsed facts: settings {counts}, UBO slots {capacity}, "
        f"stride {stride_when_over} above {threshold} ({len(rules)} loops)"
    )
    if capacity == counts[-1]:
        print("note: the largest setting exactly fills the UBO array - no headroom for a bigger one.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
