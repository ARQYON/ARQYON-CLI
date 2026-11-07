"""
ARQYON Python Plugin API Contract
==================================
Every ARQYON plugin should follow this interface.
Import this module in your plugin to access helper utilities.

Usage in a plugin:
    from runner_api import ArqyonPlugin, log, success, warn, fail
"""

import sys
import os
import json
import datetime


# ── Logging helpers ──────────────────────────────────────────

def log(msg: str):
    print(f"  · {msg}")

def success(msg: str):
    print(f"  ✔ {msg}")

def warn(msg: str):
    print(f"  ⚠ {msg}", file=sys.stderr)

def fail(msg: str, code: int = 1):
    print(f"  ✘ {msg}", file=sys.stderr)
    sys.exit(code)


# ── Plugin base class ────────────────────────────────────────

class ArqyonPlugin:
    """
    Base class for ARQYON plugins.
    Subclass this and implement run().

    Example:
        class MyPlugin(ArqyonPlugin):
            name = "my-plugin"
            version = "1.0.0"
            description = "Does something useful."

            def run(self, args):
                log("Running...")
                success("Done.")

        if __name__ == "__main__":
            MyPlugin().execute()
    """

    name: str        = "unnamed-plugin"
    version: str     = "0.0.1"
    description: str = ""

    def run(self, args: list[str]) -> int:
        """Override this. Return 0 for success, non-zero for failure."""
        raise NotImplementedError("Plugin must implement run()")

    def execute(self):
        """Entry point called by arqyon run."""
        args = sys.argv[1:]
        log(f"Plugin  : {self.name} v{self.version}")
        if self.description:
            log(f"Info    : {self.description}")
        log(f"Args    : {args or '(none)'}")
        print()

        try:
            code = self.run(args)
            sys.exit(code if isinstance(code, int) else 0)
        except Exception as e:
            fail(f"Plugin raised an exception: {e}")


# ── Utilities ────────────────────────────────────────────────

def read_json(path: str) -> dict:
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)

def write_json(path: str, data: dict, indent: int = 2):
    with open(path, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=indent)
    success(f"Written: {path}")

def timestamp() -> str:
    return datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
