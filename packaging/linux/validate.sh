#!/usr/bin/env bash
#
# Kaizen Engine — distribution validator (Linux).
#
# Verifies structure, metadata, checksums, binaries, export templates, managed
# (.NET) files, runtime behavior, the official .tar.xz archive and the
# installer/uninstaller (user, coexistence and simulated system-wide modes).
#
# Usage:
#   ./packaging/linux/validate.sh [options]
#
# Options:
#   -d, --dist DIR      Distribution directory (default: dist/Kaizen)
#   -t, --tarball FILE  Archive to validate (default: auto-detect beside DIST)
#       --no-tarball    Skip archive validation
#       --no-install    Skip installation/uninstallation tests
#       --global        Also test a simulated system-wide installation
#       --mono          Expect a .NET-enabled distribution
#       --no-mono       Expect a distribution without .NET
#                       (default: auto-detect)
#       --gui           Also start the editor once (requires a display)
#       --skip-run      Do not execute the packaged binary
#   -h, --help          Show this help

set -euo pipefail

# shellcheck source=../common.sh
source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/../common.sh"

usage() {
	sed -n '2,25p' "$0" | sed 's/^# \{0,1\}//'
	exit "${1:-0}"
}

DIST="${KZ_DIST_DIR}"
TARBALL=""
TARBALL_AUTO="yes"
INSTALL_TESTS="yes"
GLOBAL_TEST="no"
MONO="auto"
SKIP_RUN="no"
GUI="no"

while [[ $# -gt 0 ]]; do
	case "$1" in
	-d | --dist)
		[[ $# -ge 2 ]] || kz_die "--dist requires a value."
		DIST="$2"
		shift 2
		;;
	-t | --tarball)
		[[ $# -ge 2 ]] || kz_die "--tarball requires a value."
		TARBALL="$2"
		TARBALL_AUTO="no"
		shift 2
		;;
	--no-tarball)
		TARBALL_AUTO="no"
		shift
		;;
	--no-install)
		INSTALL_TESTS="no"
		shift
		;;
	--global)
		GLOBAL_TEST="yes"
		shift
		;;
	--mono)
		MONO="yes"
		shift
		;;
	--no-mono)
		MONO="no"
		shift
		;;
	--skip-run)
		SKIP_RUN="yes"
		shift
		;;
	--gui)
		GUI="yes"
		shift
		;;
	-h | --help)
		usage 0
		;;
	*)
		kz_die "Unknown option: $1 (use --help)"
		;;
	esac
done

kz_need_cmd python3 find sort sha256sum tar
[[ "${TARBALL_AUTO}" == "yes" || -n "${TARBALL}" ]] && kz_need_cmd xz

PASS=0
FAIL=0
WARN=0
FAILED_ITEMS=()

# Scratch directories are removed on exit. Runtime and installation tests run
# against disposable copies so the packaged tree stays pristine (portable mode
# writes editor_data/ next to the executable).
SCRATCH_DIRS=()
cleanup_scratch() {
	local d
	for d in ${SCRATCH_DIRS[@]+"${SCRATCH_DIRS[@]}"}; do
		[[ -n "${d}" ]] && rm -rf "${d}"
	done
	return 0
}
trap cleanup_scratch EXIT

check_ok() {
	PASS=$((PASS + 1))
	printf '  %sPASS%s %s\n' "${KZ_C_OK}" "${KZ_C_RESET}" "$1"
}
check_warn() {
	WARN=$((WARN + 1))
	printf '  %sWARN%s %s\n' "${KZ_C_WARN}" "${KZ_C_RESET}" "$1"
}
check_fail() {
	FAIL=$((FAIL + 1))
	FAILED_ITEMS+=("$1")
	printf '  %sFAIL%s %s\n' "${KZ_C_ERR}" "${KZ_C_RESET}" "$1"
}
require_file() { [[ -f "$1" ]] && check_ok "$2" || check_fail "$2 (missing: $1)"; }
require_exec() { [[ -x "$1" ]] && check_ok "$2" || check_fail "$2 (not executable: $1)"; }
require_dir() { [[ -d "$1" ]] && check_ok "$2" || check_fail "$2 (missing: $1)"; }
require_link() { [[ -L "$1" && -e "$1" ]] && check_ok "$2" || check_fail "$2 (missing or broken: $1)"; }

[[ -d "${DIST}" ]] || kz_die "Distribution directory not found: ${DIST}"
DIST="$(cd "${DIST}" && pwd)"
BIN_DIR="${DIST}/Engine/Binaries/${KZ_OS_DIR}"
TEMPLATES_DIR="${DIST}/ExportTemplates/${KZ_OS_DIR}"
MANAGED_DIR="${DIST}/Engine/Managed/KaizenSharp"
BUILD_INFO="${DIST}/Build/Config/build-info.json"
PKG_INFO="${DIST}/Build/Config/package-info.env"
MANIFEST="${DIST}/MANIFEST.sha256"

pkg_get() { { grep -m1 "^$2=" "$1" 2>/dev/null || true; } | cut -d= -f2-; }

CANONICAL=""
for f in "${BIN_DIR}"/kaizen.linuxbsd.*; do
	[[ -f "${f}" ]] || continue
	CANONICAL="${f}"
	break
done

if [[ "${MONO}" == "auto" ]]; then
	if [[ -n "${CANONICAL}" && "${CANONICAL}" == *.mono ]] || [[ -e "${DIST}/Engine/Managed/dotnet" ]]; then
		MONO="yes"
	elif [[ -e "${BIN_DIR}/KaizenSharp" ]]; then
		MONO="yes"
	else
		MONO="no"
	fi
fi

