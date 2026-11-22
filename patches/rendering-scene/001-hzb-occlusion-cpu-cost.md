# Patch 001 — HZB Occlusion Culling CPU Cost

# Status: APPLIED IN TREE, MEASURED OFF-TARGET (2026-11-22)
# Priority: R5 item 2 / large-scene scaling
# Target: servers/rendering/renderer_scene_occlusion_cull.cpp
#         servers/rendering/renderer_scene_occlusion_cull.h

# Two changes to the CPU half of occlusion culling. This is the path a project
# enters as soon as it turns on rendering/occlusion_culling/use_occlusion_culling,
# which is exactly what large open-world and indoor/corridor maps do: the cost is
# paid on the cull thread per viewport per frame, next to the per-instance loop in
# RasterizerSceneCull::_scene_cull().

# 1. HZBuffer::update_mips(): hoist the loop-invariant work out of the texel loop.
#    The upstream body re-reads sizes[mip - 1].width/.height, recomputes the two
#    parity tests and re-indexes mips[mip - 1] for every single texel, and runs
#    two MIN() per sampled offset to clamp against the previous level's edge.
#    Only the last row and the last column can read past that edge, so the clamps
#    are now hoisted to a per-row and per-column branch and the inner loop works
#    with three row pointers. The pyramid covers 4/3 of the buffer area, i.e.
#    2.76M texels per frame for a 1920x1080 occlusion buffer, so this is 2.76M
#    x (4 loads + 2 divisions + 8 MIN) removed, and the simpler body lets the
#    compiler vectorize the max reduction.

#    The restructure is bit-identical, not an approximation: for an even previous
#    dimension the level is w = prev_w / 2, so an interior texel's largest offset
#    is 2 * (w - 2) + 2 = prev_w - 2; for an odd one prev_w = 2 * w + 1 and the
#    offset is prev_w - 4. Both stay inside the row, so MIN() there is a no-op and
#    the fast path returns the same value. The +2 offsets are still read only when
#    the corresponding dimension is odd, exactly as before.

# 2. HZBuffer::_is_occluded(): test the near plane before projecting the corner.
#    The corner was run through p_cam_projection and the result thrown away when
#    -view.z < 0.0 breaks out of the 8-corner loop. Moving the projection after
#    the test saves a matrix multiply per corner for any instance whose AABB
#    straddles the camera plane, which is the normal case for geometry a player
#    walks into, i.e. the indoor case. No semantic change: projected is not read
#    on the break path and xform() has no side effects.

# Measured cost (the only measurement available so far):
#    Both bodies were extracted verbatim into a standalone harness on plain
#    std::vector mip chains and timed on this sandbox's 2-core Xeon @ 2.60GHz
#    (no GPU, no engine build), 1920x1080 pyramid, best of 5 blocks x 30 frames:
#      -O2: 1.274 ms -> 0.720 ms per frame  (-43.5 %, 1.77x)
#      -O3: 1.284 ms -> 0.882 ms per frame  (-31.3 %, 1.45x)
#    Output compared bit-for-bit against the upstream body over 15 fixed sizes
#    (1x1, 2x2, 3x3, 5x7, 1x1080, 1920x1, 1023x1023, 127x255, odd and even
#    chains) plus 40 random sizes up to 2048x2048 with random depth values
#    including FLT_MAX: identical in all 55 cases.
#    These are harness numbers, not in-engine numbers: the engine adds the
#    per-instance test, the raycast module's traversal and the readback around
#    it. Treat the percentage as the size of the win on update_mips() alone.

# Deliberately NOT done:
#    ceil(log2(size)) for the starting LOD in _is_occluded() is a libm call per
#    instance and looks trivially replaceable by reading the float exponent and
#    adding one when the mantissa is non-zero. It is not equivalent: for the
#    smallest float above a power of two, log2f() rounds to exactly that integer
#    and the upstream expression returns one level lower. The result is still a
#    valid LOD (coarser, so more conservative for culling) but it is a behaviour
#    change, and on this path a changed LOD choice changes which instances get
#    culled. Left alone until there is an in-engine A/B to justify it.
#    The dummy/base RendererSceneOcclusionCull keeps update_mips() with no caller
#    outside modules/raycast, so the gain lands in Embree-backed occlusion
#    culling builds (default on x86_64/arm64 desktop), not in the stub.

# Verification:
#   - Extracted function compiles clean under -Wall -Wextra -Wshadow -Wconversion
#     at -O2 and -O3 (0 diagnostics from the function body itself).
#   - Bit-identity: 55/55 mip chains, see above.
#   - Outstanding: a full engine build. The sandbox has no scons installed and no
#     generated *.gen.h headers, so the two edited files were never fed to the real
#     build; the extracted-body compile is a weaker check. Needs a large-scene frame
#     capture with occlusion culling on to confirm the win survives the code around it.

# Rollback: drop patches/rendering-scene/001-hzb-occlusion-cpu-cost.patch.
