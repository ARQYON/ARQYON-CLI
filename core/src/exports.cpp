// ============================================================
//  ARQYON CORE — exports.cpp
//  DLL entry point + all exported C functions.
//  Each function allocates a char* with _strdup / new,
//  which C# must free via FreeCoreString().
// ============================================================

#define ARQYON_CORE_EXPORTS
#include "arqyon_exports.h"
#include "system_info.h"
#include "dev_tools.h"
#include "file_utils.h"
#include "diagnostics.h"
#include "py_runner.h"

#include <cstring>
#include <string>

// ── Helpers ──────────────────────────────────────────────────

static char* AllocReturn(const std::string& s) {
    char* buf = new char[s.size() + 1];
    std::memcpy(buf, s.c_str(), s.size() + 1);
    return buf;
}

// ── Memory ───────────────────────────────────────────────────

ARQYON_API void FreeCoreString(char* ptr) {
    delete[] ptr;
}

// ── System Info ──────────────────────────────────────────────

ARQYON_API char* GetCpuInfo() {
    return AllocReturn(arqyon::system::GetCpuInfoJson());
}

ARQYON_API char* GetRamInfo() {
    return AllocReturn(arqyon::system::GetRamInfoJson());
}

ARQYON_API char* GetOsInfo() {
    return AllocReturn(arqyon::system::GetOsInfoJson());
}

ARQYON_API char* GetDiskInfo() {
    return AllocReturn(arqyon::system::GetDiskInfoJson());
}

// ── Developer Tools ──────────────────────────────────────────

ARQYON_API char* DetectLanguages() {
    return AllocReturn(arqyon::dev::DetectLanguagesJson());
}

ARQYON_API char* ScanEnvPath() {
    return AllocReturn(arqyon::dev::ScanEnvPathJson());
}

// ── File Utilities ───────────────────────────────────────────

ARQYON_API char* HashFileSHA256(const char* filepath) {
    return AllocReturn(arqyon::file::HashSHA256(filepath ? filepath : ""));
}

ARQYON_API char* HashFileMD5(const char* filepath) {
    return AllocReturn(arqyon::file::HashMD5(filepath ? filepath : ""));
}

ARQYON_API char* GetFileInfo(const char* filepath) {
    return AllocReturn(arqyon::file::GetFileInfoJson(filepath ? filepath : ""));
}

// ── Diagnostics ──────────────────────────────────────────────

ARQYON_API char* RunDiagnostics() {
    return AllocReturn(arqyon::diag::RunFullDiagnostics());
}

// ── Python Runner ────────────────────────────────────────────

ARQYON_API int RunPythonScript(const char* scriptPath, const char* args) {
    return arqyon::python::RunScript(
        scriptPath ? scriptPath : "",
        args       ? args       : ""
    );
}

ARQYON_API char* GetPythonVersion() {
    return AllocReturn(arqyon::python::GetVersion());
}

// ── DLL Entry (Windows) ──────────────────────────────────────
#ifdef _WIN32
#include <windows.h>
BOOL APIENTRY DllMain(HMODULE, DWORD reason, LPVOID) {
    return TRUE;
}
#endif
