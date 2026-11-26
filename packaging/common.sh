#!/usr/bin/env bash
# Kaizen Engine — shared helpers for packaging scripts (Linux).
# This file is meant to be sourced, not executed directly.

# --- Paths ---------------------------------------------------------------
KZ_PACKAGING_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
KZ_REPO_ROOT="$(cd "${KZ_PACKAGING_DIR}/.." && pwd)"

# Default distribution tree landed by package.sh.
KZ_DIST_DIR="${KZ_DIST_DIR:-${KZ_REPO_ROOT}/dist/Kaizen}"

# Engine platform/arch identifiers as used by SCons and binary names.
KZ_PLATFORM="linuxbsd"
KZ_OS_DIR="Linux"
KZ_ARCH="x86_64"

# --- Output helpers ------------------------------------------------------
if [[ -t 1 ]]; then
	KZ_C_RESET=$'\033[0m'
	KZ_C_INFO=$'\033[1;34m'
	KZ_C_OK=$'\033[1;32m'
	KZ_C_WARN=$'\033[1;33m'
	KZ_C_ERR=$'\033[1;31m'
else
	KZ_C_RESET=""
	KZ_C_INFO=""
	KZ_C_OK=""
	KZ_C_WARN=""
	KZ_C_ERR=""
fi

kz_info() { printf '%s[kz]%s %s\n' "${KZ_C_INFO}" "${KZ_C_RESET}" "$*"; }
kz_ok() { printf '%s[ ok ]%s %s\n' "${KZ_C_OK}" "${KZ_C_RESET}" "$*"; }
kz_warn() { printf '%s[warn]%s %s\n' "${KZ_C_WARN}" "${KZ_C_RESET}" "$*" >&2; }
kz_err() { printf '%s[fail]%s %s\n' "${KZ_C_ERR}" "${KZ_C_RESET}" "$*" >&2; }

kz_die() {
	kz_err "$*"
	exit 1
}

# --- Environment checks --------------------------------------------------
kz_need_cmd() {
	local cmd
	for cmd in "$@"; do
		command -v "${cmd}" >/dev/null 2>&1 || kz_die "Required command not found: ${cmd}"
	done
}

kz_nproc() {
	nproc 2>/dev/null || echo 4
}

# --- Binary discovery ----------------------------------------------------
# kz_binary_name <target>
# Mirrors the SCons naming: kaizen.<platform>.<target>.<arch>
kz_binary_name() {
	local target="$1"
	printf 'kaizen.%s.%s.%s' "${KZ_PLATFORM}" "${target}" "${KZ_ARCH}"
}

# kz_binary_path <target> [bin_dir]
kz_binary_path() {
	local target="$1" bin_dir="${2:-${KZ_REPO_ROOT}/bin}"
	printf '%s/%s' "${bin_dir}" "$(kz_binary_name "${target}")"
}

# kz_resolve_binary <target> [bin_dir]
# Returns the binary the build produced, or fails when it is not there.
kz_resolve_binary() {
	local target="$1" bin_dir="${2:-${KZ_REPO_ROOT}/bin}"
	local exact
	exact="$(kz_binary_path "${target}" "${bin_dir}")"
	[[ -f "${exact}" ]] || return 1
	printf '%s' "${exact}"
}

# --- Version helpers -----------------------------------------------------
# kz_version_full_build <binary>
# Returns the engine's full build string, e.g. "4.8.beta.custom_build".
kz_version_full_build() {
	local binary="$1" out
	[[ -x "${binary}" ]] || return 1
	out="$("${binary}" --version 2>/dev/null | head -n1 | tr -d '\r' | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')"
	[[ -n "${out}" ]] || return 1
	printf '%s' "${out}"
}

# kz_version_full_config <full_build>
# Strips the build tag ("custom_build", "official", ...) to obtain the value of
# KAIZEN_VERSION_FULL_CONFIG, which is the export-templates directory name.
# Example: "4.8.beta.custom_build" -> "4.8.beta"
kz_version_full_config() {
	local full_build="$1"
	case "${full_build}" in
	*.custom_build) printf '%s' "${full_build%.custom_build}" ;;
	*.official) printf '%s' "${full_build%.official}" ;;
	*.dev) printf '%s' "${full_build%.dev}" ;;
	*) printf '%s' "${full_build}" ;;
	esac
}

# --- Release metadata ----------------------------------------------------
# Version/channel are explicit packaging metadata. They are never derived from
# directory names; callers either pass them or read them from version.py.
kz_channel_ok() {
	case "$1" in
	stable | beta | rc | dev) return 0 ;;
	*) return 1 ;;
	esac
}

# kz_status_to_channel <version.py status>
kz_status_to_channel() {
	case "$1" in
	stable | release | official) printf 'stable' ;;
	beta | alpha) printf 'beta' ;;
	rc) printf 'rc' ;;
	dev | development | nightly) printf 'dev' ;;
	*) printf '' ;;
	esac
}

kz_semver_ok() {
	[[ "$1" =~ ^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$ ]]
}

# kz_package_version <X.Y.Z> <channel> <revision>
kz_package_version() {
	local base="$1" channel="$2" revision="$3"
	if [[ "${channel}" == "stable" ]]; then
		printf '%s' "${base}"
	else
		printf '%s-%s%s' "${base}" "${channel}" "${revision}"
	fi
}

# kz_package_name <version> [arch]
kz_package_name() {
	local version="$1" arch="${2:-${KZ_ARCH}}"
	printf 'kaizen-%s-linux-%s' "${arch}" "${version}"
}

# --- Reproducibility -----------------------------------------------------
# kz_epoch [reproducible:yes|no]
# Prints the POSIX timestamp used for archive mtimes and generated_at.
kz_epoch() {
	local reproducible="${1:-no}"
	if [[ -n "${SOURCE_DATE_EPOCH:-}" ]]; then
		printf '%s' "${SOURCE_DATE_EPOCH}"
	elif [[ "${reproducible}" == "yes" ]]; then
		# 2000-01-01T00:00:00Z: stable, valid for tar and human-readable enough.
		printf '946684800'
	else
		date +%s
	fi
}

# --- Small utilities -----------------------------------------------------
kz_sha256() {
	if command -v sha256sum >/dev/null 2>&1; then
		sha256sum "$1" | awk '{print $1}'
	else
		shasum -a 256 "$1" | awk '{print $1}'
	fi
}

kz_require_repo() {
	[[ -f "${KZ_REPO_ROOT}/SConstruct" ]] || kz_die "Not inside the Kaizen source repository (SConstruct not found): ${KZ_REPO_ROOT}"
	[[ -f "${KZ_REPO_ROOT}/version.py" ]] || kz_die "version.py not found in ${KZ_REPO_ROOT}"
}

kz_confirm_or_die() {
	local prompt="$1"
	if [[ "${KZ_ASSUME_YES:-no}" == "yes" ]]; then
		return 0
	fi
	local answer
	printf '%s [y/N] ' "${prompt}"
	read -r answer
	[[ "${answer}" == "y" || "${answer}" == "Y" ]] || kz_die "Aborted by user."
}