PACKAGE_VERSION=""
PACKAGE_NAME=""
CHANNEL=""
ENGINE_VERSION=""
TEMPLATES_CONFIG=""
DOTNET_BUNDLED="no"
SOURCE_INCLUDED="no"
if [[ -f "${PKG_INFO}" ]]; then
	PACKAGE_VERSION="$(pkg_get "${PKG_INFO}" PACKAGE_VERSION)"
	PACKAGE_NAME="$(pkg_get "${PKG_INFO}" PACKAGE_NAME)"
	CHANNEL="$(pkg_get "${PKG_INFO}" CHANNEL)"
	ENGINE_VERSION="$(pkg_get "${PKG_INFO}" ENGINE_VERSION)"
	TEMPLATES_CONFIG="$(pkg_get "${PKG_INFO}" TEMPLATES_CONFIG)"
	DOTNET_BUNDLED="$(pkg_get "${PKG_INFO}" DOTNET_BUNDLED)"
	SOURCE_INCLUDED="$(pkg_get "${PKG_INFO}" SOURCE_INCLUDED)"
fi

kz_info "Validating Kaizen distribution"
kz_info "  dist : ${DIST}"
kz_info "  mono : ${MONO}"

# --- 1. Layout -----------------------------------------------------------
kz_info "Layout"
require_dir "${BIN_DIR}" "engine binaries directory"
require_dir "${DIST}/ExportTemplates/Linux/Release" "release export templates directory"
require_dir "${DIST}/ExportTemplates/Linux/Debug" "debug export templates directory"
require_dir "${DIST}/Build/Config" "build configuration directory"
require_dir "${DIST}/Build/Toolchains" "toolchain notes directory"
require_dir "${DIST}/Config/linux" "Linux integration directory"
require_dir "${DIST}/Config/linux/icons/hicolor/scalable/apps" "scalable icon directory"
require_dir "${DIST}/Config/linux/icons/hicolor/256x256/apps" "256x256 icon directory"
require_dir "${DIST}/LICENSES" "licenses directory"
require_dir "${DIST}/Samples/hello-kaizen" "Lua sample directory"
require_dir "${DIST}/Setup" "setup directory"
require_file "${DIST}/README.txt" "root README"
require_file "${DIST}/LICENSES/LICENSE.txt" "Kaizen license"
require_file "${DIST}/Config/linux/org.kaizenengine.Kaizen.desktop" "desktop entry"
require_file "${DIST}/Config/linux/org.kaizenengine.Kaizen.appdata.xml" "AppStream metadata"
require_file "${DIST}/Config/linux/org.kaizenengine.Kaizen.xml" "MIME definition"
require_file "${DIST}/Config/linux/kaizen.6" "manual page"
require_file "${DIST}/Config/linux/icons/hicolor/scalable/apps/kaizen.svg" "scalable icon"
require_file "${DIST}/Config/linux/icons/hicolor/256x256/apps/kaizen.png" "256x256 icon"
require_file "${DIST}/Samples/hello-kaizen/project.kaizen" "Lua sample project"

for helper in install.sh uninstall.sh select.sh check-deps.sh; do
	require_exec "${DIST}/Setup/${helper}" "Setup/${helper} is executable"
done

if [[ "${MONO}" == "yes" ]]; then
	require_dir "${DIST}/Samples/hello-kaizen-csharp" "C# sample directory"
	require_file "${DIST}/Samples/hello-kaizen-csharp/HelloKaizenCSharp.csproj" "C# sample project"
else
	[[ -e "${DIST}/Samples/hello-kaizen-csharp" ]] &&
		check_warn "C# sample present in a non-mono package" ||
		check_ok "no C# sample in a non-mono package"
fi

if [[ "${SOURCE_INCLUDED}" == "yes" ]]; then
	require_dir "${DIST}/Engine/Source" "engine source (SOURCE_INCLUDED=yes)"
else
	[[ -e "${DIST}/Engine/Source" ]] &&
		check_fail "Engine/Source present but metadata says SOURCE_INCLUDED=no" ||
		check_ok "engine source omitted (SOURCE_INCLUDED=no)"
fi

# The old layout must not linger: it would duplicate templates.
for legacy in Engine/ExportTemplates Engine/Build Engine/Config; do
	[[ -e "${DIST}/${legacy}" ]] && check_fail "legacy path still present: ${legacy}" || check_ok "no legacy path: ${legacy}"
done

empty_dirs="$(find "${DIST}" -type d -empty -print 2>/dev/null || true)"
[[ -z "${empty_dirs}" ]] && check_ok "no empty directories" || check_fail "empty directories: $(tr '\n' ' ' <<<"${empty_dirs}")"

# --- 2. Package metadata -------------------------------------------------
kz_info "Package metadata"
if [[ -f "${PKG_INFO}" ]]; then
	check_ok "package metadata exists"
	[[ -n "${PACKAGE_VERSION}" && -n "${CHANNEL}" && -n "${ENGINE_VERSION}" ]] &&
		check_ok "required metadata keys present" ||
		check_fail "package metadata is missing required keys"
	[[ "${PACKAGE_VERSION}" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-(beta|rc|dev)[1-9][0-9]*)?$ ]] &&
		check_ok "package version format: ${PACKAGE_VERSION}" ||
		check_fail "invalid package version: ${PACKAGE_VERSION}"
	case "${CHANNEL}" in
	stable | beta | rc | dev) check_ok "package channel: ${CHANNEL}" ;;
	*) check_fail "invalid package channel: ${CHANNEL}" ;;
	esac
	if [[ "${CHANNEL}" == "stable" ]]; then
		[[ "${PACKAGE_VERSION}" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]] &&
			check_ok "stable version has no pre-release suffix" ||
			check_fail "stable version carries a pre-release suffix: ${PACKAGE_VERSION}"
	else
		[[ "${PACKAGE_VERSION}" == *-"${CHANNEL}"[0-9]* ]] &&
			check_ok "pre-release version matches its channel" ||
			check_fail "version '${PACKAGE_VERSION}' does not match channel '${CHANNEL}'"
	fi
	expected_name="$(kz_package_name "${PACKAGE_VERSION}")"
	[[ "${PACKAGE_NAME}" == "${expected_name}" ]] &&
		check_ok "package name follows the convention: ${PACKAGE_NAME}" ||
		check_fail "unexpected package name: ${PACKAGE_NAME} (expected ${expected_name})"
	[[ "${TEMPLATES_CONFIG}" == "$(kz_version_full_config "${ENGINE_VERSION}")" ]] &&
		check_ok "templates config matches the engine version" ||
		check_fail "templates config '${TEMPLATES_CONFIG}' does not match '${ENGINE_VERSION}'"
