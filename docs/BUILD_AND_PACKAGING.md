# Kaizen Engine — Build and Distribution

Technical document for the build and packaging infrastructure of the Kaizen Engine
(a fork of the Godot Engine 4.8, renamed and with its own modifications).

- Engine version: `4.8.beta.mono.custom_build` (`version.py` = `4.8.0`,
  status `beta`)
- Validated platform: **Linux x86_64** (Fedora 44, GCC 16.2.1)
- .NET variant: **validated**, with private embedded SDK/runtime 10
- Windows: infrastructure **prepared, not validated** in this environment and still
  in the old layout (see §12)

---

## 1. Principles

1. **Build ≠ packaging.** SCons remains the only entity responsible for
   compiling. The scripts drive SCons and organize artifacts; no internal
   build-system option has been changed.
2. **Nothing is deleted implicitly.** `build.sh` never runs `scons -c` nor
   removes `bin/`. `package.sh` only replaces the output with an explicit
   `--force`, via staging + atomic swap.
3. **Version/channel are explicit metadata.** They are never deduced from
   directory names. The package version comes from `version.py` or from `--version`;
   the channel, from `status` or from `--channel`.
4. **Reproducible distribution.** The `.tar.xz` is deterministic; with the same
   toolchain and `SOURCE_DATE_EPOCH`, two runs generate identical bytes.
5. **No intermediates in the distribution.** `.o`, `.a`, `.lib`, `obj/`, caches,
   `__pycache__`, `.sconsign*`, `.scons_env.json`, `doc/translations` and
   `tests/` are never copied.
6. **No empty content.** No directory is created without real content.

---

## 2. Directory architecture

### 2.1 Repository (source)

```text
KaizenEngine/
├── SConstruct, methods.py, platform_methods.py   # build system (SCons)
├── core/ editor/ main/ scene/ servers/ platform/ modules/ drivers/
├── modules/mono/                                 # .NET integration
│   ├── build_scripts/build_assemblies.py
│   ├── glue/KaizenSharp/                         # C# API sources (glue)
│   ├── editor/Kaizen.NET.Sdk/                    # distributed MSBuild SDK
│   └── editor/KaizenTools/                       # C# editor tooling
├── misc/dist/linux/                              # .desktop, AppData, MIME, man
├── misc/logo/                                    # icons
├── packaging/                                    # this infrastructure
│
│ ── build outputs (generated, not part of the source) ──
├── bin/                                          # build artifacts
└── dist/                                         # .reference/, Kaizen/, .tar.xz
```

> The repository is in a **source-only** state: `bin/` and `dist/` are
> outputs of `build.sh`/`package.sh` and were removed on 2026-11-17;
> they reappear only after a full rebuild.

### 2.2 Distribution

| Path | Content | Consumer |
|---|---|---|
| `Engine/Binaries/Linux/` | canonical executable, `kaizen` wrapper, `_sc_` marker, `KaizenSharp` symlink | user/runtime |
| `ExportTemplates/Linux/{Release,Debug}/` | `linux_release.x86_64`, `linux_debug.x86_64` | export plugin |
| `Templates/<FULL_CONFIG>/` | ready-to-install view: **relative symlinks only** | installer |
| `Engine/Managed/KaizenSharp/` | `Api/{Debug,Release}`, `Tools` (includes `nupkgs/`) | .NET editor |
| `Engine/Managed/dotnet/` | private SDK + runtime (mono) | wrapper/editor |
| `Engine/Source/` | source (includes `packaging/`) — **only** with `--with-source` | development |
| `Build/Config/` | `build-info.json`, `package-info.env`, `dotnet-info.env` | validation/installation |
| `Build/Toolchains/` | toolchain notes | maintainers |
| `Config/linux/` | `.desktop`, AppStream, MIME, man page, icons | desktop integration |
| `LICENSES/` | Kaizen + licenses of the embedded .NET | distribution |
| `Samples/` | `hello-kaizen`, `hello-kaizen-csharp` | user/CI |
| `Setup/` | `install.sh`, `uninstall.sh`, `select.sh`, `check-deps.sh` | installation |
| `README.txt` | version/channel summary and instructions | user |
| `MANIFEST.sha256` | hash of every regular file | integrity |

