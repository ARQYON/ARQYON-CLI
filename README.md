# ARQYON CLI

```
  █████╗ ██████╗  ██████╗ ██╗   ██╗ ██████╗ ███╗   ██╗
 ██╔══██╗██╔══██╗██╔═══██╗╚██╗ ██╔╝██╔═══██╗████╗  ██║
 ███████║██████╔╝██║   ██║ ╚████╔╝ ██║   ██║██╔██╗ ██║
 ██╔══██║██╔══██╗██║▄▄ ██║  ╚██╔╝  ██║   ██║██║╚██╗██║
 ██║  ██║██║  ██║╚██████╔╝   ██║   ╚██████╔╝██║ ╚████║
 ╚═╝  ╚═╝╚═╝  ╚═╝ ╚══▀▀═╝   ╚═╝    ╚═════╝ ╚═╝  ╚═══╝
```

> **Think Beyond. Build Without Limits.**

A unified, polyglot command-line toolkit. Fast system inspection, developer
environment detection, file utilities, diagnostics, and Python-powered
automation — all in one binary.

---

## Architecture

```
┌─────────────────────────────────────────────┐
│          C# Shell  (arqyon.exe)             │  ← User-facing CLI
│   Command routing · Output formatting       │
└────────────────┬────────────────────────────┘
                 │  P/Invoke (Native DLL)
┌────────────────▼────────────────────────────┐
│       C++ Core  (arqyon_core.dll)           │  ← Speed + system access
│  System · Dev · File · Diagnostics          │
└────────────────┬────────────────────────────┘
                 │  Python C API / shell-out
┌────────────────▼────────────────────────────┐
│        Python Layer  (automation/)          │  ← Extensibility
│   Plugins · Automation · runner_api         │
└─────────────────────────────────────────────┘
```

**Why three languages?**

| Layer | Language | Reason |
|---|---|---|
| Core | C++ | Windows API, hashing, raw speed |
| Shell | C# | Cross-platform CLI UX, .NET ecosystem |
| Automation | Python | Scriptability, extensibility, rapid plugins |

**Inter-layer communication:** C++ compiles to `arqyon_core.dll`. C# calls it
via P/Invoke (`[DllImport]`). Every `char*` returned from C++ is freed with
`FreeCoreString()` — the memory contract is enforced in `ArqyonCore.cs`.

---

## Project Structure

```
arqyon/
├── core/                        C++ DLL (arqyon_core)
│   ├── include/
│   │   ├── arqyon_exports.h     DLL contract — all exported symbols
│   │   ├── system_info.h
│   │   ├── dev_tools.h
│   │   ├── file_utils.h
│   │   ├── diagnostics.h
│   │   └── py_runner.h
│   ├── src/
│   │   ├── exports.cpp          DLL entry + all C exports
│   │   ├── system_info.cpp      CPU · RAM · OS · Disk (Windows API)
│   │   ├── dev_tools.cpp        Language detection · PATH scan
│   │   ├── file_utils.cpp       SHA-256 · MD5 · file metadata (CNG)
│   │   ├── diagnostics.cpp      Full diagnostic report
│   │   └── py_runner.cpp        Embedded Python runner
│   └── CMakeLists.txt
│
├── shell/                       C# CLI shell
│   ├── Commands/
│   │   ├── SystemCommand.cs     arqyon system
│   │   ├── DevCommand.cs        arqyon dev
│   │   ├── FileCommand.cs       arqyon file
│   │   ├── DiagCommand.cs       arqyon diag
│   │   └── RunCommand.cs        arqyon run / plugin
│   ├── Interop/
│   │   └── ArqyonCore.cs        P/Invoke bridge (all DllImport)
│   ├── Output/
│   │   ├── Printer.cs           Tables · colors · banners
│   │   └── JsonExporter.cs      JSON / HTML report export
│   ├── Program.cs               Entry point + command router
│   └── ArqyonShell.csproj
│
├── automation/                  Python layer
│   ├── plugins/
│   │   └── hello_plugin.py      Example plugin
│   └── runner_api.py            Plugin base class + helpers
│
├── CMakeLists.txt               Root CMake
├── build.ps1                    One-shot build script (PowerShell)
└── README.md
```

---

## Prerequisites

