# ARQYON CLI
**A C++ Powered Developer & System Toolkit with C# Host and Python Extensibility**

> Think Beyond. Build Without Limits. — ARQYON

---

## Architecture

```
arqyon.exe  (C# .NET 10 — host, TUI, command routing)
     │
     ├── P/Invoke ──► ArqyonNative.dll  (C++17 — OS, hashing, detection)
     │
     └── Subprocess ──► plugins/*.py    (Python 3 — automation scripts)
```

| Layer | Language | Responsibility |
|---|---|---|
| Host | C# .NET 10 | Entry point, Spectre.Console TUI, command dispatch |
| Native | C++17 (DLL) | Windows API, system info, file hashing, diagnostics |
| Plugins | Python 3 | User automation, scripting, extensibility |

---

## Commands

```powershell
arqyon --help
arqyon --version

arqyon system                     # CPU, RAM, OS, uptime
arqyon system --disk              # + disk usage on C:\
arqyon system --disk --drive D:\  # specific drive

arqyon dev                        # detected tools
arqyon dev --all                  # include not-found tools

arqyon hash <file>                # MD5 + SHA-256
arqyon hash <file> -a sha256      # SHA-256 only
arqyon hash <file> -a md5         # MD5 only

arqyon doctor                     # environment health check

arqyon run --list                 # list plugins
arqyon run <plugin>               # run a plugin
arqyon run cleanup C:\Projects    # run with args
```

---

## Build

**Prerequisites:** Visual Studio 2022 (C++), CMake 3.20+, .NET 10 SDK, Python 3

```powershell
# From the arqyon-cli/ directory:

.\build.ps1               # Release build
.\build.ps1 -Config Debug # Debug build
.\build.ps1 -Clean        # Clean + rebuild
```

**Manual build:**
```powershell
# 1. Build C++ DLL
cd ArqyonNative
cmake -S . -B build
cmake --build build --config Release

# 2. Build C# exe
cd ../ArqyonCLI
dotnet build -c Release
```

---

## Run and Test Your CLI

After a successful Release build, run these commands from the project root in PowerShell:

```powershell
.\ArqyonCLI\bin\Release\net10.0\arqyon.exe --help
.\ArqyonCLI\bin\Release\net10.0\arqyon.exe system
```

The `system` command displays the ARQYON banner and your computer's OS, hostname, user, CPU, core count, RAM, and uptime.

To use the shorter `arqyon` command, add the build output directory to `PATH` for the current PowerShell session:

```powershell
# Run from the project root.
$env:Path += ";$PWD\ArqyonCLI\bin\Release\net10.0"

arqyon system
arqyon system --disk
arqyon system --disk --drive D:\
```

Use a drive that exists on your computer. This `PATH` change lasts only until you close the PowerShell session; repeat it in a new session or use the executable path directly. If `arqyon` is not recognized, check that the Release build succeeded and that you added the correct directory.

For a Debug build, replace `Release` with `Debug` in the paths above. Keep `ArqyonNative.dll` beside `arqyon.exe` in the output directory; the build copies it there automatically. Running the built CLI requires the .NET 10 runtime, which is also included with the .NET 10 SDK.

---

## Writing a Plugin

Place any `.py` file in the `plugins/` directory next to `arqyon.exe`.

```python
# plugins/my_plugin.py
import sys

def main():
    args = sys.argv[1:]
    print(f"Hello from ARQYON! Args: {args}")
    return 0

if __name__ == "__main__":
    sys.exit(main())
```

Run it with: `arqyon run my_plugin`

---

## Project Structure

```
arqyon-cli/
├── ArqyonNative/           ← C++17 DLL
│   ├── include/
│   │   └── arqyon_native.h
│   ├── src/
│   │   ├── sys_info.cpp
│   │   ├── disk_info.cpp
│   │   ├── dev_detect.cpp
│   │   ├── file_hash.cpp
│   │   └── diagnostics.cpp
│   └── CMakeLists.txt
├── ArqyonCLI/              ← C# .NET 10 host
│   ├── Commands/
│   │   ├── SystemCommand.cs
│   │   ├── DevCommand.cs
│   │   ├── HashCommand.cs
│   │   ├── DoctorCommand.cs
│   │   └── RunCommand.cs
│   ├── Core/
│   │   ├── NativeBridge.cs
│   │   ├── OutputRenderer.cs
│   │   └── PythonBridge.cs
│   ├── Program.cs
│   └── ArqyonCLI.csproj
├── plugins/                ← Python automation
│   ├── example_plugin.py
│   └── cleanup.py
├── build.ps1               ← One-shot build
├── CMakeLists.txt
└── README.md
```

---

## Organization
**ARQYON** — Think Beyond. Build Without Limits.

## License
MIT License
