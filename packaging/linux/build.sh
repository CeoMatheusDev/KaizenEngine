#!/usr/bin/env bash
#
# Kaizen Engine — Linux build driver.
#
# Wraps the engine's own SCons build system without altering it. Builds the
# editor and/or export templates, preserving previous artifacts (never cleans
# implicitly).
#
# Usage:
#   ./packaging/linux/build.sh [options]
#
# Options:
#   -t, --target LIST     editor, template_debug, template_release, all
#                         (comma separated or repeated; default: all)
#       --dev             Dev build (dev_build=yes)
#       --production      Production build (production=yes)
#       --jobs N          Parallel jobs (default: nproc)
#       --arch ARCH       Target architecture (default: x86_64)
#       --dry-run         Print commands without executing them
#   -v, --verbose         Verbose SCons output
#   -h, --help            Show this help

set -euo pipefail

# shellcheck source=../common.sh
source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/../common.sh"

usage() {
	sed -n '2,26p' "$0" | sed 's/^# \{0,1\}//'
	exit "${1:-0}"
}

# --- Defaults ------------------------------------------------------------
declare -a TARGETS=()
DEV_BUILD="no"
PRODUCTION="no"
JOBS="$(kz_nproc)"
ARCH="${KZ_ARCH}"
DRY_RUN="no"
VERBOSE="no"

parse_targets() {
	local raw="$1" item
	IFS=',' read -r -a items <<<"${raw}"
	for item in "${items[@]}"; do
		item="${item//[[:space:]]/}"
		[[ -n "${item}" ]] || continue
		case "${item}" in
		editor | template_debug | template_release | all)
			TARGETS+=("${item}")
			;;
		*)
			kz_die "Unknown target '${item}'. Use: editor, template_debug, template_release, all."
			;;
		esac
	done
}

while [[ $# -gt 0 ]]; do
	case "$1" in
	-t | --target)
		[[ $# -ge 2 ]] || kz_die "--target requires a value."
		parse_targets "$2"
		shift 2
		;;
	--dev)
		DEV_BUILD="yes"
		shift
		;;
	--production)
		PRODUCTION="yes"
		shift
		;;
	--jobs)
		[[ $# -ge 2 ]] || kz_die "--jobs requires a value."
		JOBS="$2"
		shift 2
		;;
	--arch)
		[[ $# -ge 2 ]] || kz_die "--arch requires a value."
		ARCH="$2"
		shift 2
		;;
	--dry-run)
		DRY_RUN="yes"
		shift
		;;
	-v | --verbose)
		VERBOSE="yes"
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

# --- Validation ----------------------------------------------------------
[[ ${#TARGETS[@]} -gt 0 ]] || TARGETS=("all")
kz_require_repo

if [[ "${DEV_BUILD}" == "yes" && "${PRODUCTION}" == "yes" ]]; then
	kz_die "--dev and --production are mutually exclusive."
fi

case "${ARCH}" in
x86_64) ;;
*) kz_die "Unsupported architecture '${ARCH}'. Only x86_64 is validated for Linux." ;;
esac

if [[ "${DRY_RUN}" == "no" ]]; then
	kz_need_cmd scons python3
fi

# Expand "all" preserving deterministic order.
declare -a BUILD_ORDER=()
want_editor="no"
want_release="no"
want_debug="no"
for t in "${TARGETS[@]}"; do
	case "${t}" in
	all)
		want_editor="yes"
		want_release="yes"
		want_debug="yes"
		;;
	editor) want_editor="yes" ;;
	template_release) want_release="yes" ;;
	template_debug) want_debug="yes" ;;
	esac
done
[[ "${want_editor}" == "yes" ]] && BUILD_ORDER+=("editor")
[[ "${want_release}" == "yes" ]] && BUILD_ORDER+=("template_release")
[[ "${want_debug}" == "yes" ]] && BUILD_ORDER+=("template_debug")

# --- Command helpers -----------------------------------------------------
run_cmd() {
	if [[ "${DRY_RUN}" == "yes" ]]; then
		printf '  + %s\n' "$*"
		return 0
	fi
	"$@"
}

# --- Build configuration ------------------------------------------------
declare -a SCONS_ARGS=(platform=linuxbsd "arch=${ARCH}" "-j${JOBS}")
[[ "${DEV_BUILD}" == "yes" ]] && SCONS_ARGS+=(dev_build=yes)
[[ "${PRODUCTION}" == "yes" ]] && SCONS_ARGS+=(production=yes)

kz_info "Kaizen build"
kz_info "  repository : ${KZ_REPO_ROOT}"
kz_info "  platform   : ${KZ_PLATFORM}/${ARCH}"
kz_info "  targets    : ${BUILD_ORDER[*]}"
kz_info "  dev build  : ${DEV_BUILD}"
kz_info "  production : ${PRODUCTION}"
kz_info "  jobs       : ${JOBS}"

# --- Build targets -------------------------------------------------------
declare -a BUILT_FILES=()

for target in "${BUILD_ORDER[@]}"; do
	kz_info "Building ${target} ..."
	cmd=(scons "${SCONS_ARGS[@]}" "target=${target}")
	[[ "${VERBOSE}" == "yes" ]] && cmd+=(verbose=yes)
	run_cmd "${cmd[@]}"

	if [[ "${DRY_RUN}" == "no" ]]; then
		if ! binary="$(kz_resolve_binary "${target}")"; then
			kz_die "Build of ${target} finished but no binary was found in ${KZ_REPO_ROOT}/bin. Expected: $(kz_binary_name "${target}")"
		fi
		BUILT_FILES+=("${binary}")
		kz_ok "Built: $(realpath --relative-to="${KZ_REPO_ROOT}" "${binary}")"
	fi
done

# --- Summary ------------------------------------------------------------
if [[ "${DRY_RUN}" == "no" ]]; then
	kz_info "Build complete. Artifacts:"
	for f in "${BUILT_FILES[@]}"; do
		printf '  %-72s %s\n' "$(basename "${f}")" "$(du -h "${f}" | cut -f1)"
	done
	kz_info "Next: ./packaging/linux/package.sh"
fi