else
	check_fail "package metadata missing: ${PKG_INFO}"
fi

# --- 3. Engine binary ----------------------------------------------------
kz_info "Engine binary"
if [[ -z "${CANONICAL}" ]]; then
	check_fail "canonical engine binary (kaizen.linuxbsd.*) not found"
else
	check_ok "canonical binary: $(basename "${CANONICAL}")"
	require_exec "${CANONICAL}" "canonical binary is executable"
	if [[ "${CANONICAL}" == *.mono && "${MONO}" != "yes" ]]; then
		check_fail "mono binary present but --no-mono was requested"
	fi
	if head -c 4 "${CANONICAL}" | grep -q $'\x7fELF'; then
		check_ok "canonical binary is an ELF executable"
	else
		check_fail "canonical binary is not an ELF executable"
	fi
	arch="$(file -b "${CANONICAL}" 2>/dev/null || true)"
	[[ "${arch}" == *x86-64* ]] && check_ok "architecture is x86-64" || check_warn "could not confirm x86-64 architecture (file: ${arch})"
	[[ -f "${BIN_DIR}/_sc_" ]] && check_ok "portable marker (_sc_) present" || check_warn "portable marker (_sc_) missing (portable mode disabled)"

	# The launcher is a generated POSIX-sh wrapper, not a symlink, so it can
	# locate a bundled .NET runtime regardless of PATH.
	if [[ -f "${BIN_DIR}/kaizen" && ! -L "${BIN_DIR}/kaizen" ]]; then
		check_ok "'kaizen' launcher is a generated wrapper"
		if grep -q 'DOTNET_ROOT' "${BIN_DIR}/kaizen"; then
			check_ok "launcher configures a private .NET root"
		else
			check_fail "launcher does not configure DOTNET_ROOT"
		fi
		if grep -q "exec \"\$dir/$(basename "${CANONICAL}")\"" "${BIN_DIR}/kaizen" ||
			grep -q "exec .*$(basename "${CANONICAL}")" "${BIN_DIR}/kaizen"; then
			check_ok "launcher target matches the canonical binary"
		else
			check_fail "launcher does not target $(basename "${CANONICAL}")"
		fi
	else
		check_fail "'kaizen' launcher is missing or is a symlink"
	fi
fi

# --- 4. Export templates -------------------------------------------------
kz_info "Export templates"
require_exec "${TEMPLATES_DIR}/Release/linux_release.${KZ_ARCH}" "release template"
require_exec "${TEMPLATES_DIR}/Debug/linux_debug.${KZ_ARCH}" "debug template"

if [[ -n "${CANONICAL}" && -x "${CANONICAL}" ]]; then
	FULL_BUILD="$("${CANONICAL}" --version 2>/dev/null | head -n1 | tr -d '\r')" || FULL_BUILD=""
	if [[ -n "${FULL_BUILD}" ]]; then
		check_ok "engine version: ${FULL_BUILD}"
		FULL_CONFIG="$(kz_version_full_config "${FULL_BUILD}")"
		if [[ -n "${TEMPLATES_CONFIG}" && "${FULL_CONFIG}" != "${TEMPLATES_CONFIG}" ]]; then
			check_fail "engine config '${FULL_CONFIG}' differs from metadata '${TEMPLATES_CONFIG}'"
		fi
		PORTABLE_TEMPLATES="${BIN_DIR}/editor_data/export_templates/${FULL_CONFIG}"
		if [[ -d "${PORTABLE_TEMPLATES}" ]]; then
			check_ok "portable template path uses '${FULL_CONFIG}'"
			[[ -e "${PORTABLE_TEMPLATES}/linux_release.${KZ_ARCH}" ]] && check_ok "portable release template resolves" || check_fail "portable release template does not resolve"
			[[ -e "${PORTABLE_TEMPLATES}/linux_debug.${KZ_ARCH}" ]] && check_ok "portable debug template resolves" || check_fail "portable debug template does not resolve"
		else
			check_fail "portable template directory missing: ${PORTABLE_TEMPLATES}"
		fi
		VIEW_DIR="${DIST}/Templates/${FULL_CONFIG}"
		if [[ -d "${VIEW_DIR}" ]]; then
			check_ok "install-ready template view exists"
			[[ -e "${VIEW_DIR}/linux_release.${KZ_ARCH}" ]] && check_ok "template view release link resolves" || check_fail "template view release link does not resolve"
			[[ -e "${VIEW_DIR}/linux_debug.${KZ_ARCH}" ]] && check_ok "template view debug link resolves" || check_fail "template view debug link does not resolve"
			[[ -L "${VIEW_DIR}/linux_release.${KZ_ARCH}" ]] &&
				check_ok "template view contains symlinks (no duplication)" ||
				check_fail "template view contains duplicated template binaries"
		else
			check_fail "install-ready template view missing: ${VIEW_DIR}"
		fi
	else
		check_fail "could not read version from the engine binary"
	fi
fi

