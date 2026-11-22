# Patch 002 — Camera Ray Generation Cost

# Status: APPLIED IN TREE, MEASURED OFF-TARGET (2026-11-22)
# Priority: R5 item 2 / large-scene scaling
# Target: modules/raycast/raycast_occlusion_cull.cpp
#         (RaycastOcclusionCull::RaycastHZBuffer::_generate_camera_rays)
# Applies cleanly before or after patches/raycast/001-hzb-level0-scatter.patch - both
# orders were tested and both reproduce this tree - since the two hunks touch
# different functions.

# The third and largest CPU term of the per-frame occlusion pass. In
# RaycastOcclusionCull::buffer_update() the work is: generate one camera ray per
# texel of the base level, cast them through Embree, scatter the results into level 0
# (patch 001), then build the mips above it
# (patches/rendering-scene/001-hzb-occlusion-cpu-cost.md). Generating is 1 ray per
# texel too, but each ray costs a normalised direction - a sqrt and three divisions -
# plus two divisions for u and v, where the scatter is a couple of adds. That makes
# it the term worth attacking, not the mip build: measured below, the saving here is
# seven to eight times the saving of the other two patches combined at the engine's
# default ray budget.

# 1. Copy the CameraRayThreadData fields out of the pointer once per call.
#    They are loop-invariant, but the ray writes go to camera_rays, a memalloc'd
#    buffer the compiler has no alias information about: p_data is const-qualified,
#    which does not mean the memory it points to is not written through another
#    pointer in the same loop. So upstream reloads pixel_corner, the two interp
#    vectors, camera_pos, camera_dir, z_near and z_far for every one of the
#    TILE_RAYS rays of every tile. Hoisting them into locals says what is true.
#    Copies, not references: a reference would still be a load through the pointer.
#
# 2. Tabulate u and v per tile instead of dividing per ray.
#    u depends only on the column within the tile and v only on the row, so
#    2 * TILE_RAYS divisions per tile become 2 * TILE_SIZE, and each tabulated value
#    is the identical division with the identical operands. They are deliberately not
#    computed as an incremental step along the tile: (x + 1.5f) / w is not the same
#    rounding as (x + 0.5f) / w + 1 / w, and no reciprocal multiply is used anywhere
#    for the same reason.
#
# 3. Walk the tile as row x column instead of a flat 0..TILE_RAYS index with j % and
#    j / inside it. j == row * TILE_SIZE + column enumerates the same indices in the
#    same order, and since TILE_RAYS is TILE_SIZE * TILE_SIZE, j % TILE_SIZE and
#    j / TILE_SIZE are exactly column and row - so this is the same 16 slots, and it
#    is what lets the per-row v be loaded once. The SoA writes still go to tile.ray.*[j].

# Measurement (offline; the two bodies were extracted verbatim - the new one from the
# tree, the old one from git - into one translation unit with a Vector3 stub that
# mirrors core/math/vector3.h: float components, normalize() with the is_finite()
# guard, length via a double sqrt, and tnear divided after being stored, as upstream
# does, so the store/load round trip is preserved):
#
#   Equivalence: 7020 cases byte-for-byte identical - every buffer size from 1x1 to
#   34x34 in both axes (partial tiles, 1-wide, 1-tall, primes) plus 147x83, 83x147,
#   295x166, 960x540, 1920x1080, 4x4, 7x9, 1022x1026, each against 6 input variants:
#   perspective and orthogonal, coordinates at 1e-3 and 5e2 scale, degenerate interp
#   vectors (all zero), and a zero camera_dir so tnear divides by zero. Both tile
#   arrays were poisoned with the same non-zero pattern before each run, so a slot one
#   body wrote and the other skipped would show up as a difference.
#
#   Timing, 2-core Xeon @ 2.60 GHz, one thread over the whole tile grid (the engine
#   splits tiles across threads; see the caveat below), best of 5 blocks x 40 frames,
#   microseconds per viewport per frame:
#
#     base level     upstream -O3    this -O3    delta    upstream -O2   this -O2   delta
#     147x83          130 us          87 us      -33 %       138 us      104 us     -25 %
#     295x166         523 us         374 us      -28 %       561 us      423 us     -25 %
#     960x540        6000 us        4411 us      -27 %      6333 us     5288 us     -17 %
#     1920x1080     27285 us       21095 us      -23 %     28451 us    22830 us     -20 %
#
#   Read the size of the win against the other two patches, at the default budget of
#   147x83: about 43 us saved here, 2.4 us in the level-0 scatter and 3.3 us in the
#   mip build. The occlusion path's CPU work is dominated by preparing rays, not by
#   maintaining the pyramid - which is the opposite of what the two earlier patches
#   implied by being the only things measured.
#
#   Caveat that bounds the frame-time effect: this loop runs split across worker
#   threads (one tile range per thread, _camera_rays_threaded), so those microseconds
#   are aggregate CPU time, not wall clock on an idle machine - on a 24-thread box the
#   parallel part contributes roughly a twentieth of that to the frame, while the two
#   single-threaded terms (scatter, mip build) land in full. The scaling matters most
#   where threads are scarce and the per-ray math is what a mobile SoC is short of:
#   which is not measurable here, and is why this is reported as offline numbers.
#   The Embree traversal between generation and scatter is unmeasured and unmeasurable
#   in this sandbox (no engine build, no Embree), so no share-of-buffer_update claim
#   is made either.

# Reproducing: git diff of modules/raycast/raycast_occlusion_cull.cpp for this
# function, applied with `patch -p1` onto the tree as of patch 001; verified to
# reproduce the file byte-for-byte. `git apply` from a subdirectory resolves paths
# against the repository root and is not a valid way to run this check.
