# Kaizen Engine — Build & Packaging

Build and packaging infrastructure for the Kaizen Engine. The scripts are
**external to the build system**: they only drive the existing SCons and organize
the artifacts. No SCons option has been changed.

Complete technical documentation: [`docs/BUILD_AND_PACKAGING.md`](../docs/BUILD_AND_PACKAGING.md).

## Layout

```text
packaging/
├── common.sh              # shared helpers (Linux)
├── common.ps1             # shared helpers (Windows)
├── linux/
│   ├── build.sh           # build driver (editor / templates / .NET)
│   ├── package.sh         # tree + deterministic .tar.xz
│   ├── install.sh         # installer (user/global) — goes to Setup/
│   ├── uninstall.sh       # per-channel/version uninstaller — goes to Setup/
│   ├── select.sh          # default channel/version selection — goes to Setup/
│   ├── check-deps.sh      # dependency report — goes to Setup/
│   └── validate.sh        # full validation (tarball, installation, coexistence)
├── windows/               # Windows equivalent (NOT validated on this host)
└── samples/
    ├── hello-kaizen/          # Lua project
    └── hello-kaizen-csharp/   # C# project (.NET)
```

## Linux flow

```bash
# 1. Build (editor + templates + .NET assemblies)
./packaging/linux/build.sh --mono --target all --jobs 24

# 2. Distribution + official archive
./packaging/linux/package.sh --mono
#   dist/Kaizen/
#   dist/kaizen-x86_64-linux-4.8.0-beta1.tar.xz
#   dist/kaizen-x86_64-linux-4.8.0-beta1.tar.xz.sha256

# 3. Full validation (tarball + installation + coexistence + simulated global)
./packaging/linux/validate.sh --mono --global

# 4. Installation
dist/Kaizen/Setup/install.sh                 # per user (default)
sudo dist/Kaizen/Setup/install.sh --system   # global
```

## Version and channel

The **package** version is explicit metadata, never derived from a directory
name:

| Origin | Example |
|---|---|
| `version.py` (`major.minor.patch`) | `4.8.0` |
| `version.py` (`status`) | `beta` |
| `--channel-revision N` | `1` |
| Final version | `4.8.0-beta1` |
| File name | `kaizen-x86_64-linux-4.8.0-beta1.tar.xz` |

- Channels: `stable`, `beta`, `rc`, `dev`.
- Stable has no suffix (`4.8.0`); the others carry `-<channel><N>`.
- `--version X.Y.Z` and `--channel NAME` may override the metadata.
- `package.sh` refuses a channel/version that contradicts the compiled binary,
  unless `--allow-channel-mismatch` is used (tests only).
- The engine version remains `4.8.beta.mono.custom_build`; the templates
  directory remains `4.8.beta.mono`.

## Distribution layout

```text
Kaizen/
├── Engine/
│   ├── Binaries/Linux/          kaizen.linuxbsd.editor.x86_64.mono, kaizen wrapper, _sc_
│   ├── Managed/KaizenSharp/     Api/{Debug,Release}, Tools/ (mono)
│   ├── Managed/dotnet/          private SDK + runtime (when bundled)
│   └── Source/                  only with --with-source
├── ExportTemplates/Linux/       Release/ and Debug/
├── Templates/<version>/         ready-to-install view (symlinks only)
├── Build/Config/                build-info.json, package-info.env, dotnet-info.env
├── Build/Toolchains/            toolchain notes
├── Config/linux/                .desktop, AppStream, MIME, man page, icons
├── LICENSES/                    Kaizen + licenses of the embedded .NET
├── Samples/                     hello-kaizen, hello-kaizen-csharp
├── Setup/                       install.sh, uninstall.sh, select.sh, check-deps.sh
├── README.txt
└── MANIFEST.sha256
```

- `Templates/<version>/` contains **relative symlinks only** to
  `ExportTemplates/`; there is no binary duplication.
- `Engine/Binaries/Linux/editor_data/export_templates/<version>/` also uses
  symlinks, keeping portable mode (`_sc_`).
- No empty directory is created.

## Metadata and integrity

- `Build/Config/package-info.env`: product, package version, channel, revision,
  engine version, templates config, mono, .NET, architecture.
- `Build/Config/build-info.json` (schema 2): metadata + SHA-256 and size of
  every file + symlink targets.
- `Build/Config/dotnet-info.env`: bundled SDK/runtime (when applicable).
- `MANIFEST.sha256`: hash of every regular file (symlinks are validated
  structurally).
- `*.tar.xz.sha256`: external checksum of the official archive.

The installer verifies the external checksum, the manifest, the architecture,
the channel/version, the symlinks (relative and contained) and free space before
writing anything.

## Reproducibility

```bash
./packaging/linux/package.sh --mono --reproducible
SOURCE_DATE_EPOCH=1700000000 ./packaging/linux/package.sh --mono --reproducible
```