# --- 5. Managed (.NET) ---------------------------------------------------
if [[ "${MONO}" == "yes" ]]; then
	kz_info "Managed assemblies (.NET)"
	require_link "${BIN_DIR}/KaizenSharp" "KaizenSharp resolves next to the executable"
	require_file "${MANAGED_DIR}/Api/Release/KaizenSharp.dll" "KaizenSharp API (Release)"
	require_file "${MANAGED_DIR}/Api/Debug/KaizenSharp.dll" "KaizenSharp API (Debug)"
	require_file "${MANAGED_DIR}/Api/Release/KaizenSharpEditor.dll" "KaizenSharp editor API (Release)"
	require_file "${MANAGED_DIR}/Tools/KaizenTools.dll" "KaizenTools assembly"
	require_file "${MANAGED_DIR}/Tools/KaizenTools.deps.json" "KaizenTools dependency manifest"
	if compgen -G "${MANAGED_DIR}/Tools/nupkgs/Kaizen.NET.Sdk.*.nupkg" >/dev/null; then
		check_ok "Kaizen.NET.Sdk NuGet package present"
	else
		check_fail "Kaizen.NET.Sdk NuGet package missing from Tools/nupkgs"
	fi
	if compgen -G "${MANAGED_DIR}/Api/Release/KaizenPlugins.dll" >/dev/null; then
		check_ok "KaizenPlugins runtime present"
	else
		check_fail "KaizenPlugins runtime missing"
	fi
	if [[ "${DOTNET_BUNDLED}" == "yes" ]]; then
		require_exec "${DIST}/Engine/Managed/dotnet/dotnet" "bundled .NET host is executable"
		require_file "${DIST}/LICENSES/dotnet/LICENSE.txt" "bundled .NET license"
		sdk_meta="$(pkg_get "${DIST}/Build/Config/dotnet-info.env" DOTNET_SDK)"
		if [[ -n "${sdk_meta}" && -d "${DIST}/Engine/Managed/dotnet/sdk/${sdk_meta}" ]]; then
			check_ok "bundled .NET SDK matches metadata: ${sdk_meta}"
		else
			check_fail "bundled .NET SDK does not match metadata: ${sdk_meta:-<unknown>}"
		fi
		bundled_version="$(env -u DOTNET_ROOT HOME="${DIST}" PATH="/usr/bin:/bin" "${DIST}/Engine/Managed/dotnet/dotnet" --version 2>/dev/null || true)"
		[[ -n "${bundled_version}" ]] &&
			check_ok "bundled .NET runs without the system installation: ${bundled_version}" ||
			check_fail "bundled .NET does not run in isolation"
	else
		[[ -d "${DIST}/Engine/Managed/dotnet" ]] &&
			check_warn "a .NET copy exists but metadata says DOTNET_BUNDLED=no" ||
			check_ok "no bundled .NET (system SDK required)"
	fi
elif [[ -e "${DIST}/Engine/Managed/dotnet" ]]; then
	check_fail "bundled .NET present in a non-mono package"
fi

# --- 6. Build manifest (JSON) -------------------------------------------
kz_info "Build manifest"
if [[ -f "${BUILD_INFO}" ]]; then
	check_ok "build manifest exists"
	KZ_VALIDATE_ROOT="${DIST}" KZ_VALIDATE_MANIFEST="${BUILD_INFO}" python3 - <<'PY' || FAIL=$((FAIL + 1))
import hashlib
import json
import os
import sys
from pathlib import Path

root = Path(os.environ["KZ_VALIDATE_ROOT"]).resolve()
manifest_path = Path(os.environ["KZ_VALIDATE_MANIFEST"])
errors = []

try:
    data = json.loads(manifest_path.read_text(encoding="utf-8"))
except Exception as exc:
    print(f"  FAIL manifest is not valid JSON: {exc}")
    sys.exit(1)

if data.get("schema") != 2:
    errors.append(f"unsupported manifest schema: {data.get('schema')!r}")
if not data.get("package", {}).get("version"):
    errors.append("manifest has no package version")
if not data.get("engine", {}).get("version_full_build"):
    errors.append("manifest has no engine version")

for entry in data.get("files", []):
    rel = entry["path"]
    target = (root / rel).resolve()
    if not str(target).startswith(str(root) + os.sep):
        errors.append(f"manifest path escapes the distribution: {rel}")
        continue
    if not target.is_file():
        errors.append(f"missing file: {rel}")
        continue
    digest = hashlib.sha256()
    with target.open("rb") as fh:
        for chunk in iter(lambda: fh.read(1024 * 1024), b""):
            digest.update(chunk)
    if digest.hexdigest() != entry["sha256"]:
        errors.append(f"checksum mismatch: {rel}")
    elif target.stat().st_size != entry["size"]:
        errors.append(f"size mismatch: {rel}")

for entry in data.get("symlinks", []):
    rel = entry["path"]
    target = root / rel
    if not target.is_symlink():
        errors.append(f"missing symlink: {rel}")
    elif os.readlink(target) != entry["target"]:
        errors.append(f"symlink target mismatch: {rel}")

if errors:
    for err in errors:
        print(f"  FAIL {err}")
    sys.exit(1)

print(f"  PASS manifest verified ({len(data.get('files', []))} files, {len(data.get('symlinks', []))} symlinks)")
PY
else
	check_fail "build manifest missing: ${BUILD_INFO}"
fi

# --- 7. SHA-256 file manifest -------------------------------------------
kz_info "SHA-256 manifest"
if [[ -f "${MANIFEST}" ]]; then
	check_ok "MANIFEST.sha256 exists"
	if ( cd "${DIST}" && sha256sum -c --quiet --strict MANIFEST.sha256 >/dev/null 2>&1 ); then
		check_ok "all files match MANIFEST.sha256"
	else
		check_fail "MANIFEST.sha256 verification failed"
	fi
	listed="$(grep -c . "${MANIFEST}")"
	actual="$(find "${DIST}" -type f ! -name MANIFEST.sha256 | wc -l)"
	if [[ "${listed}" == "${actual}" ]]; then
		check_ok "manifest covers every regular file (${listed})"
	else
		check_fail "manifest coverage mismatch: ${listed} listed, ${actual} present"
	fi
	bad="$(grep -vE '^[0-9a-f]{64}  [^/]' "${MANIFEST}" | head -n1 || true)"
	[[ -z "${bad}" ]] && check_ok "manifest entries are relative" || check_fail "unsafe manifest entry: ${bad}"
else
	check_fail "MANIFEST.sha256 missing"
fi