| Tool | Minimum Version | Purpose |
|---|---|---|
| Visual Studio / MSVC | 2022 | Compile C++ core |
| CMake | 3.20 | C++ build system |
| .NET SDK | 8.0 | Build C# shell |
| Python 3 | 3.8+ | Automation layer (optional for embedding) |

---

## Building

### Quick build (PowerShell)

```powershell
.\build.ps1
# or for Debug:
.\build.ps1 -Config Debug
```

This will:
1. Run CMake to configure and build `arqyon_core.dll`
2. Run `dotnet build` on the C# shell
3. Copy the DLL into the shell's output directory

### Manual build

**Step 1 — C++ core:**
```powershell
cmake -S . -B build
cmake --build build --config Release
```

**Step 2 — C# shell:**
```powershell
cd shell
dotnet build -c Release
```

**Step 3 — Copy DLL:**
```powershell
copy build\bin\Release\arqyon_core.dll shell\bin\Release\net8.0\win-x64\
```

### Optional: Embedded Python

To enable the Python C API (faster script execution, no subprocess):

1. Install Python 3 with the development headers.
2. In `core/CMakeLists.txt`, uncomment the `find_package(Python3 ...)` block.
3. Rebuild the core.

Without this, `arqyon run` falls back to shelling out to the system `python`.

---

## Usage

```powershell
arqyon --help
arqyon --version

# System information
arqyon system              # All: OS + CPU + RAM + Disk
arqyon system cpu          # CPU only
arqyon system ram          # Memory only
arqyon system os           # OS details only
arqyon system disk         # Storage drives

# Developer environment
arqyon dev langs           # Detect installed runtimes (Node, Python, Go…)
arqyon dev env             # Inspect PATH entries
arqyon dev doctor          # Health check for essential tools

# File utilities
arqyon file hash <path>           # SHA-256 hash
arqyon file hash <path> --md5     # MD5 hash
arqyon file info <path>           # Metadata: size, timestamps, attributes

# Diagnostics
arqyon diag                       # Full report (inline)
arqyon diag --export              # Save as JSON
arqyon diag --export --html       # Save as HTML report
arqyon diag --export --out report.json   # Custom output path

# Python automation
arqyon run <script.py>            # Run a Python script
arqyon run <script.py> arg1 arg2  # With arguments
arqyon plugin list                # List installed plugins
```

---

## Writing a Plugin

Create a `.py` file in `automation/plugins/`:

```python
# automation/plugins/my_plugin.py
import sys
sys.path.insert(0, "automation")   # so runner_api is importable
from runner_api import ArqyonPlugin, log, success

class MyPlugin(ArqyonPlugin):
    name        = "my-plugin"
    version     = "1.0.0"
    description = "Does something useful."

    def run(self, args):
        log("Starting...")
        # your logic here
        success("Done.")
        return 0

if __name__ == "__main__":
    MyPlugin().execute()
```

Run it:
```powershell
arqyon run automation/plugins/my_plugin.py
```

---

## Memory Contract (C++ ↔ C#)

Every function in `arqyon_exports.h` that returns `char*` heap-allocates the
string with `new char[]`. The C# side **must** free it via `FreeCoreString()`.
This is handled transparently in `ArqyonCore.cs` — the `MarshalAndFree()`
helper converts to a managed `string` and immediately frees the native pointer.
Consumers never touch raw `IntPtr`.

```
C++ → char* (new[])
       ↓
C# → MarshalAndFree(ptr)
       ├── Marshal.PtrToStringAnsi(ptr)   → managed string
       └── FreeCoreString(ptr)            → delete[] ptr
```

---

## Extending ARQYON

### Add a new C++ module

1. Create `core/include/my_module.h` and `core/src/my_module.cpp`
2. Add exported functions to `core/include/arqyon_exports.h`
3. Implement the exports in `core/src/exports.cpp`
4. Add P/Invoke declarations in `shell/Interop/ArqyonCore.cs`
5. Add a command class in `shell/Commands/MyCommand.cs`
6. Register the command in `shell/Program.cs`

### Add a new CLI command

1. Create `shell/Commands/MyCommand.cs` following the pattern of existing commands
2. Add the case to the `switch` in `Program.cs`
3. Add a help entry in `ShowHelp()`

---

## License

MIT License — Copyright © 2025 ARQYON

---

*ARQYON — Think Beyond. Build Without Limits.*
