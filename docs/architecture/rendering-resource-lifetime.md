# Rendering Resource Lifetime (R0 follow-up)

> **Status:** Source-verified map — **no runtime validation**
> **Verified against revision:** `eb7dd94` (2026-11-18)
> **Related:** [Rendering audit (R0)](rendering-audit.md), [Rendering roadmap](rendering-roadmap.md)

This document traces how a resource RID moves from the rendering server to the graphics API, and when it is actually destroyed. It is a prerequisite for R1, which must define new ownership rules without breaking the existing model.

## 1. Summary chain

```text
RenderingServer RID (RID_Owner)
        |
Renderer storage object (e.g. TextureStorage::Texture)
        |
RenderingDevice RID (RD owner table)
        |
Driver object ID (RDD: Vulkan/Metal/D3D12)
        |
Deferred disposal when the frame ring slot is reused
```

Two distinct lifetimes must not be confused:

- **CPU-side identity** (the RID and its owner-table entry): removed immediately on `free_rid()`.
- **GPU-side object**: destroyed later, on the render thread, when the frame slot that referenced it is recycled.

## 2. Layer 1 — RenderingServer RID

- RIDs are allocated and validated by `RID_Owner<T>` (`core/templates/rid_owner.h:517`), with `make_rid()` and `free()` (`:522`, `:548`).
- `RenderingServerDefault::free_rid()` runs on the server thread; when called from another thread it pushes `_free` to the command queue (`servers/rendering/rendering_server_default.h:1211-1219`). Ordering with prior commands is therefore preserved.
- In single-threaded mode, the call is executed immediately on the calling (main) thread.

## 3. Layer 2 — Renderer storage (RD backend)

Example: textures.

- `TextureStorage::texture_free(RID)` (`servers/rendering/renderer_rd/storage_rd/texture_storage.cpp:973`) refuses render targets, calls `Texture::cleanup()`, removes atlas/proxy links, and then frees the RID from `texture_owner`.
- `Texture::cleanup()` (`texture_storage.cpp:73-80`) frees the **sRGB variant first** (it is a dependency of the base texture), then the base texture, both through `RD::free_rid()`.

**Rule:** dependencies must be freed before their dependents. The renderer enforces this by construction (srgb before base, uniform sets before shaders), not by the backend.

## 4. Layer 3 — RenderingDevice

- `RenderingDevice::free_rid()` calls `_free_dependencies()` and then `_free_internal()` (`servers/rendering/rendering_device.cpp:7754-7757`). The function is guarded to run on the rendering thread (`ERR_RENDER_THREAD_GUARD`, `rendering_device.cpp:67-68`, `:7755`).
- `_free_internal()` does **not** destroy the GPU object immediately. It:
  1. removes the entry from the corresponding owner table;
  2. appends the object (or driver ID) to `frames[frame].<type>_to_dispose_of`;
  3. leaves destruction to `_free_pending_resources()` when that frame slot is reused.

  The comment in the source states it explicitly: *"Push everything so it's disposed of next time this frame index is processed (means, it's safe to do it)."* (`rendering_device.cpp:7770`).
- The frame ring is advanced by `swap_buffers()`: `_end_frame()` → `_execute_frame()` → `frame = (frame + 1) % frames.size()` → `_begin_frame(true)` (`rendering_device.cpp:8141-8152`).
- `_begin_frame()` first calls `_stall_for_frame(frame)`, which waits on that slot's fence (`rendering_device.cpp:8493-8494`), then calls `_free_pending_resources(frame)` (`rendering_device.cpp:8343-8344`).

Therefore a resource is destroyed only after the GPU work that used it has completed, on the render thread.

## 5. Disposal order inside `_free_pending_resources()`

Verified order (`servers/rendering/rendering_device.cpp:8176-8320`):

1. render pipelines;
2. compute pipelines;
3. ray-tracing pipelines;
4. acceleration structures;
5. uniform sets;
6. shaders;
7. samplers;
8. framebuffers;
9. textures;
10. buffers.

The comment in the source describes this as "dependency usage order, so nothing weird happens".

## 6. Number of frames in flight

- `frame_count` defaults to 1 and becomes `MAX(2, rendering/rendering_device/vsync/frame_queue_size)` when a rendering device is created (`rendering_device.cpp:8641-8643`), then `frames.resize(frame_count)` (`:8718`).
- Consequence: a freed object survives **at least one full frame slot rotation** after the free request; the exact delay depends on `frame_queue_size`.
- On device teardown, `_free_pending_resources()` is called for every slot (`rendering_device.cpp:9216`).

## 7. Rules implied for R1

1. Free resources through `RenderingDevice::free_rid()`; never manipulate driver IDs directly.
2. Never reuse a RID after `free_rid()` — the CPU-side entry is gone immediately.
3. Preserve dependency order when freeing (dependents first).
4. Treat destruction as asynchronous: do not assume memory is reclaimed at the end of `free_rid()`.
5. Record and destroy resources on the rendering thread (RD guard macro enforces this).
6. Any new resource type added to the renderer must declare its owner table, its dependency edges, and its disposal slot.

## 8. Open items

- Recursive details of `_free_dependencies()` for framebuffers and uniform sets (it frees dependents automatically; the full graph was not enumerated).
- Interaction with the transfer worker threads and staging buffers.
- Multi-window/swapchain lifetime (not audited).
- Runtime validation: allocation/free stress under frame churn — **not performed**.
