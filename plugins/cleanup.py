"""
cleanup.py — ARQYON Automation Plugin

Finds and reports (or deletes) common junk files:
  __pycache__, *.pyc, .DS_Store, Thumbs.db, *.tmp, *.log

Usage:
  arqyon run cleanup [DIR] [--delete]

Args:
  DIR       Directory to scan (default: current directory)
  --delete  Actually delete the found files (default: dry-run)
"""

import sys
import os
from pathlib import Path

JUNK_PATTERNS = {
    "names":    {"__pycache__", ".DS_Store", "Thumbs.db", "desktop.ini"},
    "suffixes": {".pyc", ".pyo", ".tmp", ".log", ".bak"},
}


def find_junk(root: Path):
    found = []
    for item in root.rglob("*"):
        if item.name in JUNK_PATTERNS["names"]:
            found.append(item)
        elif item.suffix in JUNK_PATTERNS["suffixes"]:
            found.append(item)
    return found


def main():
    args = sys.argv[1:]
    delete = "--delete" in args
    dirs = [a for a in args if not a.startswith("--")]
    root = Path(dirs[0]) if dirs else Path.cwd()

    if not root.exists():
        print(f"Error: directory not found: {root}", file=sys.stderr)
        return 1

    print(f"Scanning: {root}")
    print(f"Mode    : {'DELETE' if delete else 'DRY RUN'}")
    print("-" * 50)

    items = find_junk(root)

    if not items:
        print("  No junk files found. All clean!")
        return 0

    total_size = 0
    for item in items:
        try:
            size = item.stat().st_size if item.is_file() else 0
            total_size += size
            tag = "[DEL]" if delete else "[dry]"
            print(f"  {tag} {item}  ({size:,} bytes)" if item.is_file() else f"  {tag} {item}/")
            if delete:
                if item.is_file():
                    item.unlink()
                elif item.is_dir():
                    import shutil
                    shutil.rmtree(item)
        except Exception as e:
            print(f"  [err] {item}: {e}", file=sys.stderr)

    print("-" * 50)
    action = "Deleted" if delete else "Found"
    print(f"  {action} {len(items)} item(s)  ({total_size:,} bytes)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
