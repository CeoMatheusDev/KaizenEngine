#!/usr/bin/env python3
"""
NVIDIA SDK and PhysX SDK Management Tool for KaizenEngine.

Handles local SDK acquisition, extraction, validation, and installation.
Never downloads dependencies during normal compilation - this tool is run
manually by developers before building.

Supports:
- NVIDIA DLSS SDK (manual download required)
- NVIDIA Streamline SDK (GitHub available)
- NVIDIA Reflex SDK
- NVIDIA NGX SDK (manual download required)
- NVIDIA PhysX SDK (GitHub, BSD-3-Clause, buildable from source)

Usage:
    python tools/sdk/sdk_manager.py --list
    python tools/sdk/sdk_manager.py --download nvidia/dlss
    python tools/sdk/sdk_manager.py --install nvidia/dlss /path/to/downloaded.zip
    python tools/sdk/sdk_manager.py --validate all
    python tools/sdk/sdk_manager.py --status
"""

import argparse
import hashlib
import json
import os
import shutil
import subprocess
import sys
import zipfile
import tarfile
from pathlib import Path
from typing import Any, Dict, List, Optional, Tuple

# Project root is two levels up from tools/sdk/
PROJECT_ROOT = Path(__file__).resolve().parent.parent.parent
MANIFEST_DIR = PROJECT_ROOT / "tools" / "sdk" / "manifest"

# Color output (disabled when not a TTY)
def _color(text: str, code: str) -> str:
    if sys.stdout.isatty():
        return f"\x1b[{code}m{text}\x1b[0m"
    return text

RED = lambda t: _color(t, "31")
GREEN = lambda t: _color(t, "32")
YELLOW = lambda t: _color(t, "33")
BLUE = lambda t: _color(t, "34")
BOLD = lambda t: _color(t, "1")


class SDKError(Exception):
    pass


class Manifest:
    """Loads and provides access to SDK manifests."""

    def __init__(self, manifest_dir: Path):
        self.manifest_dir = manifest_dir
        self._sdk_groups: Dict[str, Dict[str, Any]] = {}

    def load(self, name: str) -> Dict[str, Any]:
        # Try both exact name and name with _sdks suffix
        path = self.manifest_dir / f"{name}.json"
        if not path.exists():
            path = self.manifest_dir / f"{name}_sdks.json"
        if not path.exists():
            raise SDKError(f"Manifest not found: {self.manifest_dir / name}.json")
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
        self._sdk_groups[data["sdk_group"]] = data
        return data

    def get_sdk(self, sdk_key: str) -> Tuple[Dict[str, Any], str]:
        """Returns (sdk_config, group_name) for a key like 'nvidia/dlss' or 'physx/physx'."""
        parts = sdk_key.split("/", 1)
        if len(parts) != 2:
            raise SDKError(f"Invalid SDK key: {sdk_key}. Use format 'group/sdk'.")
        group_name, sdk_name = parts
        if group_name not in self._sdk_groups:
            self.load(group_name)
        group = self._sdk_groups[group_name]
        sdks = group.get("sdks", {})
        if sdk_name not in sdks:
            available = ", ".join(sdks.keys())
            raise SDKError(f"SDK '{sdk_name}' not found in group '{group_name}'. Available: {available}")
        return sdks[sdk_name], group_name

    def list_all(self) -> List[Tuple[str, str, str]]:
        """Returns list of (key, name, status_description) for all SDKs."""
        results = []
        for manifest_file in sorted(self.manifest_dir.glob("*_sdks.json")):
            with open(manifest_file, "r", encoding="utf-8") as f:
                data = json.load(f)
            group = data["sdk_group"]
            self._sdk_groups[group] = data
            for sdk_name, sdk_info in data.get("sdks", {}).items():
                key = f"{group}/{sdk_name}"
                results.append((key, sdk_info.get("name", sdk_name), sdk_info.get("acquisition", "unknown")))
        return results


