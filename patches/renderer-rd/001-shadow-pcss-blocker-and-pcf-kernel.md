# Patch 001 — PCSS Blocker Search and PCF Kernel Shaping

# Status: APPLIED IN TREE, COST UNMEASURED (2026-11-22)
# Priority: R5 item 2 ("improved shadow filtering and cascades")
# Target: servers/rendering/renderer_rd/shaders/scene_forward_lights_inc.glsl

# Two independent changes to the shared Forward+/Forward-Mobile shadow
# sampling helpers. Both are confined to the include file, so clustered,
# mobile and both shadow paths pick them up without touching any C++ code.

# 1. Blocker search runs at half the penumbra sample count (PCSS).
#    sample_directional_soft_shadow(), light_process_omni(),
#    light_process_spot(), light_process_area().
#    The blocker loop only needs an average occluder depth; the expensive
#    part is the filtered penumbra loop that follows it, which keeps its
#    full sample count. Because get_vogel_disk() orders samples by
#    increasing radius (r = sqrt((i + 0.5) / n)), the reduced set is
#    gathered with a stride of 2 rather than truncating the array: taking
#    the first half would cover only r <= 0.707 of the disk and bias the
#    average blocker depth towards the receiver, collapsing the estimated
#    penumbra. Striding keeps the disk coverage at the same cost, measured rather than
#    asserted (tests/kaizen/shadow_filter, 2026-11-22): the strided set probes a footprint
#    0.9354..1.0000 as wide as the full kernel's, where the first half would probe 0.7071
#    and the second half 1.2247. It is not literally full coverage - striding over an even
#    count drops the outermost tap - which is why the 0.93 floor is now a checked number
#    instead of a sentence.
#    Gated on penumbra samples > 4, so Hard (4) and Very Low (4) quality
#    keep the upstream single loop unchanged.

# 2. PCF kernel radius doubles when the tap count is above 8.
#    sample_directional_pcf_shadow(), sample_pcf_shadow() and
#    sample_omni_pcf_shadow().
#    At High (16 taps) and Ultra (32 taps) the extra taps are currently
#    spent shrinking noise over the same radius. Widening the radius
#    instead turns them into a softer, better-starred penumbra at no extra
#    texture cost. Low (4) and Medium (8) are bit-identical to upstream, so
#    the published S4 baselines (pinned at quality 2) stay comparable.

# Quality gate / fallback:
#   Both changes derive their behaviour from the existing
#   rendering/textures/canvas_textures/default_texture_filter shadow
#   quality setting (RSE::ShadowQuality), which is already baked into the
#   shader through the sc_penumbra_shadow_samples() and
#   sc_soft_shadow_samples() specialization constants. No new variant is
#   introduced, so the pipeline key count does not grow. Setting the
#   quality back to Hard/Very Low (1) and (2) is the runtime fallback for
#   change 1, Low/Medium for change 2. The complete fallback is to drop
#   patches/renderer-rd/001-shadow-pcss-blocker-and-pcf-kernel.patch.

# Why not a kaizen/ subsystem:
#   Shadow *policy* (atlas budgeting, cascade count) can live in
#   servers/rendering/kaizen/shadows, but the sample count and kernel shape
#   are consumed inside the fragment loop; there is no engine boundary that
#   would let a Kaizen module override them per light. Keeping it as an
#   isolated upstream patch preserves rebaseability.

# Measured cost:
#   NOT measured. The sandbox has no GPU, so no S4 run exists for this
#   patch. Expected direction on the reference GPU (RX 550, Forward+,
#   1920x1080): a reduction in the soft-shadow pixel cost at quality 2 and
#   above, where S4 is GPU-bound (7.55 ms at 32 spots + 64 omnis vs 2.66 ms
#   with no positional lights). Do not quote a number until
#   benchmarks/rd_3d S4 is re-run with --bench-variant shadows.

# Verification:
#   - tests/kaizen/shadow_filter/ (run by ./tests/kaizen/run_tests.sh) measures the claim in
#     change 1 and the cost of change 1's trade, from the kernel the engine uploads
#     (get_vogel_disk, extracted verbatim) and the constants parsed out of the three sources.
#     Worst per-setting penumbra-width error against sampling every tap: 8 taps 1.81x,
#     12 taps 2.24x, 16 taps 2.48x, 24 taps 2.39x, 32 taps 2.35x, 4 taps 1.00x (the stride
#     is not applied there). Halving the taps again would cost 4.99x, which is the reason the
#     gate is set at 3x and the reason it has teeth.
#   - Two rewrites that look strictly better were measured and rejected. Scaling the probe
#     radius by 1/0.9354 makes the footprint exact and moves the error by 0.0062 at most -
#     neutral, so the shader carries no compensation constant. Spreading the same taps over
#     0..count-1 (index round(i*(n-1)/(m-1)), no constant at all) also makes the footprint
#     exact and costs 0.0268 of error, which is a bad trade for a search pass.
#   - Balanced braces/parens and no name collisions checked in the edited
#     functions; the file is not compiled here (no GPU, no shader build).
#   - Outstanding: glslangValidator/spirv-clean compile of
#     scene_forward_clustered.glsl and scene_forward_mobile.glsl at
#     penumbra sample counts 4, 8, 12, 24 and 32.
#   - Outstanding: S4 A/B (quality 2 and 3), visual check of contact
#     hardening on a caster grid, and a check that self-shadow acne does
#     not regress with the wider High/Ultra kernel.

# Known risks:
#   - The UBO declares vec4 <name>_shadow_kernel[32] and SHADOW_QUALITY_SOFT_ULTRA already
#     uploads 32 penumbra samples: the array is exactly full. Any future setting above 32
#     reads past it, so tests/kaizen/shadow_filter/extract_source_bodies.py compares the two
#     and refuses to generate (verified: with a 40-sample setting hand-inserted it fails with
#     "uploads 40 samples but ... holds 32"). Rebase onto an upstream that grows either side
#     needs no action here; one that grows only the C++ side will fail loudly.
#   - Fewer blocker taps means a noisier average occluder depth on
#     low-frequency geometry; the rotation is already TAA-jittered, which
#     hides most of it, but a scene with a single large caster may show
#     penumbra width flicker.
#   - Widening the kernel without touching depth bias can expose acne on
#     grazing surfaces at High/Ultra if a project also lowered
#       shadow_bias; that is a per-light tuning matter, not a default change.

# Engine defaults are deliberately NOT changed: positional and directional
# soft_shadow_filter_quality stay at upstream 2 (Soft Medium) in
# servers/rendering/rendering_server.cpp. Raising the default to Soft High
# doubles the penumbra taps (12 -> 24) and the PCF taps (8 -> 16) for every
# project, which is the wrong trade on the low-end target this branch
# benchmarks against. Projects that want change 2 opt in per project.
