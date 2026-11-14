#define ARQYON_EXPORTS
#include "../include/arqyon_native.h"

#include <windows.h>
#include <sysinfoapi.h>
#include <lmcons.h>
#include <cstring>
#include <cstdio>

// ─── OS Name ─────────────────────────────────────────────────────────────────
ARQYON_API void get_os_name(char* buf, int len) {
    OSVERSIONINFOEXW osvi = { sizeof(osvi) };
#pragma warning(suppress: 4996)
    GetVersionExW((LPOSVERSIONINFOW)&osvi);

    const char* name = "Windows (Unknown)";
    if (osvi.dwMajorVersion == 10) {
        if (osvi.wProductType == VER_NT_WORKSTATION)
            name = (osvi.dwBuildNumber >= 22000) ? "Windows 11" : "Windows 10";
        else
            name = "Windows Server 2016+";
    } else if (osvi.dwMajorVersion == 6 && osvi.dwMinorVersion == 3) {
        name = (osvi.wProductType == VER_NT_WORKSTATION) ? "Windows 8.1" : "Windows Server 2012 R2";
    }

    char full[256];
    snprintf(full, sizeof(full), "%s (Build %lu)", name, osvi.dwBuildNumber);
    strncpy_s(buf, len, full, _TRUNCATE);
}

// ─── CPU Name ────────────────────────────────────────────────────────────────
ARQYON_API void get_cpu_name(char* buf, int len) {
    HKEY hKey;
    if (RegOpenKeyExA(HKEY_LOCAL_MACHINE,
        "HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0",
        0, KEY_READ, &hKey) == ERROR_SUCCESS) {
        DWORD sz = (DWORD)len;
        RegQueryValueExA(hKey, "ProcessorNameString", nullptr, nullptr, (LPBYTE)buf, &sz);
        RegCloseKey(hKey);
    } else {
        strncpy_s(buf, len, "Unknown CPU", _TRUNCATE);
    }
}

// ─── CPU Core Count ───────────────────────────────────────────────────────────
ARQYON_API int get_cpu_core_count() {
    SYSTEM_INFO si;
    GetSystemInfo(&si);
    return (int)si.dwNumberOfProcessors;
}

// ─── RAM ──────────────────────────────────────────────────────────────────────
ARQYON_API long get_total_ram_mb() {
    MEMORYSTATUSEX ms = { sizeof(ms) };
    GlobalMemoryStatusEx(&ms);
    return (long)(ms.ullTotalPhys / (1024 * 1024));
}

ARQYON_API long get_free_ram_mb() {
    MEMORYSTATUSEX ms = { sizeof(ms) };
    GlobalMemoryStatusEx(&ms);
    return (long)(ms.ullAvailPhys / (1024 * 1024));
}

// ─── Uptime ───────────────────────────────────────────────────────────────────
ARQYON_API void get_uptime(char* buf, int len) {
    ULONGLONG ms = GetTickCount64();
    long long seconds = ms / 1000;
    int days    = (int)(seconds / 86400);
    int hours   = (int)((seconds % 86400) / 3600);
    int minutes = (int)((seconds % 3600) / 60);
    snprintf(buf, len, "%dd %dh %dm", days, hours, minutes);
}

// ─── Hostname ─────────────────────────────────────────────────────────────────
ARQYON_API void get_hostname(char* buf, int len) {
    DWORD sz = (DWORD)len;
    if (!GetComputerNameA(buf, &sz))
        strncpy_s(buf, len, "Unknown", _TRUNCATE);
}

// ─── Username ─────────────────────────────────────────────────────────────────
ARQYON_API void get_username(char* buf, int len) {
    DWORD sz = (DWORD)len;
    if (!GetUserNameA(buf, &sz))
        strncpy_s(buf, len, "Unknown", _TRUNCATE);
}
