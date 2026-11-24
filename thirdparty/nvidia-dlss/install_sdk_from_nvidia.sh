#!/bin/bash
# NVIDIA DLSS SDK installation helper.
#
# SDK acquisition is handled by tools/sdk/sdk_manager.py, which knows the
# expected layout (thirdparty/nvidia/dlss/) and the required headers. This
# script is a convenience wrapper kept for discoverability.
set -e

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

echo "=== NVIDIA DLSS SDK Installation ==="
echo ""
echo "Option 1 - automatic (public GitHub mirror):"
echo "  python3 tools/sdk/sdk_manager.py --download nvidia/dlss"
echo ""
echo "Option 2 - manual download (NVIDIA Developer Portal):"
python3 "${REPO_ROOT}/tools/sdk/sdk_manager.py" --instructions nvidia/dlss
echo ""
echo "Validate after installation:"
echo "  python3 tools/sdk/sdk_manager.py --validate nvidia/dlss"