With the same toolchain and `SOURCE_DATE_EPOCH`, two runs produce the same
`.tar.xz` byte for byte. The archive uses `tar --sort=name --mtime --owner=0
--group=0 --numeric-owner --hard-dereference` and `xz -T`.

## .NET

The `--mono` variant embeds a private SDK and runtime in
`Engine/Managed/dotnet/` (a subset of .NET 10: host, SDK,
`Microsoft.NETCore.App` runtime, reference pack and licenses). The `kaizen`
wrapper injects `DOTNET_ROOT`/`PATH` before executing the engine, without
touching the system's .NET.

On install, that content is shared in `<opt>/.dotnet/<sdk>/` and each version
points to it via symlink; thus multiple versions do not duplicate ~300 MB.

The private SDK compiles and exports C# projects. NuGet restore may still
require the network or a local source (`Tools/nupkgs`).

## Installation
```bash
./Setup/install.sh                       # ~/.local (default)
./Setup/install.sh --from-tarball FILE   # verifies checksum and extracts
./Setup/install.sh --select              # makes the installed channel the default
sudo ./Setup/install.sh --system         # /opt/kaizen + /usr/local + /usr/share
```

Per-user layout:

```text
~/.local/opt/kaizen/<channel>/<version>/  installed tree + receipt
~/.local/opt/kaizen/<channel>/current -> <version>
~/.local/opt/kaizen/.dotnet/<sdk>/        shared private .NET
~/.local/opt/kaizen/default-channel       default channel
~/.local/bin/kaizen                       default-channel launcher
~/.local/bin/kaizen-<channel>             per-channel launcher
~/.local/bin/kaizen-select                selection
~/.local/share/kaizen/export_templates/   templates
```

- `kaizen` is a generated wrapper (not a symlink), to locate the private .NET.
- Changing channel/version: `kaizen-select --list`, `kaizen-select beta`,
  `kaizen-select beta/4.8.0-beta1`.
- The installer **never** edits `~/.bashrc`/`~/.profile`; it only reports the
  required `PATH`.
- Overwriting an existing installation requires `--force`.

Limitation: the engine only looks for templates in
`$XDG_DATA_HOME/kaizen/export_templates/<version>`. Therefore a global
installation installs the templates in the XDG of the user who ran the installer. For
other users, extract the archive and run `./Setup/install.sh --templates-only`
with each user's own `HOME`.

## Uninstallation

```bash
./Setup/uninstall.sh --list
./Setup/uninstall.sh --channel beta --version 4.8.0-beta1
./Setup/uninstall.sh --all --yes
```

Removes only the files of that channel/version, updates pointers and keeps
shared templates/.NET while another version uses them. Projects,
configuration and caches are never removed. The default selection is re-evaluated.

## Validation

```bash
./packaging/linux/validate.sh --mono [--global] [--gui]
./packaging/linux/validate.sh --mono --dist DIR --tarball FILE
```

The validator keeps the structural and runtime checks and adds:

- new layout, metadata, JSON manifest (schema 2) and `MANIFEST.sha256`;
- relative/contained symlinks and absence of empty directories;
- `.tar.xz`: name, external checksum, entries inside `Kaizen/`, safe
  types, extraction and execution of the extracted engine;
- per-user installation in isolated `HOME`/XDG, launcher, selection, XDG;
- coexistence of two channels and selective uninstallation with data preservation;
- simulated global installation (no root) with `--opt-root/--bin-dir/...`.

The packaged tree is never executed in place: runtime and installation tests
run on disposable copies.

## Windows

```powershell
.\packaging\windows\build.ps1   -Target all -Mono -Jobs 16
.\packaging\windows\package.ps1 -Mono yes -Output dist\Kaizen -Force
.\packaging\windows\validate.ps1 -Dist dist\Kaizen -Mono yes
```

> **Not validated.** This host is Fedora, without MSVC/MinGW-w64. The Windows
> flow predates the new channel structure and the `.tar.xz`; it will be updated
> when a Windows host is available. No Windows format (MSI, ZIP) has been
> implemented at this stage.

## Conventions

- SCons targets: `editor`, `template_debug`, `template_release`.
- Binary: `kaizen.<platform>.<target>.<arch>[.mono]`.
- Linux templates: `linux_release.x86_64`, `linux_debug.x86_64`.
- Templates in `KAIZEN_VERSION_FULL_CONFIG` (`4.8.beta[.mono]`).
- Project: `project.kaizen`; data in `.kaizen/`.

## What is never included

`bin/obj/`, `.sconsign*.dblite`, `.scons_env.json`, `__pycache__/`, `*.pyc`,
`*.o`, `*.a`, `*.lib`, `doc/translations/`, `tests/`, `obj/` residue.

## Reference

A reference copy of the binaries from a previous build was preserved in
`dist/.reference/` with `SHA256SUMS`, but was **removed** along with the
`dist/` directory (which also contained the official delivery), by explicit
decision: the repository today contains only the source. Those files were not
part of any generated distribution. Obtaining an artifact again requires
rebuilding everything from scratch.
