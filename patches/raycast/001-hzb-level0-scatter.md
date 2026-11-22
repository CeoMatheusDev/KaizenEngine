# Patch 001 — HZB Level-0 Scatter (sort_rays)

# Status: APPLIED IN TREE, MEASURED OFF-TARGET (2026-11-22)
# Priority: R5 item 2 / large-scene scaling
# Target: modules/raycast/raycast_occlusion_cull.cpp
#         (RaycastOcclusionCull::RaycastHZBuffer::sort_rays)

# The other half of the CPU cost of occlusion culling, and the bigger half by
# texel count. patches/rendering-scene/001-hzb-occlusion-cpu-cost.md optimises
# HZBuffer::update_mips(), which builds the levels ABOVE level 0 - about one third
# as many texels as the base level. sort_rays() writes level 0 itself: one texel per
# camera ray, i.e. the whole base level per viewport per frame, roughly three times
# the texel count of that mip build.

# The function scatters Embree's tiled SoA ray results (16 rays per 4x4 tile,
# RTCRayHit16) into the linear level-0 buffer. Upstream recomputes, for every
# single texel, the two multiplies that locate its tile (tile_index = i *
# tile_grid_size.x + j) and its slot inside the tile (k = tile_i * TILE_SIZE +
# tile_j), the two multiplies that locate the destination row, and a pair of
# bounds tests. None of that depends on the texel except as an offset within a
# tile.

# 1. Hoist the per-tile and per-row derivation, and replace the per-texel bounds
#    test with clamped loop counts. Out-of-range texels can only ever be trailing
#    columns of the rightmost tile or trailing rows of the bottommost one, because
#    x grows with tile_j and y grows with tile_i; so MIN(TILE_SIZE, extent - base)
#    covers exactly the texels the tests used to accept, and a tile whose first row
#    or column is already past the buffer breaks instead of being visited. Same
#    addresses, same source index per address, no change to any value.
#
#    Why the break is equivalent rather than merely safe: for a tile with
#    y0 = i * TILE_SIZE >= buffer height, every larger i is also out of range, so
#    continuing to loop would test and skip the same condition TILE_SIZE times per
#    tile with no write. tile_grid_size is ceil(extent / TILE_SIZE), so at most one
#    row band and one column band are partial, which is what the clamps assume.

# 2. Not changed, and worth recording so the next reader does not "find" it:
#    sort_rays() ignores both of its parameters (p_camera_dir, p_orthogonal) - the
#    name says sort, the body is a transpose. Cleaning that up touches the header
#    and the call site for no measured gain, so it stays out of a performance
#    patch. Same for the ray-generation loop's (float(x) + 0.5f) / buffer_size
#    divisions: replacing a division by a reciprocal multiply is NOT bit-identical
#    outside power-of-two extents, and this path has no tolerance for a texel that
#    flips visibility.

# Measurement (offline, this sandbox has no GPU and no engine build; the two bodies
# were extracted verbatim - the new one from the tree, the old one from git - into
# one translation unit with stub types and compiled together):
#
#   Equivalence: 4830 cases byte-for-byte identical - every size from 1x1 to 40x40
#   (partial tiles in both axes, 1-wide, 1-first, primes), plus 147x83, 83x147,
#   295x166, 960x540, 1920x1080, 1024x1, 1x1024, 41x1021, 5x3, 13x271, three random
#   tile fill patterns each. The destinations were poisoned differently before each
#   run, so an unwritten texel would have shown up: the harness caught exactly that
#   on its first iteration, before the buffer size was wired, which is the evidence
#   that the comparison is not vacuous.
#
#   Timing, 2-core Xeon @ 2.60 GHz, best of 5 blocks x 60 frames, microseconds per
#   viewport per frame:
#
#     size        upstream -O3    this -O3    delta    upstream -O2   this -O2   delta
#     147x83          5.3 us        2.9 us     -46 %       10.3 us      9.4 us    -9 %
#     295x166        21.5 us       12.6 us     -42 %       41.6 us     37.2 us    -10 %
#     960x540       256.0 us      196.0 us     -23 %      452.0 us    399.0 us    -12 %
#     1920x1080    1217.0 us     1056.0 us     -13 %     2014.0 us   1756.0 us    -13 %
#
#   This fork compiles with -O3 (methods.py:force_optimization_on_debug), so the -O3
#   columns are the ones that describe a build. The shape is worth understanding
#   before quoting either: the win is largest where the buffer still fits in cache
#   and the cost is instruction count, and it decays toward the memory-bound floor as
#   the working set grows. At the engine's DEFAULT ray budget (147x83 at 24 threads)
#   that means about 2.4 us saved per viewport per frame, next to the ~3 us from the
#   mip-build patch - the two together are single-digit microseconds, which is what
#   this path is actually worth, and both numbers are dominated by the Embree ray
#   cast in the same buffer_update() call.

# Reproducing: git diff of modules/raycast/raycast_occlusion_cull.cpp, applied with
# `patch -p1` onto the pre-change tree; verified to reproduce the file byte-for-byte.
# Note that `git apply` from a subdirectory resolves paths against the repository
# root, so it is not a valid way to run this check.