class SDKManager:
    """Manages SDK download, installation, and validation."""

    def __init__(self):
        self.project_root = PROJECT_ROOT
        self.thirdparty_dir = self.project_root / "thirdparty"
        self.manifest = Manifest(MANIFEST_DIR)

    def get_sdk_dir(self, sdk_key: str) -> Path:
        """Returns the target directory for an SDK."""
        sdk_info, group_name = self.manifest.get_sdk(sdk_key)
        group_manifest = self.manifest._sdk_groups.get(group_name)
        if group_manifest is None:
            group_manifest = self.manifest.load(group_name)
        sdk_root = group_manifest.get("sdk_root", f"thirdparty/{group_name}")

        # Handle bundled SDKs that share a parent directory
        acquisition = sdk_info.get("acquisition", "")
        if acquisition == "bundled_with_dlss":
            return self.project_root / sdk_root / "dlss"
        elif acquisition == "bundled_with_streamline":
            return self.project_root / sdk_root / "streamline"
        elif acquisition == "bundled_with_physx":
            return self.project_root / sdk_root / "physx" / "physx"
        elif acquisition.startswith("bundled_with"):
            return self.project_root / sdk_root / "physx" / "physx"

        return self.project_root / sdk_root / sdk_key.split("/", 1)[1]

    def is_installed(self, sdk_key: str) -> bool:
        """Check if an SDK appears to be installed."""
        sdk_dir = self.get_sdk_dir(sdk_key)
        sdk_info, _ = self.manifest.get_sdk(sdk_key)

        expected = sdk_info.get("expected_structure", {})
        include_dirs = expected.get("include", [])
        lib_dirs = expected.get("lib", [])

        # Check for at least one include file and one lib if specified
        if include_dirs:
            for inc in include_dirs:
                if not (sdk_dir / inc).exists() and not (sdk_dir / "include" / inc).exists():
                    return False

        if lib_dirs:
            for lib in lib_dirs:
                if not (sdk_dir / lib).exists() and not (sdk_dir / "lib" / lib).exists():
                    return False

        # Check source directories for source-built SDKs
        source_dirs = expected.get("source", [])
        if source_dirs and not include_dirs and not lib_dirs:
            for src in source_dirs:
                if not (sdk_dir / src).exists():
                    return False

        return sdk_dir.exists() and any(sdk_dir.iterdir())

    def sha256_file(self, path: Path) -> str:
        """Compute SHA-256 hash of a file."""
        h = hashlib.sha256()
        with open(path, "rb") as f:
            for chunk in iter(lambda: f.read(8192), b""):
                h.update(chunk)
        return h.hexdigest()

    def validate_installation(self, sdk_key: str) -> Tuple[bool, List[str]]:
        """Validate an installed SDK. Returns (is_valid, list_of_issues)."""
        issues: List[str] = []
        sdk_dir = self.get_sdk_dir(sdk_key)
        sdk_info, group_name = self.manifest.get_sdk(sdk_key)

        if not sdk_dir.exists():
            return False, [f"SDK directory does not exist: {sdk_dir}"]

        if not any(sdk_dir.iterdir()):
            return False, [f"SDK directory is empty: {sdk_dir}"]

        expected = sdk_info.get("expected_structure", {})

        # Check include files (try both with and without include/ prefix)
        for inc_path in expected.get("include", []):
            full_path = sdk_dir / inc_path
            if not full_path.exists():
                full_path = sdk_dir / "include" / inc_path
            if not full_path.exists():
                issues.append(f"Missing include file: {inc_path}")

        # Check lib files (try both with and without lib/ prefix)
        for lib_path in expected.get("lib", []):
            full_path = sdk_dir / lib_path
            if not full_path.exists():
                full_path = sdk_dir / "lib" / lib_path
            if not full_path.exists():
                issues.append(f"Missing library file: {lib_path}")

        # Check source directories for source-built SDKs
        for src_path in expected.get("source", []):
            full_path = sdk_dir / src_path
            if not full_path.exists():
                issues.append(f"Missing source directory: {src_path}")

        return len(issues) == 0, issues

    def list_sdks(self):
        """List all known SDKs with their status."""
        print(f"\n{BOLD('NVIDIA & PhysX SDK Status')}\n")
        print(f"{'SDK Key':<30} {'Name':<45} {'Status':<15}")
        print("-" * 90)

        for key, name, acquisition in self.manifest.list_all():
            installed = self.is_installed(key)
            valid = False
            if installed:
                valid, _ = self.validate_installation(key)

            if valid:
                status = GREEN("installed")
            elif installed:
                status = YELLOW("partial")
            else:
            # Not installed
                status = RED("missing")

            print(f"{key:<30} {name:<45} {status:<15}")

        print()

    def install_from_archive(self, sdk_key: str, archive_path: Path) -> bool:
        """Install an SDK from a downloaded archive file."""
        if not archive_path.exists():
            print(RED(f"Archive not found: {archive_path}"))
            return False

        sdk_info, group_name = self.manifest.get_sdk(sdk_key)
        sdk_dir = self.get_sdk_dir(sdk_key)

        # Prevent accidental replacement of working SDK versions
        if self.is_installed(sdk_key):
            valid, issues = self.validate_installation(sdk_key)
            if valid:
                print(YELLOW(f"SDK {sdk_key} is already installed and valid. Remove it first to reinstall."))
                return False

        sdk_dir.mkdir(parents=True, exist_ok=True)

        try:
            if archive_path.suffix.lower() == ".zip":
                with zipfile.ZipFile(archive_path, "r") as zf:
                    zf.extractall(sdk_dir)
            elif archive_path.suffix.lower() in (".tar.gz", ".tgz"):
                with tarfile.open(archive_path, "r:gz") as tf:
                    tf.extractall(sdk_dir)
            elif archive_path.suffix.lower() == ".tar":
                with tarfile.open(archive_path, "r:") as tf:
                    tf.extractall(sdk_dir)
            else:
                print(RED(f"Unsupported archive format: {archive_path.suffix}"))
                return False

            print(GREEN(f"Extracted {sdk_key} to {sdk_dir}"))

            # Validate after extraction
            valid, issues = self.validate_installation(sdk_key)
            if valid:
                print(GREEN(f"SDK {sdk_key} installed and validated successfully."))
                return True
            else:
                print(YELLOW(f"SDK {sdk_key} installed but validation found issues:"))
                for issue in issues:
                    print(f"  - {issue}")
                return True  # Still return True; some SDKs have optional files

        except Exception as e:
            print(RED(f"Failed to install {sdk_key}: {e}"))
            return False

    def download_instructions(self, sdk_key: str):
        """Print manual download instructions for SDKs that require them."""
        sdk_info, group_name = self.manifest.get_sdk(sdk_key)

        print(f"\n{BOLD(f'Manual Download Instructions: {sdk_info.get("name", sdk_key)}')}\n")
        print(f"Acquisition: {sdk_info.get('acquisition', 'unknown')}")
        print(f"License: {sdk_info.get('license', 'N/A')}")
        print(f"Redistribution: {sdk_info.get('redistribution', 'N/A')}")
        print(f"Developer account required: {sdk_info.get('developer_account_required', False)}")
        print(f"\nDownload URL: {BLUE(sdk_info.get('acquisition_url', 'N/A'))}")
        print(f"\nCurrent version: {sdk_info.get('current_version', 'N/A')}")
        print(f"Minimum version: {sdk_info.get('min_version', 'N/A')}")
        print(f"\nExpected structure after extraction:")
        sdk_dir = self.get_sdk_dir(sdk_key)
        expected = sdk_info.get("expected_structure", {})
        for inc in expected.get("include", []):
            print(f"  {sdk_dir / 'include' / inc}")
        for lib in expected.get("lib", []):
            print(f"  {sdk_dir / lib}")
        print(f"\nAfter downloading, run:")
        print(f"  python tools/sdk/sdk_manager.py --install {sdk_key} <path-to-archive>")
        print(f"\n{BOLD('WARNING:')} Do not commit proprietary NVIDIA binaries to version control.")
        print(f"Check NVIDIA's license terms before redistributing.\n")


