# Web Export Runtime (R0 follow-up)

> **Status:** Source-verified — **no runtime validation in a browser**
> **Verified against revision:** `d2d9ca5` (2026-11-18)
> **Related:** [Rendering audit (R0)](rendering-audit.md), [Rendering benchmark methodology](rendering-benchmarks.md), [Rendering roadmap](rendering-roadmap.md)

This document records what the source establishes about the Web export: which renderer can run, how the WebGL context is created, how threading is configured, and which features degrade or disappear. Everything here is read from code; nothing was exercised in a browser.

## 1. Renderer selection is pinned

- The project setting `rendering/renderer/rendering_method.web` only accepts `gl_compatibility`, with the source comment calling it *"a bit of a hack until we have WebGPU support"* (`main/main.cpp:2662`).
- The Compatibility driver override for Web only accepts `opengl3` (`main.cpp:2407`).
- Consequently, a Web build **cannot** use Forward+, Mobile, or `RenderingDevice` at all. Every RD-only capability in the other documents — compute pipelines, storage buffers, deferred disposal via `RenderingDevice::free_rid()`, device PSO cache — is absent on Web by construction, not by configuration.

## 2. WebGL 2 is a hard requirement

- The display server checks for WebGL 2 and creates the context with `majorVersion = 2`, `antialias = false`, `explicitSwapControl = true` (`platform/web/display_server_web.cpp:1152-1161`).
- If the context cannot be created or made current, it shows an alert ("Your browser seems not to support WebGL 2...") and falls back to `RasterizerDummy` (`display_server_web.cpp:1163-1174`). There is **no WebGL 1 fallback**.
- `OVR_multiview2` is enabled opportunistically when present; failure only prints a verbose message and rendering continues (`display_server_web.cpp:1163-1167`).
- The Emscripten flag `-sMAX_WEBGL_VERSION=2` only makes WebGL 2 available; it does not disable WebGL 1 (`platform/web/detect.py:246-247`). The engine's own requirement is enforced by the display server, not by the linker flag.
- The HTML shell also performs a client-side capability check that reports missing WebGL 2, Fetch, and (when applicable) thread requirements before start (`platform/web/js/engine/features.js:77-105`).

## 3. Threading: off by default

- The export preset option `variant/thread_support` defaults to **`false`** and is documented as *"Thread support (i.e. run with or without COEP/COOP headers)"* (`platform/web/export/export_plugin.cpp:385`).
- Templates are built in two variants; the non-threaded one is named with a `_nothreads` suffix (`platform/web/export/export_plugin.h:73-80`, `export_plugin.cpp:367-371`), and the exporter picks the template that matches the preset (`export_plugin.cpp:445-449`, `:526-527`).
- When threads are enabled at build time: `-pthread`, `-sDEFAULT_PTHREAD_STACK_SIZE`, `-sPTHREAD_POOL_SIZE` (default 8, overridable via `Module['emscriptenPoolSize']`), and `-sMAXIMUM_MEMORY=2048MB` are added (`platform/web/detect.py:259-266`).
- Runtime requirements for a threaded build are enforced in the shell: secure context, cross-origin isolation, and `SharedArrayBuffer`; if any is missing, startup lists them as missing features (`platform/web/js/engine/features.js:92-101`). Serving a threaded build without `COOP`/`COEP` headers therefore fails before the engine starts.
- The engine-side thread pool size comes from the export option `threads/kaizen_pool_size` (default 4) while Emscripten's pool comes from `threads/emscripten_pool_size` (default 8) (`export_plugin.cpp:405-406`, `:166-167`); without `PThread` the engine reports a pool size of 1 (`platform/web/js/libs/library_kaizen_os.js:373-381`).
- `OS_Web::get_processor_count()` does not return the browser's reported core count directly: it clamps to a maximum of 2 (`library_kaizen_os.js:366-371`, `platform/web/os_web.cpp:160-162`), with a source comment noting that Kaizen core otherwise spawns too many threads.

### Rendering thread

- The render thread is only used when the project setting `rendering/driver/threads/thread_model` is explicitly `Separate`; the default is `Safe` (`main/main.cpp:2789`).
- `separate_thread_render` is forced to `0` when the build has no thread support (`main.cpp:2795-2797`), so a default Web export renders entirely on the main thread.
- Even when enabled, the "render thread" is not a dedicated `Thread`: it is a WorkerThreadPool task named `Rendering Server pump task` (`servers/rendering/rendering_server_default.cpp:287-290`).

