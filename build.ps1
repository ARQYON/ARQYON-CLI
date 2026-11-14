<#
.SYNOPSIS
    Builds the ARQYON CLI native DLL and .NET 10 executable.
.EXAMPLE
    .\build.ps1
.EXAMPLE
    .\build.ps1 -Config Debug
.EXAMPLE
    .\build.ps1 -Clean
#>

param(
    [ValidateSet("Release", "Debug")]
    [string] $Config = "Release",
    [switch] $Clean
)

$ErrorActionPreference = "Stop"

$Native = Join-Path $PSScriptRoot "ArqyonNative"
$CSharp = Join-Path $PSScriptRoot "ArqyonCLI"
$Build = Join-Path $Native "build"
$Exe = Join-Path $CSharp "bin\$Config\net10.0\arqyon.exe"

function Write-Step([string] $Message) {
    Write-Host ""
    Write-Host "  >> $Message" -ForegroundColor Cyan
}

function Write-Ok([string] $Message) {
    Write-Host "  [OK] $Message" -ForegroundColor Green
}

try {
    foreach ($Tool in @("cmake", "dotnet")) {
        if (-not (Get-Command $Tool -ErrorAction SilentlyContinue)) {
            throw "$Tool is not installed or not in PATH."
        }
    }

    if (-not (Test-Path -LiteralPath $Native -PathType Container)) {
        throw "Native project directory not found: $Native"
    }
    if (-not (Test-Path -LiteralPath $CSharp -PathType Container)) {
        throw "C# project directory not found: $CSharp"
    }

    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "              ARQYON CLI BUILDER              " -ForegroundColor Cyan
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "  Configuration: $Config"

    if ($Clean) {
        Write-Step "Cleaning previous build artifacts"
        $Directories = @(
            $Build
            (Join-Path $CSharp "bin")
            (Join-Path $CSharp "obj")
        )
        foreach ($Directory in $Directories) {
            if (Test-Path -LiteralPath $Directory) {
                Remove-Item -LiteralPath $Directory -Recurse -Force
                Write-Ok "Removed $Directory"
            }
        }
    }

    Write-Step "Configuring the C++ project"
    & cmake -S $Native -B $Build "-DCMAKE_BUILD_TYPE=$Config"
    if ($LASTEXITCODE -ne 0) {
        throw "CMake configuration failed (exit code $LASTEXITCODE)."
    }
    Write-Ok "CMake configured successfully"

    Write-Step "Building ArqyonNative.dll"
    & cmake --build $Build --config $Config
    if ($LASTEXITCODE -ne 0) {
        throw "C++ build failed (exit code $LASTEXITCODE)."
    }
    Write-Ok "Native C++ build completed"

    Write-Step "Building ARQYON CLI (.NET 10)"
    Push-Location -LiteralPath $CSharp
    try {
        & dotnet build -c $Config
        if ($LASTEXITCODE -ne 0) {
            throw ".NET build failed (exit code $LASTEXITCODE)."
        }
    }
    finally {
        Pop-Location
    }
    Write-Ok "C# build completed"

    if (-not (Test-Path -LiteralPath $Exe -PathType Leaf)) {
        throw "Expected executable not found: $Exe. Check the C# project output configuration."
    }

    Write-Host ""
    Write-Host "BUILD SUCCESSFUL" -ForegroundColor Green
    Write-Host "  Executable: $Exe"
    Write-Host ('  Run: & "{0}" --help' -f $Exe) -ForegroundColor Cyan
}
catch {
    Write-Host ("  [ERROR] " + $_.Exception.Message) -ForegroundColor Red
    exit 1
}