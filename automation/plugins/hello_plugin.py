"""
ARQYON Plugin — hello_plugin.py
A minimal example plugin to verify the Python automation layer works.
Run with: arqyon run automation/plugins/hello_plugin.py
"""

import sys
import platform
import datetime

def main():
    print("=" * 50)
    print("  ARQYON Python Plugin — hello_plugin")
    print("=" * 50)
    print(f"  Python  : {sys.version.split()[0]}")
    print(f"  Platform: {platform.system()} {platform.release()}")
    print(f"  Time    : {datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print("=" * 50)
    print("  Plugin ran successfully.")

if __name__ == "__main__":
    main()