def main():
    parser = argparse.ArgumentParser(
        description="KaizenEngine NVIDIA & PhysX SDK Manager"
    )
    parser.add_argument("--list", action="store_true", help="List all SDKs and their status")
    parser.add_argument("--status", action="store_true", help="Same as --list")
    parser.add_argument("--validate", metavar="SDK_KEY", help="Validate an SDK installation (use 'all' for all)")
    parser.add_argument("--install", metavar="SDK_KEY", help="Install SDK from archive (requires --archive)")
    parser.add_argument("--archive", metavar="PATH", help="Path to archive file for --install")
    parser.add_argument("--instructions", metavar="SDK_KEY", help="Show manual download instructions")
    parser.add_argument("--download", metavar="SDK_KEY", help="Attempt automatic download (GitHub-hosted SDKs only)")

    args = parser.parse_args()

    manager = SDKManager()

    if args.list or args.status:
        manager.list_sdks()
    elif args.validate:
        if args.validate == "all":
            for key, _, _ in manager.manifest.list_all():
                valid, issues = manager.validate_installation(key)
                status = GREEN("VALID") if valid else RED("INVALID")
                print(f"{key}: {status}")
                for issue in issues:
                    print(f"  - {issue}")
        else:
            valid, issues = manager.validate_installation(args.validate)
            status = GREEN("VALID") if valid else RED("INVALID")
            print(f"{args.validate}: {status}")
            for issue in issues:
                print(f"  - {issue}")
    elif args.instructions:
        manager.download_instructions(args.instructions)
    elif args.install:
        if not args.archive:
            print(RED("--install requires --archive"))
            sys.exit(1)
        manager.install_from_archive(args.install, Path(args.archive))
    elif args.download:
        print(YELLOW(f"Automatic download not yet implemented for {args.download}"))
        manager.download_instructions(args.download)
    else:
        parser.print_help()


if __name__ == "__main__":
    main()
