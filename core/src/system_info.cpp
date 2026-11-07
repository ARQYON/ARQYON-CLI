// ============================================================
//  ARQYON CORE — system_info.cpp
//  Windows API system inspection: CPU, RAM, OS, Disk.
//  All functions return JSON strings.
// ============================================================

#include "system_info.h"

#ifdef _WIN32
#include <windows.h>
#include <sysinfoapi.h>
#include <intrin.h>
#include <pdh.h>
#pragma comment(lib, "pdh.lib")
#endif

#include <sstream>
#include <iomanip>
#include <string>
#include <vector>

namespace arqyon::system {

// ── Helpers ──────────────────────────────────────────────────

static std::string EscapeJson(const std::string& s) {
    std::string out;
    for (char c : s) {
        switch (c) {
            case '"':  out += "\\\""; break;
            case '\\': out += "\\\\"; break;
            case '\n': out += "\\n";  break;
            case '\r': out += "\\r";  break;
            case '\t': out += "\\t";  break;
            default:   out += c;
        }
    }
    return out;
}

static std::string FormatBytes(unsigned long long bytes) {
    const double GB = 1024.0 * 1024.0 * 1024.0;
    const double MB = 1024.0 * 1024.0;
    std::ostringstream ss;
    ss << std::fixed << std::setprecision(2);
    if (bytes >= (unsigned long long)GB)
        ss << (bytes / GB) << " GB";
    else if (bytes >= (unsigned long long)MB)
        ss << (bytes / MB) << " MB";
    else
        ss << bytes << " B";
    return ss.str();
}

// ── CPU ──────────────────────────────────────────────────────

std::string GetCpuInfoJson() {
#ifdef _WIN32
    // CPU brand string via CPUID
    char cpuBrand[49] = {};
    int cpuInfo[4] = {};
    __cpuid(cpuInfo, 0x80000002);
    memcpy(cpuBrand,      cpuInfo, sizeof(cpuInfo));
    __cpuid(cpuInfo, 0x80000003);
    memcpy(cpuBrand + 16, cpuInfo, sizeof(cpuInfo));
    __cpuid(cpuInfo, 0x80000004);
    memcpy(cpuBrand + 32, cpuInfo, sizeof(cpuInfo));

    std::string brand = cpuBrand;
    // Trim leading spaces
    size_t start = brand.find_first_not_of(' ');
    if (start != std::string::npos) brand = brand.substr(start);

    SYSTEM_INFO si;
    GetSystemInfo(&si);
    DWORD logicalCores = si.dwNumberOfProcessors;

    // Physical cores via registry
    DWORD physicalCores = 0;
    DWORD len = 0;
    GetLogicalProcessorInformationEx(RelationProcessorCore, nullptr, &len);
    std::vector<BYTE> buf(len);
    if (GetLogicalProcessorInformationEx(RelationProcessorCore,
        reinterpret_cast<PSYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX>(buf.data()), &len)) {
        DWORD offset = 0;
        while (offset < len) {
            auto* info = reinterpret_cast<PSYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX>(buf.data() + offset);
            if (info->Relationship == RelationProcessorCore) physicalCores++;
            offset += info->Size;
        }
    }
    if (physicalCores == 0) physicalCores = logicalCores;

    // CPU speed from registry
    HKEY hKey;
    DWORD mhz = 0;
    if (RegOpenKeyExA(HKEY_LOCAL_MACHINE,
        "HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0",
        0, KEY_READ, &hKey) == ERROR_SUCCESS) {
        DWORD sz = sizeof(mhz);
        RegQueryValueExA(hKey, "~MHz", nullptr, nullptr, (LPBYTE)&mhz, &sz);
        RegCloseKey(hKey);
    }

    std::ostringstream json;
    json << "{"
         << "\"name\":\""    << EscapeJson(brand) << "\","
         << "\"cores\":"     << physicalCores << ","
         << "\"threads\":"   << logicalCores  << ","
         << "\"speed_mhz\":" << mhz           << ","
         << "\"speed_ghz\":" << std::fixed << std::setprecision(2) << (mhz / 1000.0)
         << "}";
    return json.str();
#else
    return R"({"error":"Unsupported platform"})";
#endif
}

// ── RAM ──────────────────────────────────────────────────────

std::string GetRamInfoJson() {
#ifdef _WIN32
    MEMORYSTATUSEX ms;
    ms.dwLength = sizeof(ms);
    GlobalMemoryStatusEx(&ms);

    unsigned long long total     = ms.ullTotalPhys;
    unsigned long long available = ms.ullAvailPhys;
    unsigned long long used      = total - available;
    double percent               = (double)used / total * 100.0;

    std::ostringstream json;
    json << std::fixed << std::setprecision(1);
    json << "{"
         << "\"total\":\""     << FormatBytes(total)     << "\","
         << "\"available\":\""  << FormatBytes(available) << "\","
         << "\"used\":\""       << FormatBytes(used)      << "\","
         << "\"percent\":"      << percent
         << "}";
    return json.str();
#else
    return R"({"error":"Unsupported platform"})";
#endif
}

// ── OS ───────────────────────────────────────────────────────

std::string GetOsInfoJson() {
#ifdef _WIN32
    // Read version from registry (accurate on Windows 10/11)
    HKEY hKey;
    char productName[256] = "Windows";
    char displayVersion[64] = "";
    char buildNumber[32]  = "";
    char ubr[32]          = "";

    if (RegOpenKeyExA(HKEY_LOCAL_MACHINE,
        "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion",
        0, KEY_READ, &hKey) == ERROR_SUCCESS) {
        DWORD sz;
        sz = sizeof(productName);
        RegQueryValueExA(hKey, "ProductName", nullptr, nullptr, (LPBYTE)productName, &sz);
        sz = sizeof(displayVersion);
        RegQueryValueExA(hKey, "DisplayVersion", nullptr, nullptr, (LPBYTE)displayVersion, &sz);
        sz = sizeof(buildNumber);
        RegQueryValueExA(hKey, "CurrentBuildNumber", nullptr, nullptr, (LPBYTE)buildNumber, &sz);
        DWORD ubrVal = 0; sz = sizeof(ubrVal);
        if (RegQueryValueExA(hKey, "UBR", nullptr, nullptr, (LPBYTE)&ubrVal, &sz) == ERROR_SUCCESS) {
            snprintf(ubr, sizeof(ubr), ".%lu", ubrVal);
        }
        RegCloseKey(hKey);
    }

    // Architecture
    SYSTEM_INFO si; GetNativeSystemInfo(&si);
    std::string arch;
    switch (si.wProcessorArchitecture) {
        case PROCESSOR_ARCHITECTURE_AMD64: arch = "x64"; break;
        case PROCESSOR_ARCHITECTURE_ARM64: arch = "ARM64"; break;
        case PROCESSOR_ARCHITECTURE_INTEL: arch = "x86"; break;
        default: arch = "Unknown";
    }

    std::string fullBuild = std::string(buildNumber) + ubr;

    std::ostringstream json;
    json << "{"
         << "\"name\":\""    << EscapeJson(productName)    << "\","
         << "\"version\":\"" << EscapeJson(displayVersion)  << "\","
         << "\"build\":\""   << EscapeJson(fullBuild)       << "\","
         << "\"arch\":\""    << EscapeJson(arch)            << "\""
         << "}";
    return json.str();
#else
    return R"({"error":"Unsupported platform"})";
#endif
}

// ── Disk ─────────────────────────────────────────────────────

std::string GetDiskInfoJson() {
#ifdef _WIN32
    std::ostringstream json;
    json << "[";

    DWORD drives = GetLogicalDrives();
    bool first = true;
    for (int i = 0; i < 26; i++) {
        if (!(drives & (1 << i))) continue;
        char root[4] = { (char)('A' + i), ':', '\\', 0 };
        if (GetDriveTypeA(root) != DRIVE_FIXED) continue;

        ULARGE_INTEGER freeBytes, totalBytes, totalFreeBytes;
        if (!GetDiskFreeSpaceExA(root, &freeBytes, &totalBytes, &totalFreeBytes)) continue;

        unsigned long long total = totalBytes.QuadPart;
        unsigned long long free  = freeBytes.QuadPart;
        unsigned long long used  = total - free;
        double percent           = (double)used / total * 100.0;

        if (!first) json << ",";
        first = false;

        json << std::fixed << std::setprecision(1);
        json << "{"
             << "\"drive\":\"" << root[0] << ":\\\\\","
             << "\"total\":\""   << FormatBytes(total) << "\","
             << "\"used\":\""    << FormatBytes(used)  << "\","
             << "\"free\":\""    << FormatBytes(free)  << "\","
             << "\"percent\":"   << percent
             << "}";
    }
    json << "]";
    return json.str();
#else
    return R"([{"error":"Unsupported platform"}])";
#endif
}

} // namespace arqyon::system