The portable mode uses `Engine/Binaries/Linux/editor_data/export_templates/<FULL_CONFIG>/`
with symlinks to `ExportTemplates/`. The **installed** distribution drops
`_sc_`, `editor_data/`, `ExportTemplates/` and `Templates/`, because the templates
go to the XDG directory and the editor stops being self-contained.

### 2.3 Installation (per user)

```text
~/.local/opt/kaizen/
├── <channel>/<version>/      installed tree + .kaizen-receipt.env
├── <channel>/current -> <version>
├── .dotnet/<sdk>/            shared private .NET
├── default-channel           default channel
├── select.sh
└── check-deps.sh

~/.local/bin/kaizen           wrapper for the default channel
~/.local/bin/kaizen-<channel>  per-channel launcher
~/.local/bin/kaizen-select    selection
~/.local/share/kaizen/export_templates/<FULL_CONFIG>/
~/.local/share/{applications,metainfo,mime,icons,man}/
~/.local/share/kaizen/{app_userdata,...}   # user data (never removed)
```

---

## 3. Versioning and channels

### 3.1 Model

`version.py` provides `major.minor.patch` and `status`:

| Field | Example |
|---|---|
| `major.minor.patch` | `4.8.0` |
| `status` | `beta` |
| `--channel-revision N` | `1` |
| Package version | `4.8.0-beta1` |
| Name | `kaizen-x86_64-linux-4.8.0-beta1` |

Accepted channels: `stable`, `beta`, `rc`, `dev`.

- `stable`: no suffix (`4.8.0`).
- `beta`/`rc`/`dev`: `X.Y.Z-<channel><N>` (N ≥ 1, default 1).
- `--version`/`--channel` override the `version.py` metadata.
- The packager cross-checks the channel/version against what the binary reports. If
  they diverge, it fails; `--allow-channel-mismatch` exists only for test fixtures.

The engine version (internal) does not change with the package channel:
`4.8.beta.mono.custom_build`, templates at `4.8.beta.mono`.

### 3.2 Coexistence

Versions of different channels coexist in `<opt-root>/<channel>/<version>/`.
Example:

```text
~/.local/opt/kaizen/stable/4.8.0/
~/.local/opt/kaizen/beta/4.8.0-beta1/
~/.local/opt/kaizen/rc/4.8.0-rc1/
~/.local/opt/kaizen/dev/4.8.0-dev1/
```

`kaizen` points to the default channel; `kaizen-beta` etc. are fixed launchers
per channel. `kaizen-select` changes the default and/or a channel's `current`.

---

## 4. Build system (SCons)

### 4.1 Relevant options (verified in `SConstruct`)

| Option | Default | Use |
|---|---|---|
| `platform=` | detected | `linuxbsd`, `windows` |
| `target=` | `editor` | `editor`, `template_debug`, `template_release` |
| `arch=` | detected | `x86_64` |
| `module_mono_enabled=` | `no` | enables .NET |
| `production=` | `no` | production build (LTO/strip) |
| `dev_build=` | `no` | development build |
| `use_llvm=` | `no` | Clang instead of GCC |
| `use_mingw=` | `no` | MinGW-w64 on Windows |
| `accesskit=` | `yes` | screen reader (requires extra deps) |
| `redirect_build_objects=` | `yes` | objects in `bin/obj/` |

Final executable name:

```text
<PROGPREFIX>kaizen.<platform>.<target>.<arch><module_version><PROGSUFFIX>
```

Examples:

```text
kaizen.linuxbsd.editor.x86_64
kaizen.linuxbsd.template_release.x86_64.mono
kaizen.windows.editor.x86_64.mono.exe
```

### 4.2 Reproducible commands

```bash
scons platform=linuxbsd target=editor -j24
scons platform=linuxbsd target=editor module_mono_enabled=yes -j24
scons platform=linuxbsd target=template_release -j24
scons platform=linuxbsd target=template_release module_mono_enabled=yes -j24
scons platform=linuxbsd target=template_debug   module_mono_enabled=yes -j24
```