# --- 8. Symlink safety ---------------------------------------------------
kz_info "Symlinks"
unsafe_links=""
link_count=0
while IFS= read -r -d '' link; do
	link_count=$((link_count + 1))
	target="$(readlink "${link}")"
	rel="${link#"${DIST}"/}"
	if [[ "${target}" == /* ]]; then
		unsafe_links+="${rel} (absolute) "
		continue
	fi
	resolved="$(realpath -m "$(dirname "${link}")/${target}")"
	case "${resolved}/" in
	"${DIST}/"*) ;;
	*) unsafe_links+="${rel} (escapes) " ;;
	esac
done < <(find "${DIST}" -type l -print0)
if [[ -z "${unsafe_links}" ]]; then
	check_ok "all ${link_count} symlinks are relative and contained"
else
	check_fail "unsafe symlinks: ${unsafe_links}"
fi

# --- 9. Artifact hygiene -------------------------------------------------
kz_info "Artifact hygiene"
for pattern in '*.o' '*.a' '*.lib' '.sconsign*' '__pycache__' '*.pyc' '*.nupkg.tmp'; do
	found="$(find "${DIST}" -name "${pattern}" -print -quit 2>/dev/null || true)"
	[[ -z "${found}" ]] && check_ok "no ${pattern} artifacts" || check_fail "forbidden build artifact found: ${pattern}"
done

# --- 10. Dynamic dependencies -------------------------------------------
kz_info "Dynamic dependencies"
if [[ -n "${CANONICAL}" ]]; then
	if command -v ldd >/dev/null 2>&1; then
		deps="$(ldd "${CANONICAL}" 2>/dev/null || true)"
		if grep -q "not found" <<<"${deps}"; then
			check_fail "unresolved shared libraries: $(grep 'not found' <<<"${deps}" | tr '\n' ' ')"
		else
			check_ok "all shared library dependencies resolve"
		fi
		unexpected="$(awk '{print $1}' <<<"${deps}" | grep -vE '^(linux-vdso|libm\.so|libc\.so|ld-linux|/lib|/usr)' | sort -u | tr '\n' ' ' || true)"
		[[ -z "${unexpected}" ]] && check_ok "only base system libraries required" || check_warn "extra shared libraries: ${unexpected}"
	else
		check_warn "ldd not available; skipped dependency check"
	fi
fi

# --- Runtime helpers -----------------------------------------------------
SMOKE_BIN=""
prepare_smoke_bin() {
	[[ -n "${SMOKE_BIN}" ]] && return 0
	local root
	root="$(mktemp -d "${TMPDIR:-/tmp}/kaizen-validate.XXXXXX")"
	SCRATCH_DIRS+=("${root}")
	if cp -a --reflink=auto "${DIST}" "${root}/dist" 2>/dev/null; then
		SMOKE_BIN="${root}/dist/Engine/Binaries/${KZ_OS_DIR}"
	else
		kz_warn "could not create a scratch copy of the distribution; runtime tests will run in-place"
		rm -rf "${root}/dist"
		SMOKE_BIN="${BIN_DIR}"
	fi
	return 0
}

# Isolated environment for anything that runs Kaizen.
run_isolated() { # <home> <command...>
	local home="$1"
	shift
	env -u DOTNET_ROOT -u LD_LIBRARY_PATH \
		HOME="${home}" \
		XDG_DATA_HOME="${home}/xdg-data" \
		XDG_CONFIG_HOME="${home}/xdg-config" \
		XDG_CACHE_HOME="${home}/xdg-cache" \
		PATH="/usr/bin:/bin" \
		"$@"
}

# --- 11. Runtime smoke test ---------------------------------------------
if [[ "${SKIP_RUN}" == "no" && -n "${CANONICAL}" ]]; then
	kz_info "Runtime smoke test"
	prepare_smoke_bin
	ISOLATED="$(mktemp -d "${TMPDIR:-/tmp}/kaizen-validate.XXXXXX")"
	SCRATCH_DIRS+=("${ISOLATED}")
	mkdir -p "${ISOLATED}/home" "${ISOLATED}/home2"
	if run_isolated "${ISOLATED}/home" "${SMOKE_BIN}/kaizen" --version >/dev/null 2>&1; then
		check_ok "engine runs with an isolated HOME"
	else
		check_fail "engine failed to run with an isolated HOME"
	fi
	if run_isolated "${ISOLATED}/home2" timeout 120 "${SMOKE_BIN}/kaizen" --headless --quit >"${ISOLATED}/run.log" 2>&1; then
		check_ok "headless startup completed (--headless --quit)"
	else
		check_fail "headless startup failed; see log below"
		tail -20 "${ISOLATED}/run.log" | sed 's/^/        /'
	fi
fi

# --- 12. Optional GUI smoke test ----------------------------------------
if [[ "${GUI}" == "yes" && -n "${CANONICAL}" ]]; then
	kz_info "GUI smoke test (Project Manager)"
	prepare_smoke_bin
	ISOLATED_GUI="$(mktemp -d "${TMPDIR:-/tmp}/kaizen-validate-gui.XXXXXX")"
	SCRATCH_DIRS+=("${ISOLATED_GUI}")
	mkdir -p "${ISOLATED_GUI}/home"
	if run_isolated "${ISOLATED_GUI}/home" timeout 45 "${SMOKE_BIN}/kaizen" --quit-after 120 >"${ISOLATED_GUI}/gui.log" 2>&1; then
		check_ok "editor started and exited cleanly (--quit-after 120)"
	else
		check_warn "editor GUI test did not complete cleanly (display/permissions?); log: ${ISOLATED_GUI}/gui.log"
	fi
fi

# --- 13. Tarball ---------------------------------------------------------
validate_tarball() { # <tarball>
	local file="$1"
	kz_info "Archive"
	[[ -f "${file}" ]] || {
		check_fail "archive not found: ${file}"
		return 0
	}
	check_ok "archive exists: $(basename "${file}")"
	if [[ -f "${file}.sha256" ]]; then
		if ( cd "$(dirname "${file}")" && sha256sum -c --quiet "$(basename "${file}").sha256" >/dev/null 2>&1 ); then
			check_ok "external checksum verifies"
		else
			check_fail "external checksum does not verify"
		fi
	else
		check_fail "external checksum missing: ${file}.sha256"
	fi

	local scratch archive_ok="yes" name types
	scratch="$(mktemp -d "${TMPDIR:-/tmp}/kaizen-archive.XXXXXX")"
	SCRATCH_DIRS+=("${scratch}")
	xz -dc "${file}" >"${scratch}/archive.tar" 2>/dev/null || {
		check_fail "archive cannot be decompressed"
		return 0
	}

	while IFS= read -r name; do
		[[ -z "${name}" ]] && continue
		case "${name}" in
		Kaizen | Kaizen/*) ;;
		*)
			check_fail "archive entry outside Kaizen/: ${name}"
			archive_ok="no"
			;;
		esac
		case "/${name}/" in
		*/../* | */./*)
			check_fail "unsafe archive entry: ${name}"
			archive_ok="no"
			;;
		esac
	done < <(tar -tf "${scratch}/archive.tar")
	[[ "${archive_ok}" == "yes" ]] && check_ok "every archive entry stays inside Kaizen/"

	types="$(tar -tvf "${scratch}/archive.tar" | cut -c1 | sort -u | tr -d '\n')"
	if [[ "${types}" == *[bcpsh]* ]]; then
		check_fail "archive contains device/fifo/hardlink entries: ${types}"
	else
		check_ok "archive entry types are files, directories and symlinks"
	fi

	mkdir -p "${scratch}/extract"
	tar -xpf "${scratch}/archive.tar" -C "${scratch}/extract" --no-same-owner 2>/dev/null ||
		check_fail "archive extraction failed"
	if [[ -d "${scratch}/extract/Kaizen" ]]; then
		check_ok "archive extracts a Kaizen/ root"
		if ( cd "${scratch}/extract/Kaizen" && sha256sum -c --quiet --strict MANIFEST.sha256 >/dev/null 2>&1 ); then
			check_ok "extracted tree matches MANIFEST.sha256"
		else
			check_fail "extracted tree does not match its manifest"
		fi
		if [[ -x "${scratch}/extract/Kaizen/Engine/Binaries/${KZ_OS_DIR}/kaizen" ]]; then
			mkdir -p "${scratch}/home"
			if run_isolated "${scratch}/home" "${scratch}/extract/Kaizen/Engine/Binaries/${KZ_OS_DIR}/kaizen" --version >/dev/null 2>&1; then
				check_ok "engine from the archive runs"
			else
				check_fail "engine from the archive failed to run"
			fi
		fi
		if [[ -n "${PACKAGE_NAME}" ]]; then
			[[ "$(basename "${file}")" == "${PACKAGE_NAME}.tar.xz" ]] &&
				check_ok "archive name matches the package metadata" ||
				check_fail "archive name '$(basename "${file}")' does not match '${PACKAGE_NAME}.tar.xz'"
		fi
	fi
	return 0
}

if [[ "${TARBALL_AUTO}" == "yes" && -z "${TARBALL}" && -n "${PACKAGE_NAME}" ]]; then
	candidate="$(dirname "${DIST}")/${PACKAGE_NAME}.tar.xz"
	[[ -f "${candidate}" ]] && TARBALL="${candidate}"
fi
if [[ -n "${TARBALL}" ]]; then
	validate_tarball "${TARBALL}"
elif [[ "${TARBALL_AUTO}" == "yes" ]]; then
	check_warn "no archive beside ${DIST}; tarball checks skipped"
fi

# --- 14. Installation tests --------------------------------------------
install_from() { # <home> <source-args...>
	local home="$1"
	shift
	run_isolated "${home}" "${DIST}/Setup/install.sh" "$@" --yes
}

if [[ "${INSTALL_TESTS}" == "yes" ]]; then
	kz_info "User installation"
	INSTALL_ROOT="$(mktemp -d "${TMPDIR:-/tmp}/kaizen-install-tests.XXXXXX")"
	SCRATCH_DIRS+=("${INSTALL_ROOT}")
	H1="${INSTALL_ROOT}/home1"
	mkdir -p "${H1}"
	# Sentinel: user data must survive installation and removal.
	mkdir -p "${H1}/xdg-data/kaizen/app_userdata/keepme"
	printf 'user data\n' >"${H1}/xdg-data/kaizen/app_userdata/keepme/sentinel.txt"
	printf '# user shell rc\n' >"${H1}/.bashrc"
	bashrc_before="$(sha256sum "${H1}/.bashrc" | cut -d' ' -f1)"

	if install_from "${H1}" >"${INSTALL_ROOT}/install1.log" 2>&1; then
		check_ok "installer completed (user mode)"
	else
		check_fail "installer failed (user mode); log follows"
		tail -20 "${INSTALL_ROOT}/install1.log" | sed 's/^/        /'
	fi

	OPT="${H1}/.local/opt/kaizen"
	BINDIR="${H1}/.local/bin"
	XDGDATA="${H1}/xdg-data"
	INSTALLED_VERSION="${OPT}/${CHANNEL}/${PACKAGE_VERSION}"

	[[ -d "${INSTALLED_VERSION}" ]] &&
		check_ok "version installed under <opt>/<channel>/<version>" ||
		check_fail "installed version directory missing"
	[[ -f "${INSTALLED_VERSION}/.kaizen-receipt.env" ]] &&
		check_ok "installation receipt written" ||
		check_fail "installation receipt missing"
	[[ ! -e "${INSTALLED_VERSION}/Engine/Binaries/${KZ_OS_DIR}/_sc_" ]] &&
		check_ok "installed tree drops the portable marker" ||
		check_fail "installed tree still contains _sc_"
	[[ ! -e "${INSTALLED_VERSION}/ExportTemplates" && ! -e "${INSTALLED_VERSION}/Templates" ]] &&
		check_ok "installed tree does not duplicate export templates" ||
		check_fail "installed tree duplicates export templates"
	[[ -d "${XDGDATA}/kaizen/export_templates/${TEMPLATES_CONFIG}" ]] &&
		check_ok "export templates installed into XDG data" ||
		check_fail "export templates missing from XDG data"
	[[ -f "${XDGDATA}/applications/org.kaizenengine.Kaizen.desktop" ]] &&
		check_ok "desktop entry installed" ||
		check_fail "desktop entry missing"
	if grep -q "^Exec=${BINDIR}/kaizen %f$" "${XDGDATA}/applications/org.kaizenengine.Kaizen.desktop" 2>/dev/null; then
		check_ok "desktop entry uses an absolute launcher path"
	else
		check_fail "desktop entry does not use ${BINDIR}/kaizen"
	fi
	[[ -f "${XDGDATA}/icons/hicolor/256x256/apps/kaizen.png" ]] &&
		check_ok "application icon installed" ||
		check_fail "application icon missing"
	[[ -f "${XDGDATA}/mime/packages/org.kaizenengine.Kaizen.xml" ]] &&
		check_ok "MIME definition installed" ||
		check_fail "MIME definition missing"
	[[ -f "${XDGDATA}/man/man6/kaizen.6" ]] &&
		check_ok "manual page installed" ||
		check_fail "manual page missing"

	if [[ "$(run_isolated "${H1}" "${BINDIR}/kaizen" --version 2>/dev/null | head -n1)" == "${ENGINE_VERSION}" ]]; then
		check_ok "'kaizen' runs the installed engine"
	else
		check_fail "'kaizen' does not run the installed engine"
	fi
	if run_isolated "${H1}" "${BINDIR}/kaizen-select" --list >"${INSTALL_ROOT}/select.log" 2>&1 &&
		grep -q "${PACKAGE_VERSION}" "${INSTALL_ROOT}/select.log"; then
		check_ok "kaizen-select lists the installed version"
	else
		check_fail "kaizen-select did not list the installed version"
	fi
	[[ "$(cat "${OPT}/default-channel" 2>/dev/null || true)" == "${CHANNEL}" ]] &&
		check_ok "default channel persisted" ||
		check_fail "default channel was not persisted"
	[[ "$(sha256sum "${H1}/.bashrc" | cut -d' ' -f1)" == "${bashrc_before}" ]] &&
		check_ok "installer did not modify shell rc files" ||
		check_fail "installer modified shell rc files"

	# --- Coexistence: a second fixture channel -------------------------
	kz_info "Coexistence"
	FIXTURE="${INSTALL_ROOT}/fixture"
	if cp -a --reflink=auto "${DIST}" "${FIXTURE}" 2>/dev/null; then
		SECOND_CHANNEL=""
		for candidate in rc dev stable; do
			[[ "${candidate}" == "${CHANNEL}" ]] && continue
			SECOND_CHANNEL="${candidate}"
			break
		done
		if [[ "${SECOND_CHANNEL}" == "stable" ]]; then
			SECOND_VERSION="4.8.0"
		else
			SECOND_VERSION="4.8.0-${SECOND_CHANNEL}1"
		fi
		SECOND_NAME="$(kz_package_name "${SECOND_VERSION}")"
		SECOND_BASE="${SECOND_VERSION%%-*}"
		REVISION="0"
		[[ "${SECOND_VERSION}" == *-* ]] && REVISION="${SECOND_VERSION##*[a-z]}"
		sed -i \
			-e "s|^PACKAGE_NAME=.*|PACKAGE_NAME=${SECOND_NAME}|" \
			-e "s|^PACKAGE_VERSION=.*|PACKAGE_VERSION=${SECOND_VERSION}|" \
			-e "s|^PACKAGE_VERSION_BASE=.*|PACKAGE_VERSION_BASE=${SECOND_BASE}|" \
			-e "s|^CHANNEL=.*|CHANNEL=${SECOND_CHANNEL}|" \
			-e "s|^CHANNEL_REVISION=.*|CHANNEL_REVISION=${REVISION}|" \
			"${FIXTURE}/Build/Config/package-info.env"
		( cd "${FIXTURE}" && find . -type f ! -path './MANIFEST.sha256' -printf '%P\0' |
			LC_ALL=C sort -z | xargs -0 -r sha256sum ) >"${FIXTURE}/MANIFEST.sha256"

		if run_isolated "${H1}" "${FIXTURE}/Setup/install.sh" --yes --no-select >"${INSTALL_ROOT}/install2.log" 2>&1; then
			check_ok "second channel installed alongside the first"
		else
			check_fail "second channel installation failed"
			tail -20 "${INSTALL_ROOT}/install2.log" | sed 's/^/        /'
		fi
		[[ -d "${OPT}/${SECOND_CHANNEL}/${SECOND_VERSION}" ]] &&
			check_ok "both channel trees coexist" ||
			check_fail "second channel tree missing"
		[[ -L "${BINDIR}/kaizen-${SECOND_CHANNEL}" ]] &&
			check_ok "per-channel launcher created: kaizen-${SECOND_CHANNEL}" ||
			check_fail "per-channel launcher missing"
		[[ "$(cat "${OPT}/default-channel" 2>/dev/null || true)" == "${CHANNEL}" ]] &&
			check_ok "--no-select preserved the default channel" ||
			check_fail "--no-select changed the default channel"
		if run_isolated "${H1}" "${BINDIR}/kaizen-select" "${SECOND_CHANNEL}" >/dev/null 2>&1 &&
			[[ "$(run_isolated "${H1}" "${BINDIR}/kaizen" --version 2>/dev/null | head -n1)" == "${ENGINE_VERSION}" ]]; then
			check_ok "selection switches the default channel"
		else
			check_fail "selection did not switch the default channel"
		fi

		# --- Selective uninstall ---------------------------------------
		kz_info "Uninstall"
		if run_isolated "${H1}" "${OPT}/${SECOND_CHANNEL}/${SECOND_VERSION}/Setup/uninstall.sh" --yes >"${INSTALL_ROOT}/uninstall1.log" 2>&1; then
			check_ok "uninstaller completed for the second channel"
		else
			check_fail "uninstaller failed for the second channel"
			tail -20 "${INSTALL_ROOT}/uninstall1.log" | sed 's/^/        /'
		fi
		[[ ! -e "${OPT}/${SECOND_CHANNEL}" ]] &&
			check_ok "second channel removed" ||
			check_fail "second channel still present"
		[[ -d "${INSTALLED_VERSION}" ]] &&
			check_ok "first version preserved by selective uninstall" ||
			check_fail "selective uninstall removed the first version"
		[[ -d "${XDGDATA}/kaizen/export_templates/${TEMPLATES_CONFIG}" ]] &&
			check_ok "shared templates kept while a version remains" ||
			check_fail "shared templates removed too early"
		[[ "$(cat "${OPT}/default-channel" 2>/dev/null || true)" == "${CHANNEL}" ]] &&
			check_ok "default falls back to the remaining channel" ||
			check_fail "default channel not repaired after uninstall"
		[[ ! -e "${BINDIR}/kaizen-${SECOND_CHANNEL}" && -e "${BINDIR}/kaizen-${CHANNEL}" ]] &&
			check_ok "only the removed channel's launcher is gone" ||
			check_fail "launcher cleanup is incorrect"

		if run_isolated "${H1}" "${INSTALLED_VERSION}/Setup/uninstall.sh" --yes >"${INSTALL_ROOT}/uninstall2.log" 2>&1; then
			check_ok "final uninstall completed"
		else
			check_fail "final uninstall failed"
			tail -20 "${INSTALL_ROOT}/uninstall2.log" | sed 's/^/        /'
		fi
		[[ ! -e "${OPT}/${CHANNEL}" ]] &&
			check_ok "last channel removed" ||
			check_fail "last channel still present"
		[[ ! -e "${XDGDATA}/kaizen/export_templates/${TEMPLATES_CONFIG}" ]] &&
			check_ok "export templates removed with the last version" ||
			check_fail "export templates left behind"
		[[ ! -e "${BINDIR}/kaizen" && ! -e "${BINDIR}/kaizen-select" ]] &&
			check_ok "default launchers removed" ||
			check_fail "default launchers left behind"
		[[ ! -e "${XDGDATA}/applications/org.kaizenengine.Kaizen.desktop" ]] &&
			check_ok "desktop integration removed" ||
			check_fail "desktop integration left behind"
		[[ -f "${H1}/xdg-data/kaizen/app_userdata/keepme/sentinel.txt" ]] &&
			check_ok "user data preserved by uninstall" ||
			check_fail "uninstall removed user data"
	else
		check_warn "could not create a second-channel fixture; coexistence tests skipped"
	fi
fi

# --- 15. Simulated system-wide installation ----------------------------
if [[ "${GLOBAL_TEST}" == "yes" && "${INSTALL_TESTS}" == "yes" ]]; then
	kz_info "System-wide installation (simulated, no root)"
	GHOME="$(mktemp -d "${TMPDIR:-/tmp}/kaizen-global.XXXXXX")"
	SCRATCH_DIRS+=("${GHOME}")
	GOOT="${GHOME}/opt/kaizen"
	GBIN="${GHOME}/usr/local/bin"
	GSHARE="${GHOME}/usr/share"
	GDATA="${GHOME}/data"
	mkdir -p "${GHOME}/home"
	if run_isolated "${GHOME}/home" "${DIST}/Setup/install.sh" \
		--system --opt-root "${GOOT}" --bin-dir "${GBIN}" --share-dir "${GSHARE}" \
		--data-home "${GDATA}" --yes >"${GHOME}/install.log" 2>&1; then
		check_ok "system-wide installation into a simulated root completed"
	else
		check_fail "simulated system-wide installation failed"
		tail -20 "${GHOME}/install.log" | sed 's/^/        /'
	fi
	[[ -d "${GOOT}/${CHANNEL}/${PACKAGE_VERSION}" ]] &&
		check_ok "system tree uses /opt-style channels" ||
		check_fail "system tree layout is wrong"
	[[ -f "${GSHARE}/applications/org.kaizenengine.Kaizen.desktop" ]] &&
		check_ok "system desktop entry installed" ||
		check_fail "system desktop entry missing"
	[[ -x "${GBIN}/kaizen" ]] &&
		check_ok "system launcher installed" ||
		check_fail "system launcher missing"
	if run_isolated "${GHOME}/home" "${GBIN}/kaizen" --version >/dev/null 2>&1; then
		check_ok "system launcher runs"
	else
		check_fail "system launcher failed to run"
	fi
	if run_isolated "${GHOME}/home" "${DIST}/Setup/uninstall.sh" \
		--system --opt-root "${GOOT}" --bin-dir "${GBIN}" --share-dir "${GSHARE}" \
		--data-home "${GDATA}" --all --yes >"${GHOME}/uninstall.log" 2>&1; then
		check_ok "system-wide uninstallation completed"
	else
		check_fail "system-wide uninstallation failed"
		tail -20 "${GHOME}/uninstall.log" | sed 's/^/        /'
	fi
	[[ ! -e "${GOOT}/${CHANNEL}" && ! -e "${GBIN}/kaizen" ]] &&
		check_ok "system installation fully removed" ||
		check_fail "system installation left files behind"
fi

# --- Summary -------------------------------------------------------------
printf '\n%sValidation summary:%s %d passed, %d failed, %d warnings\n' "${KZ_C_INFO}" "${KZ_C_RESET}" "${PASS}" "${FAIL}" "${WARN}"
if [[ "${FAIL}" -gt 0 ]]; then
	printf '%sFailed checks:%s\n' "${KZ_C_ERR}" "${KZ_C_RESET}"
	for item in "${FAILED_ITEMS[@]}"; do
		[[ -n "${item}" ]] && printf '  - %s\n' "${item}"
	done
	exit 1
fi

kz_ok "Distribution is valid."
