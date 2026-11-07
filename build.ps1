# ============================================================
#  ARQYON — build.ps1
#  Builds the C++ core DLL then the C# shell in one shot.
#  Run from the repo root:  .\build.ps1
#  Optional: .\build.ps1 -Config Release
# ============================================================

param(
    [ValidateSet("Debug","Release")]
    [string]$Config = "Release"
)

$ErrorActionPreference = "Stop"
$Root    = $PSScriptRoot
$Build   = "$Root\build"
$ShellOut = "$Root\shell\bin\$Config\net8.0\win-x64"

function Step($msg) {
    Write-Host ""
    Write-Host "  ► $msg" -ForegroundColor Cyan
}

function Ok($msg) {
    Write-Host "  ✔ $msg" -ForegroundColor Green
}

function Fail($msg) {
    Write-Host "  ✘ $msg" -ForegroundColor Red
    exit 1
}

# ── 1. CMake configure ───────────────────────────────────────
Step "Configuring C++ core (CMake)..."
cmake -S "$Root" -B "$Build" -DCMAKE_BUILD_TYPE=$Config
if ($LASTEXITCODE -ne 0) { Fail "CMake configure failed." }
Ok "CMake configured."

# ── 2. Build C++ DLL ─────────────────────────────────────────
Step "Building arqyon_core.dll ($Config)..."
cmake --build "$Build" --config $Config
if ($LASTEXITCODE -ne 0) { Fail "C++ build failed." }
Ok "arqyon_core.dll built."

# ── 3. Locate the DLL ────────────────────────────────────────
$DllSrc = "$Build\bin\$Config\arqyon_core.dll"
if (-not (Test-Path $DllSrc)) {
    # Fallback: flat bin directory (some generators don't use config subfolder)
    $DllSrc = "$Build\bin\arqyon_core.dll"
}
if (-not (Test-Path $DllSrc)) {
    Fail "Could not find arqyon_core.dll. Expected at: $Build\bin\$Config\arqyon_core.dll"
}

# ── 4. Build C# shell ────────────────────────────────────────
Step "Building ArqyonShell (dotnet)..."
dotnet build "$Root\shell\ArqyonShell.csproj" -c $Config
if ($LASTEXITCODE -ne 0) { Fail "dotnet build failed." }
Ok "ArqyonShell built."

# ── 5. Copy DLL to shell output ──────────────────────────────
Step "Copying arqyon_core.dll to shell output..."
$Destinations = @(
    "$Root\shell\bin\$Config\net8.0\win-x64",
    "$Root\shell\bin\$Config\net8.0"
)
foreach ($dest in $Destinations) {
    if (Test-Path $dest) {
        Copy-Item -Force $DllSrc "$dest\arqyon_core.dll"
        Ok "Copied to $dest"
    }
}

# ── Done ─────────────────────────────────────────────────────
Write-Host ""
Write-Host "  ══════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host "  ARQYON build complete ($Config)" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Run:  .\shell\bin\$Config\net8.0\win-x64\arqyon.exe --help" -ForegroundColor Gray
Write-Host "  ══════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host ""