### 4.3 Version and template directory

`core/version.h` defines:

- `KAIZEN_VERSION_FULL_CONFIG` = `4.8.beta` or `4.8.beta.mono`
- `KAIZEN_VERSION_FULL_BUILD` = `4.8.beta.mono.custom_build`

The editor looks for templates in
`<data>/export_templates/<KAIZEN_VERSION_FULL_CONFIG>/`. `package.sh` derives
this name by removing `.custom_build`/`.official` from `kaizen --version`.

---

## 5. Build driver — `packaging/linux/build.sh`

```bash
./packaging/linux/build.sh [--target editor|template_debug|template_release|all]
                           [--mono|--no-mono] [--dev|--production]
                           [--jobs N] [--arch x86_64] [--dry-run] [-v]
```

- `--target` accepts a comma-separated list or repetition; default `all`
  (editor → template_release → template_debug).
- Validates that the expected binary appeared in `bin/`.
- `--mono` adds `module_mono_enabled=yes` and, after the editor:
  1. generates the C# glue with `--generate-mono-glue`;
  2. compiles the assemblies with `build_assemblies.py`;
  3. verifies `bin/KaizenSharp/Api/Release/KaizenSharp.dll` and
     `bin/KaizenSharp/Tools/KaizenTools.dll`.
- `--dry-run` prints the commands without executing them.

---

## 6. .NET integration

### 6.1 Generated components

| Component | Origin | Destination |
|---|---|---|
| C# glue | `--generate-mono-glue` | `modules/mono/glue/KaizenSharp/**/Generated` |
| Managed API | `build_assemblies.py` | `bin/KaizenSharp/Api/{Debug,Release}` |
| Editor tools | `KaizenTools.sln` | `bin/KaizenSharp/Tools` |
| MSBuild SDK | `Kaizen.NET.Sdk.sln` | `bin/KaizenSharp/Tools/nupkgs` |
| Source generators | `Kaizen.SourceGenerators*.sln` | inside `Tools` |

### 6.2 Runtime location

`modules/mono/kaizensharp_dirs.cpp` resolves `KaizenSharp` **next to the
executable** (`exe_dir/KaizenSharp/{Tools,Api/<Config>}`). The distribution
keeps:

```text
Engine/Managed/KaizenSharp/         # real content
Engine/Binaries/Linux/KaizenSharp -> ../../Managed/KaizenSharp
```

Do not move the content without keeping that link: the editor does not search in
`share/kaizen` or in XDG paths.

### 6.3 Private SDK + runtime

The `--mono` variant embeds a subset of .NET 10 in
`Engine/Managed/dotnet/`:

- `dotnet`, `host`, `metadata`
- `sdk/<version>` (full SDK, required to compile C#)
- `shared/Microsoft.NETCore.App/<version>`
- `packs/Microsoft.NETCore.App.Ref`
- `LICENSE.txt`, `ThirdPartyNotices.txt`

Not embedded: AspNetCore, RID packs (Fedora), project templates from the
SDK, glibc, Vulkan/graphics drivers or fundamental system libraries.

The wrapper `Engine/Binaries/Linux/kaizen` resolves its own path, sets
`DOTNET_ROOT=<tree>/Engine/Managed/dotnet`, prepends that directory to `PATH`
and executes the canonical binary. `KAIZEN_DOTNET_ROOT` may override it.

On install, the content is moved to `<opt-root>/.dotnet/<sdk>/` and each
version receives the symlink:

```text
<version>/Engine/Managed/dotnet -> ../../../../.dotnet/<sdk>
```

This avoids duplicating ~300 MB per version. Removal only deletes `.dotnet/<sdk>`
when no remaining receipt uses the same SDK.

Verification:

```bash
<opt-root>/.dotnet/10.0.113/dotnet --info
<opt-root>/.dotnet/10.0.113/dotnet --list-sdks
```

### 6.4 NuGet and C# projects

`build_assemblies.py` generates `KaizenSharp`, `KaizenSharpEditor`,
`Kaizen.SourceGenerators` and `Kaizen.NET.Sdk` at the assemblies' version
(`4.8.0-beta`). `Kaizen.NET.Sdk` does not exist on NuGet.org; the sample uses a
relative local source inside the distribution:

```xml
<add key="kaizen-local" value="../../Engine/Managed/KaizenSharp/Tools/nupkgs" />
```

Projects created by the editor receive `Sdk="Kaizen.NET.Sdk/<version>"` and
`TargetFramework=net10.0`, with output in `.kaizen/mono/temp/`.

The private SDK compiles and exports C# without a system SDK. Restore may still
need the network (NuGet.org) for third-party dependencies; the local feed
covers the Kaizen packages.

---

## 7. Packaging — `packaging/linux/package.sh`

```bash
./packaging/linux/package.sh [--mono|--no-mono]
    [--version X.Y.Z] [--channel stable|beta|rc|dev] [--channel-revision N]
    [--dotnet-bundle auto|yes|no] [--dotnet-root DIR] [--dotnet-version VER]
    [-o|--output DIR] [--tarball-out DIR] [--tarball-name NAME]
    [--no-dir] [--no-tarball] [--with-source] [--reproducible]
    [--xz-preset 0-9] [-j|--jobs N] [--force] [--dry-run]
```

Without `--mono/--no-mono`, it detects the variant by the presence of
`bin/kaizen.linuxbsd.editor.x86_64.mono`.

Stages:

1. Resolves binaries and reads the executable's real version (`--version`).
2. Resolves explicit version/channel and validates them against the binary.
3. Creates staging; nothing is written to the destination until the end.
4. Copies the canonical executable (`0755`), generates the `kaizen` wrapper and the
   `_sc_` marker.
5. Installs templates in `ExportTemplates/Linux/{Release,Debug}` and creates
   symlinks in `editor_data/export_templates/<FULL_CONFIG>/` and in
   `Templates/<FULL_CONFIG>/`.
6. Copies `bin/KaizenSharp` to `Engine/Managed/KaizenSharp` and creates the
   symlink next to the executable (mono).
7. Optionally embeds the private .NET and validates it with `dotnet --info`.
8. Copies `.desktop`, AppStream, MIME, man page, icons and licenses.
9. Copies the applicable samples, excluding `.kaizen/`, `out/`, `export/`,
   `bin/`, `obj/`.
10. Copies `install.sh`, `uninstall.sh`, `select.sh`, `check-deps.sh` and
    `Setup/README.md`.
11. Generates `README.txt`, `Build/Config/package-info.env`, `dotnet-info.env` and
    `Build/Config/build-info.json` (schema 2, with SHA-256 of each file).
12. Generates `MANIFEST.sha256`.
13. Creates the deterministic `.tar.xz` and the external `.sha256`.
14. Swaps the staging into the destination (temporary backup + removal).

### 7.1 Metadata

`package-info.env`:

```ini
PRODUCT=Kaizen
PACKAGE_NAME=kaizen-x86_64-linux-4.8.0-beta1
PACKAGE_VERSION=4.8.0-beta1
PACKAGE_VERSION_BASE=4.8.0
CHANNEL=beta
CHANNEL_REVISION=1
ENGINE_VERSION=4.8.beta.mono.custom_build
TEMPLATES_CONFIG=4.8.beta.mono
MONO=yes
DOTNET_BUNDLED=yes
DOTNET_SDK=10.0.113
DOTNET_RUNTIME=10.0.13
...
```

`build-info.json` (schema 2) contains `package`, `engine`, `build`, `files` and
`symlinks`. The validator recomputes all hashes. `build-info.json` itself is
**not** included in `files` (it does not exist yet when the list is computed);
`MANIFEST.sha256` is generated afterwards and covers all regular files,
except itself. In this build: 3907 files + 5 symlinks in the JSON,
3908 entries in `MANIFEST.sha256`.

### 7.2 Official archive

```text
kaizen-x86_64-linux-<version>.tar.xz
kaizen-x86_64-linux-<version>.tar.xz.sha256
```

Root `Kaizen/`, name sorting, fixed mtime, owner 0/0, no ACL/SELinux/
xattrs, expanded hardlinks (`--hard-dereference`), `xz -T<jobs>`.

### 7.3 Reproducibility

- `--reproducible` pins `mtime` and `generated_at` to `946684800`
  (2000-01-01T00:00:00Z).
- `SOURCE_DATE_EPOCH` takes precedence.
- The manifest's `host` field and the .NET origin path are omitted
  when reproducible.
- Tested: two runs produced the same SHA-256.

### 7.4 Portable mode

`_sc_` next to the executable makes `EditorPaths` use `exe_dir/editor_data/`.
The tree is movable and does not depend on the repository. The wrapper is POSIX sh and
works in any shell.

---

## 8. Installation

### 8.1 Modes and options

```bash
./Setup/install.sh [--user|--system] [--prefix DIR]
    [--opt-root DIR] [--bin-dir DIR] [--share-dir DIR] [--data-home DIR]
    [--from-tarball FILE] [--channel NAME] [--version VER]
    [--select[=CHANNEL]] [--no-select] [--templates-only]
    [--no-share-dotnet] [--force] [--dry-run] [-y|--yes]
```

- `--user` (default): `~/.local/opt/kaizen`, `~/.local/bin`,
  `$XDG_DATA_HOME` (or `~/.local/share`).
- `--system`: `/opt/kaizen`, `/usr/local/bin`, `/usr/share`; the templates go
  to the XDG directory of the user running the installer (engine limitation, see §8.6).
- `--from-tarball` verifies the external `.sha256`, inspects the entries,
  extracts into a private directory and validates the manifest.
- `--templates-only` installs only the templates (useful for other users).
- `--force` replaces an already-installed version.

### 8.2 Checks before writing

1. external file checksum;
2. SHA-256 manifest and complete coverage;
3. relative paths, no `..`/absolute paths, safe entry types;
4. symlinks relative and contained in the tree;
5. product, `linux` platform, `x86_64` architecture, channel and version;
6. free space (with an estimate already accounting for shared .NET);
7. write permission on the destinations.

### 8.3 Transaction and rollback

- The tree is copied to `<channel>/.<version>.staging.$$`.
- The private .NET is moved to `.dotnet/<sdk>` (or reused) before the swap.
- The existing version (with `--force`) becomes a backup; on error, the backup is
  restored and the staging/launchers are cleaned up.
- Launchers and selection are updated only after the engine in staging answers
  `--version` correctly.
- The `.kaizen-receipt.env` receipt records channel, version, paths and SDK.

### 8.4 XDG integration

- `.desktop` with `Exec=<bin-dir>/kaizen %f` (absolute path).
- AppStream in `metainfo/`, MIME in `mime/packages/`, man in `man/man6/`,
  icons in `icons/hicolor/{scalable,256x256}/apps/`.
- `update-desktop-database`, `update-mime-database` and
  `gtk-update-icon-cache` are executed best-effort.
- No shell file (`~/.bashrc`, `~/.profile`, `~/.zshrc`) is edited.

### 8.5 Selection

`kaizen` reads `<opt-root>/default-channel` and falls back in deterministic order
(`stable`, `rc`, `beta`, `dev`) if the channel does not exist. `kaizen-select`:

```bash
kaizen-select --list
kaizen-select --show
kaizen-select beta
kaizen-select beta/4.8.0-beta1
```

### 8.6 Templates and global installation

The engine only looks for templates in
`$XDG_DATA_HOME/kaizen/export_templates/<FULL_CONFIG>`. There is no system-wide
path. Consequences:

- a global installation installs the templates in the XDG of the user who ran
  the installer;
- other users need to extract the archive and run
  `./Setup/install.sh --templates-only` with their own `HOME`;
- `--templates-only` does **not** work from an already-installed tree
  (it does not contain `Templates/`), only from the archive/portable tree.

---

## 9. Uninstallation — `Setup/uninstall.sh`

```bash
./Setup/uninstall.sh --list
./Setup/uninstall.sh --channel beta --version 4.8.0-beta1
./Setup/uninstall.sh --all [--channel beta] --yes
/opt/kaizen/beta/4.8.0-beta1/Setup/uninstall.sh   # infers everything from the receipt
```

- Removes only `<opt-root>/<channel>/<version>` and validates the receipt first.
- Updates `<channel>/current` and `default-channel` (deterministic fallback).
- Removes `.dotnet/<sdk>` only if no remaining receipt uses the SDK.
- Removes templates only if no remaining receipt shares the same
  `TEMPLATES_CONFIG`.
- Removes launchers/icons/`.desktop`/MIME/man only when the last channel is gone.
- Never removes projects, `app_userdata`, configuration or caches; `rmdir` only
  acts on empty directories.

---

## 10. Validation — `packaging/linux/validate.sh`

```bash
./packaging/linux/validate.sh --mono [--global] [--gui]
./packaging/linux/validate.sh --mono --dist DIR --tarball FILE
./packaging/linux/validate.sh --mono --no-install --no-tarball --skip-run
```

Check blocks (each reports PASS/WARN/FAIL):

1. **Layout**: fresh tree, icons, MIME, man, licenses, samples, executable
   Setup, absence of legacy paths and of empty directories.
2. **Metadata**: required keys, version format, channel, name,
   `TEMPLATES_CONFIG` × engine, `SOURCE_INCLUDED`.
3. **Binary**: ELF x86-64, permission, `kaizen` wrapper (not a symlink),
   `DOTNET_ROOT`, `exec` target, `_sc_`, `--version`.
4. **Templates**: Release/Debug, portable path and the `Templates/` view with
   resolvable symlinks.
5. **.NET**: `KaizenSharp` next to the executable, DLLs, `.deps.json`, nupkg,
   `KaizenPlugins`; if bundled, host, license, SDK × metadata and isolated
   execution of `dotnet`.
6. **JSON manifest**: schema 2, files/sizes/hashes, symlinks.
7. **MANIFEST.sha256**: complete verification and coverage of all files.
8. **Symlinks**: relative and contained.
9. **Hygiene**: no `.o/.a/.lib/.sconsign/__pycache__/*.pyc`.
10. **Dependencies**: `ldd` without `not found`.
11. **Runtime**: disposable copy, isolated `HOME`/XDG, `--version` and
    `--headless --quit`.
12. **Optional GUI**: Project Manager with `--quit-after`.
13. **Tarball**: name, checksum, entries inside `Kaizen/`, safe types,
    extraction, manifest and execution of the extracted engine.
14. **Installation**: per-user install in isolated HOME/XDG, launcher,
    `kaizen-select`, XDG, receipt, absence of `_sc_`/duplicate templates,
    intact `.bashrc`.
15. **Coexistence**: second channel (fixture), launchers, `--no-select`,
    default swap.
16. **Uninstallation**: selective removal, preservation of the other version,
    shared templates, default repair, final removal and preservation of
    user data.
17. **Simulated global**: `--system` with `--opt-root/--bin-dir/--share-dir/
    --data-home` in a writable directory, without root; launcher and removal.

Runtime/installation tests use disposable copies; the packaged tree
is not executed in place nor dirtied.

---

## 11. Dependencies (Linux)

Compilation:

- GCC ≥ 11 or Clang ≥ 9
- Python 3.10+, SCons ≥ 4.4 (required by `SConstruct`)
- Headers: X11, Xcursor, Xinerama, XRandR, Xi, XInput2, Wayland, libudev,
  ALSA, PulseAudio, fontconfig, freetype, dbus
- Vulkan loader (runtime) and GPU drivers
- .NET SDK ≥ 10.0.100 (only to rebuild the .NET variant; the mono package
  already ships the private SDK/runtime)

Package runtime:

```bash
ldd dist/Kaizen/Engine/Binaries/Linux/kaizen.linuxbsd.editor.x86_64.mono
# linux-vdso, libm, libgcc_s, libc, ld-linux (+ libdl/libpthread) and X11/GL
```

Optional drivers are loaded via `dlopen` (Vulkan, ALSA, PulseAudio,
DBus, fontconfig): their absence degrades functionality, it does not prevent startup.

`Setup/check-deps.sh` reports .NET (private or system), dynamic
libraries and graphical session; `--strict` fails if a required dependency is
missing.

---

## 12. Windows

The infrastructure in `packaging/windows/` exists, but has **not been validated** on this
host (Fedora, no MSVC/MinGW-w64) and still produces the old layout
(`Engine/ExportTemplates`, `Engine/Build`, no channels/.tar.xz). No
Windows format (MSI, ZIP, installer) has been implemented at this stage.

---

## 13. Known limitations

1. **Windows not validated** and out of scope for this stage.
2. Only **x86_64** on Linux; ARM64/ARM32 not tested.
3. Global installation cannot serve templates to all users
   (engine limitation, §8.6).
4. `--templates-only` only works from the archive/portable tree.
5. Third-party NuGet may require the network; the local feed covers only Kaizen packages.
6. `--with-source` is not a complete development distribution (it does not
   include `bin/obj`); recompiling requires the build dependencies.
7. Signing, distro packages (`.deb`/`.rpm`/Flatpak) and automatic updates
   are out of scope.
8. `doc/translations/` and `tests/` are not distributed.
9. The `.tar.xz` is not cryptographically signed; integrity is guaranteed by the
   `.sha256` published alongside it.

---

## 14. Validation results (Linux)

> **Note:** the measurements below are the historical record of the last run
> (artifact `kaizen-x86_64-linux-4.8.0-beta1.tar.xz`, SHA-256
> `bebd507e28e7601b0321a90cea42b2397180caf79176ff5ccc0cd92ab9a67b00`).
> On 2026-11-17, at explicit request, `dist/` was removed — including the
> `.tar.xz`, the `dist/Kaizen/` tree and `dist/.reference/`. The repository
> today contains only the source; obtaining a build again requires
> rebuilding from scratch (`build.sh` → `package.sh` → `validate.sh`), and the
> hashes/counts may differ.

Measurements from that run:

| Stage | Result |
|---|---|
| Mono editor build | OK — `4.8.beta.mono.custom_build` |
| C# glue / assemblies / NuGet SDK | OK |
| Mono Debug/Release templates | OK |
| Mono package with private .NET | OK — SDK 10.0.113, runtime 10.0.13 |
| Extracted distribution | 745 MB, 3909 files, 5 symlinks |
| `.tar.xz` | 232 MB + `.sha256` |
| Archive SHA-256 | `bebd507e28e7601b0321a90cea42b2397180caf79176ff5ccc0cd92ab9a67b00` |
| Reproducibility | OK — two runs, same SHA-256 |
| `validate.sh --mono --global` | **132 PASS, 0 FAIL, 0 WARN** |
| GDScript export | OK — exported executable runs and prints |
| `--build-solutions` C# | OK with private SDK |
| C# export | OK — exported executable runs and prints |
| Per-user installation + `kaizen-select` | OK |
| beta/rc coexistence (fixture) | OK |
| Selective uninstallation | OK — preserves version and data |
| Simulated global installation | OK (no root) |
| Windows | **not run** |

---

## 15. Troubleshooting

| Symptom | Cause / solution |
|---|---|
| “No export model found” | template version ≠ `KAIZEN_VERSION_FULL_CONFIG`; check `kaizen --version` and the template directory |
| `Unable to find MSBuild` | the wrapper did not find the private .NET; check the `Engine/Managed/dotnet` symlink and `<opt>/.dotnet/<sdk>` |
| Restore fails with `Kaizen.NET.Sdk` | local source missing; use the sample's `NuGet.config` or point to `Tools/nupkgs` |
| `KaizenSharp` not found | keep the symlink in `Engine/Binaries/Linux`; the runtime does not search in XDG |
| Editor cannot find templates in portable mode | check `_sc_` and the symlinks in `editor_data/export_templates/<version>/` |
| `sha256sum -c` fails | corrupted or modified file; repackage or re-download |
| Installation says the channel does not exist | the package has metadata for a different channel; use the correct archive or a compatible `--channel` |
| `kaizen` opens the wrong version | `kaizen-select --list` and `kaizen-select <channel>` |
| No permission in `/opt` | use per-user mode (default) or `sudo --system` |
