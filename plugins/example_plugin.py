"""
example_plugin.py — ARQYON Plugin Example

Every ARQYON plugin is a plain Python script.
  • Receives CLI args via sys.argv[1:]
  • Prints output to stdout
  • Errors to stderr
  • Exits with 0 on success, non-zero on failure

Run with:  arqyon run example_plugin
"""

import sys
import platform
import os
from datetime import datetime


def main():
    args = sys.argv[1:]
    print("=" * 50)
    print("  ARQYON Example Plugin")
    print("=" * 50)
    print(f"  Timestamp : {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print(f"  Platform  : {platform.system()} {platform.release()}")
    print(f"  Python    : {platform.python_version()}")
    print(f"  CWD       : {os.getcwd()}")
    if args:
        print(f"  Args      : {', '.join(args)}")
    print("=" * 50)
    return 0


if __name__ == "__main__":
    sys.exit(main())
