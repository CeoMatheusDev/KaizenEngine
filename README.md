  <div align="center">
  <img src="LogoBanner_1.png" alt="Kaizen Game Engine" width="100%">
</div>

<div align="center">

[![Version](https://img.shields.io/badge/version-4.8.0--beta-blue.svg)]()
[![License](https://img.shields.io/badge/license-AGPL--3.0%20%2B%20Game%20Exception-green.svg)](LICENSE-ADDITIONAL-TERMS.md)
[![Platforms](https://img.shields.io/badge/platforms-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)]()
[![Status](https://img.shields.io/badge/status-under%20development-orange.svg)]()

</div>

---

## 🎮 Kaizen Game Engine

**Kaizen** (改善) is an open-source game engine based on the Godot Engine, built with a focus on continuous evolution, performance, and freedom of development.

The engine provides an integrated environment for creating 2D and 3D games, combining editing tools, rendering, physics, audio, and scripting systems.

The project aims to preserve the flexibility and open-source philosophy of its upstream base while developing its own modifications and features.

> *Kaizen: small, daily improvements lead to great results.*

**Origin:** KaizenEngine is derived from [Godot Engine v4.8.0-beta.1](https://godotengine.org). The original Godot code remains under the MIT license, while the modifications and original implementations are licensed under AGPL-3.0, as applicable.

---

## ✨ Features

- 🎨 **Integrated editor:** visual creation and management of projects and scenes (inherited Godot infrastructure, with original modifications under development).
- ⚡ **Rendering:** graphics infrastructure based on Godot's rendering architecture.
- 🧩 **Scene and node system:** organization of game objects and behaviors (Godot-inherited scene architecture).
- 🎯 **Physics:** 2D and 3D simulation and collision (Godot-inherited physics; the tree ships the renamed `kaizen_physics_2d`/`kaizen_physics_3d` modules as the default engine and the optional NVIDIA PhysX backend).
- 🔊 **Audio:** playback and management of audio (inherited from Godot).
- 🛠️ **Extensibility:** ability to work with native code and scripting systems.
- 📦 **Multiplatform export:** export infrastructure inherited from Godot's architecture, subject to the platforms actually enabled in Kaizen.
- 🔌 **Modularity:** ability to extend the engine's systems.

---

## 📥 Getting Kaizen

The way to obtain the Kaizen source code today is through the official repository.

```bash
git clone https://github.com/CeoMatheusDev/KaizenEngine.git

cd KaizenEngine
```

> Note: the repository does not use Git submodules.

### Binaries

When available, Kaizen binaries will allow you to use the editor without compiling the entire source code manually.

Distribution packages will be platform- and architecture-specific for each supported platform.

**Downloads:** there is no official binary distribution address as of this README.

---

## 🔨 Building from source

The upstream Godot build uses **SCons**, a Python-based build system.

Compilation can produce the full editor, development builds, and templates used to run exported games.

### Prerequisites

Requirements vary by platform. Minimum versions required by `SConstruct`:

| Tool | Minimum version | Purpose |
|---|---|---|
| Python | 3.10+ | Build system (`EnsurePythonVersion(3, 10)`) |
| SCons | 4.4+ | Configuration and build (`EnsureSConsVersion(4, 4)`) |
| SCons | 4.8+ | Required for Visual Studio 2022 |
| SCons | 4.10.1+ | Required for Visual Studio 2026 |
| GCC | 11+ | C/C++ compiler (Linux) |
| Clang | 9+ | C/C++ compiler (Linux/macOS) |
| Apple Clang | 16+ (Xcode 16) | C/C++ compiler (macOS) |
| Visual Studio | 2019 16.11+ | C/C++ compiler (Windows; VS2022/VS2026 also supported) |
| Git | any recent | Version control |
| pkg-config | — | Dependency detection on Linux |

Platform-specific development libraries are also required.

The [Godot Engine compilation guide](https://docs.godotengine.org/en/latest/engine_details/development/compiling/index.html) is the reference for the inherited architecture, but the minimum requirements above are the ones verified in Kaizen's own `SConstruct`.

### Build targets

The easiest entry point is `./build.sh`, which hides the platform plumbing
(the engine targets the `linuxbsd` family — one shared POSIX/X11/Wayland
backend for Linux and the BSDs):

```bash
./build.sh                 # Editor (produces bin/kaizen.<platform>.editor.<arch>)
./build.sh release         # Export template (release)
./build.sh physx           # Just the NVIDIA PhysX SDK group
./build.sh test            # Test suite (license banners + engine checks)
./build.sh smoke           # PhysX runtime smoke test
```

Under the hood the `SConstruct` defines three targets:

```bash
# Editor (produces bin/kaizen.<platform>.editor.<arch>)
scons platform=linuxbsd target=editor

# Export template (release)
scons platform=linuxbsd target=template_release

# Export template (debug)
scons platform=linuxbsd target=template_debug
```

`target=` can be omitted (`editor` is the default), but passing it explicitly avoids ambiguity. Replace `linuxbsd` with `windows` or `macos` as appropriate for your platform.

### Linux

Install the required build tools:

**Initial** dependency list — not universal or exhaustive. Compilation may require additional dependencies depending on the graphics drivers, features, and modules enabled. Test on a clean Fedora installation before treating this as a proven procedure:

```bash
# Fedora
sudo dnf install gcc-c++ clang python3 python3-pip scons \
    pkgconf-pkg-config \
    libX11-devel libXcursor-devel libXinerama-devel \
    libXi-devel libXrandr-devel \
    wayland-devel wayland-protocols-devel \
    mesa-libGL-devel \
    alsa-lib-devel pulseaudio-libs-devel
```

At the source-tree root:

```bash
# Build the editor
scons platform=linuxbsd target=editor

# Run the compiled editor
./bin/kaizen.linuxbsd.editor.x86_64
```

The exact executable name depends on the platform and architecture of the build.

### Windows

Install:

- Python 3.10 or newer.
- SCons 4.4 or newer (4.8+ if using Visual Studio 2022; 4.10.1+ if using Visual Studio 2026).
- Visual Studio 2019 16.11+ (or newer) with the C++ build tools, or MinGW-w64.

Install SCons:

```powershell
python -m pip install scons
```

At the source-tree root:

```powershell
# Build the editor
scons platform=windows target=editor
```

The executable will be generated in the `bin/` directory.

### macOS

Install:

- Python 3.10 or newer.
- SCons 4.4 or newer.
- Xcode 16 or newer (Apple Clang 16+) or the corresponding Command Line Tools.
- Graphics driver: Kaizen supports **Metal** (selected by default in macOS builds) and Vulkan via MoltenVK, depending on the driver enabled in the graphics configuration — a Vulkan SDK is not required to use Metal. See `platform/macos/detect.py` for the options (`metal`, etc.).

Install SCons:

```bash
python3 -m pip install scons
```

Build for Intel:

```bash
scons platform=macos target=editor arch=x86_64
```

Or for Apple Silicon:

```bash
scons platform=macos target=editor arch=arm64
```

The build output will be available in the `bin/` directory.

### Development builds

For working on the source code, you can use build options aimed at development:

```bash
# Editor with additional debugging features (defines DEV_ENABLED)
scons platform=linuxbsd target=editor dev_build=yes
```

For distribution builds:

```bash
scons platform=linuxbsd target=editor production=yes
```

`dev_build` and `production` are options defined in the `SConstruct` itself (`dev_build` enables development-only debugging code; `production` adjusts the defaults for production builds).

**C#/Mono module:** the Mono module exists in the tree, but it is **disabled by default**. To enable it, build with `module_mono_enabled=yes`. Without that option, C# support is not part of the build.

**Lua:** there is no Lua module in the project tree. There is no Lua support to announce.

---

## 🚀 Using the editor

After downloading or building a Kaizen executable, you can launch the development environment.

### Project manager

The project manager lets you create, import, organize, and open existing projects.

From it, you can start a new project or select an existing one.

### Opening a project

Projects can be opened through the editor by selecting the corresponding directory.

In Kaizen, projects are identified by the **`project.kaizen`** file — not `project.godot`. `main.cpp` recognizes that file, and the project manager also uses it to identify and open projects.

The editor can also be started directly from the terminal:

```bash
# Open the editor pointing at the project
./bin/kaizen.linuxbsd.editor.x86_64 --editor --path /path/to/project

# Run the project directly
./bin/kaizen.linuxbsd.editor.x86_64 --path /path/to/project
```

The executable name varies with the platform and architecture of the build.

### Running a project

After configuring the project, use the editor's run option to launch the game and test your changes.

You can also run the project from the command line using the editor binary, as in the example above.

---

## 📦 Exporting games

Kaizen's codebase already provides export infrastructure (with support for **Linux, Windows, and macOS** — `platform/macos/export/export_plugin.cpp` implements the macOS exporter) and command-line parameters for export. The practical availability of each platform depends on the compiled templates and on each export preset's configuration. `main.cpp` confirms `--export-release`, `--export-debug`, and `--headless`.

Export templates are engine-specific builds, compiled from the source tree itself and matching the version used to develop the project.

### Export templates

Templates are engine-specific builds used to run exported projects without including the full editor. Build them with:

```bash
scons platform=linuxbsd target=template_release
scons platform=linuxbsd target=template_debug
```

To prepare an export:

1. Build the templates matching your Kaizen version (release and/or debug).
2. Open the project in the editor.
3. Open the export menu.
4. Create a preset for the desired platform.
5. Configure the export options.
6. Generate the final game package.

### Command-line export

Kaizen supports automated export:

```bash
./bin/kaizen.linuxbsd.editor.x86_64 \
    --headless \
    --path /path/to/project \
    --export-release "My Linux preset" \
    build/game
```

Notes:

- The preset name must match a preset defined in `export_presets.cfg` in the project — `"My Linux preset"` is only an example.
- `--export-debug <preset> <path>` is also available for debug exports.
- Upstream base documentation: [exporting projects (Godot)](https://docs.godotengine.org/en/latest/tutorials/export/exporting_projects.html).

---

## 📚 Documentation and resources

Kaizen-specific engineering documentation is maintained in the repository:

- [Documentation index](docs/README.md) — entry point and documentation conventions.
- [Architecture overview](docs/architecture/overview.md) — current source baseline, inherited systems, and engineering constraints.
- [Rendering architecture and technical roadmap](docs/architecture/rendering-roadmap.md) — the proposed custom rendering architecture and staged roadmap for desktop, mobile, and Web.

The [Godot documentation](https://docs.godotengine.org/en/latest/) remains the reference for inherited APIs and workflows. Unless a Kaizen document explicitly records a divergence, upstream behavior should be verified against the Kaizen source revision before being treated as guaranteed.

---

## 🧪 Demos and examples

> **Planned section.** There are no official demo projects published for KaizenEngine yet. When they exist, they will be linked here.

In the meantime, the best way to explore the engine is to build the editor (see [Building from source](#building-from-source)) and open a new project — the Godot-inherited scene and node architecture is fully usable.

---

## 🗺️ Roadmap

> The **4.8.x** numbering identifies the version of the technology base inherited from Godot. The **0.x** and **1.x** versions represent Kaizen's own development milestones.

| Version | Feature | Status |
|---|---|---|
| **4.8.0-beta** | Engine base (2D/3D editor, renderer) inherited from Godot, Kaizen rebranding | 🚧 In development (beta) |
| **0.2** | 2D/3D physics and audio — Godot-inherited infrastructure; original integration and improvements under development | 🚧 In development |
| **0.3** | Material and shader editor — the base already provides Godot infrastructure; the pending work is original integration/evolution | 📅 Planned |
| **0.4** | Web build (WASM) | 📅 Planned |
| **1.0** | Stable release and complete documentation | 🎯 Goal |

> Note: Kaizen does not have its own ECS (Entity-Component-System) implementation yet. The current architecture is the Godot-inherited scene and node system.

---

## 🤝 Community and contributing

Kaizen is an open-source project. Contributions are welcome — fixes, improvements, documentation, or new features.

### How to contribute

1. Fork the project.
2. Create a branch:

   ```bash
   git checkout -b feature/my-feature
   ```

3. Implement your changes.
4. Commit:

   ```bash
   git commit -m "feat: add new feature"
   ```

5. Push your changes:

   ```bash
   git push origin feature/my-feature
   ```

6. Open a Pull Request.

Before contributing, see [CONTRIBUTING.md](CONTRIBUTING.md).

### Channels

Kaizen does not have a community platform of its own yet. For now, the available channels are:

- 💡 **[Issues](https://github.com/CeoMatheusDev/KaizenEngine/issues):** report problems and suggest features.
- 🔧 **[Pull Requests](https://github.com/CeoMatheusDev/KaizenEngine/pulls):** contribute code directly.

---

## 📄 License

Kaizen uses a hybrid licensing model due to its origin as a derivative work of the Godot Engine.

> **Important:** the table below is only a summary. The complete, binding terms are in the license documents. The legal interpretation of the compatibility between the files, and the scope of the game exception, requires specific legal review — this section is not legal advice or a guarantee.

| Layer | License | File |
|---|---|---|
| Original Godot Engine code | MIT | `LICENSE-MIT` |
| KaizenEngine modifications and new code | AGPL-3.0 | `LICENSE.txt` |
| Game distribution exception and additional terms | Additional permissions (AGPL Section 7) | `LICENSE-ADDITIONAL-TERMS.md` |

### Practical summary

- **Engine:** new code and modifications are subject to AGPL-3.0.
- **Games:** the additional exception allows commercial distribution of games under other licenses, subject to its conditions.
- **Godot code:** remains under the original MIT license.

To learn the complete terms, see:

- `LICENSE.txt`
- `LICENSE-MIT`
- `LICENSE-ADDITIONAL-TERMS.md`

---

<div align="center">

**Kaizen — evolving one commit at a time. 🌱**

Made with ❤️ by developers passionate about games.

</div>