## 4. No shader or pipeline cache on Web

- The GLES3 shader binary cache is compiled out on Web: `_load_from_cache()` returns `false` immediately and `_save_to_cache()` is a no-op (`drivers/gles3/shader_gles3.cpp:553-566`, `:646-649`). The file header and cache version constants are also excluded (`:555-558`).
- There is no `RenderingDevice` PSO cache, since Web does not use `RenderingDevice`.
- Consequence: every page load recompiles GLSL programs. First-frame and first-material costs are paid on every visit, and the warm-start improvements available on Vulkan/D3D12 do not exist here. This is the Web counterpart of the Metal finding in [Pipeline cache and shader variants](rendering-pipeline-cache.md).

## 5. Feature limits observed in the Compatibility renderer

| Capability | Web behavior | Source |
|---|---|---|
| Float render targets | Requires `EXT_color_buffer_float`; not assumed | `drivers/gles3/storage/config.cpp:102` |
| Linear filtering of float textures | Requires `OES_texture_float_linear` | `config.cpp:103` |
| ETC2 compressed textures | Requires `WEBGL_compressed_texture_etc`; unlike other GLES targets, not assumed | `config.cpp:104-108` |
| MSAA | Only if `GL_MAX_SAMPLES > 0` | `config.cpp:143-148` |
| Multiview / WebXR | Requires `OCULUS_multiview`; MSAA multiview only with the same extension | `config.cpp:151-158` |
| Extension discovery | Uses `emscripten_webgl_get_supported_extensions()`, because `glGetStringi` returns a fixed list since Emscripten 3.1.51 | `config.cpp:54-72` |
| `half` → `float` polyfill | Disabled on Web | `config.cpp:258-260` |
| Shader binary cache | Disabled | `shader_gles3.cpp:553-566`, `:646-649` |
| WebGL context loss | Listener shows an alert and calls `preventDefault()`; no restore path — the page must be reloaded | `platform/web/js/libs/library_kaizen_display.js:746-750` |

No claim is made here about which of these extensions any real browser/GPU combination provides; that must be measured, as specified in [Rendering benchmark methodology](rendering-benchmarks.md).

## 6. Build flags relevant to rendering

From `platform/web/detect.py`:

- `GLES3_ENABLED` is defined when `opengl3` is enabled (`:244-245`).
- `-sOFFSCREEN_FRAMEBUFFER=1` gives the engine control over buffer swapping (`:249`).
- `-sGL_ENABLE_GET_PROC_ADDRESS=0` avoids per-symbol `glGetProcAddress` overhead (`:252`).
- `-sENVIRONMENT=web,worker` restricts generated support code to browser and worker environments (`:298`).
- `wasm_simd` defaults to enabled (`:64`) and adds `-msimd128` (`:294-295`).
- `dlink_enabled` (GDExtension) is off by default (`:56`) and switches to `-sSIDE_MODULE=2` (`:276-286`).
- `proxy_to_pthread` is off by default; enabling it requires threads and moves the main application to a worker (`:289-292`).

## 7. Implications for the roadmap

1. **Web cannot share the R1 `RenderingDevice`-based architecture.** If the long-term renderer is RD-only, Web needs either a retained Compatibility backend or a WebGPU backend; the source itself names WebGPU as the intended future path (`main.cpp:2662`).
2. **Shader warm-up is a Web-specific cost.** Any startup-time budget defined for R1 must be evaluated separately for Web.
3. **Threading is opt-in and infrastructural.** A threaded build changes deployment requirements (HTTPS plus COOP/COEP), so benchmarks must record which variant was used.
4. **The Web path already has a documented single-threaded fallback** in the engine, so rendering code must not assume a worker-thread renderer exists.

## 8. Open items

- Actual WebGL 2 extension availability, MSAA support, and performance per browser/GPU — **not measured**.
- Whether `OFFSCREEN_FRAMEBUFFER` swap control behaves identically across browsers — **not verified at runtime**.
- WebXR behavior (multiview, MSAA multiview) — **not verified**.
- WebGPU support — **does not exist in this source tree**; only referenced in a comment.
- Memory ceiling behavior in practice (`-sMAXIMUM_MEMORY=2048MB` only applies to threaded builds) — **not measured**.
- Audio, input, and networking Web behavior are outside this document's scope.
