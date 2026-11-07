#pragma once

// ============================================================
//  ARQYON CORE — DLL Export Contract
//  Every function here is callable from C# via P/Invoke.
//  All char* returns are heap-allocated; call FreeCoreString()
//  after consuming them, without exception.
// ============================================================

#ifdef _WIN32
  #ifdef ARQYON_CORE_EXPORTS
    #define ARQYON_API extern "C" __declspec(dllexport)
  #else
    #define ARQYON_API extern "C" __declspec(dllimport)
  #endif
#else
  #define ARQYON_API extern "C" __attribute__((visibility("default")))
#endif

// ── Memory ───────────────────────────────────────────────────
// Must be called from C# after consuming any char* return value.
ARQYON_API void FreeCoreString(char* ptr);

// ── System Info ──────────────────────────────────────────────
ARQYON_API char* GetCpuInfo();      // JSON: name, cores, threads, speed
ARQYON_API char* GetRamInfo();      // JSON: total, available, used, percent
ARQYON_API char* GetOsInfo();       // JSON: name, version, build, arch
ARQYON_API char* GetDiskInfo();     // JSON array of drives

// ── Developer Tools ──────────────────────────────────────────
ARQYON_API char* DetectLanguages(); // JSON array: [{name, version, path}]
ARQYON_API char* ScanEnvPath();     // JSON array of PATH entries

// ── File Utilities ───────────────────────────────────────────
ARQYON_API char* HashFileSHA256(const char* filepath);
ARQYON_API char* HashFileMD5(const char* filepath);
ARQYON_API char* GetFileInfo(const char* filepath); // JSON: size, type, perms, timestamps

// ── Diagnostics ──────────────────────────────────────────────
ARQYON_API char* RunDiagnostics();  // Full JSON diagnostic report

// ── Python Runner ────────────────────────────────────────────
ARQYON_API int  RunPythonScript(const char* scriptPath, const char* args);
ARQYON_API char* GetPythonVersion();
