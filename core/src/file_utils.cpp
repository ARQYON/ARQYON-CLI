// ============================================================
//  ARQYON CORE — file_utils.cpp
//  File hashing (SHA-256, MD5) using Windows CNG API,
//  and file metadata inspection.
// ============================================================

#include "file_utils.h"

#ifdef _WIN32
#include <windows.h>
#include <bcrypt.h>
#pragma comment(lib, "bcrypt.lib")
#endif

#include <sstream>
#include <iomanip>
#include <fstream>
#include <vector>
#include <string>
#include <ctime>

namespace arqyon::file {

// ── Helpers ──────────────────────────────────────────────────

static std::string EscapeJson(const std::string& s) {
    std::string out;
    for (char c : s) {
        if      (c == '"')  out += "\\\"";
        else if (c == '\\') out += "\\\\";
        else if (c == '\n') out += "\\n";
        else if (c == '\r') {}
        else                out += c;
    }
    return out;
}

static std::string ToHex(const std::vector<BYTE>& bytes) {
    std::ostringstream ss;
    for (auto b : bytes)
        ss << std::hex << std::setw(2) << std::setfill('0') << (int)b;
    return ss.str();
}

// ── CNG-based hashing ────────────────────────────────────────

#ifdef _WIN32
static std::string HashFile(const std::string& path, LPCWSTR algorithm) {
    // Open file
    HANDLE hFile = CreateFileA(path.c_str(), GENERIC_READ,
        FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, nullptr);
    if (hFile == INVALID_HANDLE_VALUE)
        return "{\"error\":\"Cannot open file\"}";

    // Open CNG algorithm provider
    BCRYPT_ALG_HANDLE hAlg = nullptr;
    if (!BCRYPT_SUCCESS(BCryptOpenAlgorithmProvider(&hAlg, algorithm, nullptr, 0))) {
        CloseHandle(hFile);
        return "{\"error\":\"Cannot open hash algorithm\"}";
    }

    // Get hash object size
    DWORD hashObjSize = 0, hashLen = 0, dummy = 0;
    BCryptGetProperty(hAlg, BCRYPT_OBJECT_LENGTH, (PUCHAR)&hashObjSize, sizeof(DWORD), &dummy, 0);
    BCryptGetProperty(hAlg, BCRYPT_HASH_LENGTH,   (PUCHAR)&hashLen,     sizeof(DWORD), &dummy, 0);

    std::vector<BYTE> hashObj(hashObjSize);
    std::vector<BYTE> hashVal(hashLen);

    BCRYPT_HASH_HANDLE hHash = nullptr;
    BCryptCreateHash(hAlg, &hHash, hashObj.data(), hashObjSize, nullptr, 0, 0);

    // Hash file in chunks
    const DWORD CHUNK = 1024 * 1024; // 1MB chunks
    std::vector<BYTE> buf(CHUNK);
    DWORD bytesRead = 0;
    while (ReadFile(hFile, buf.data(), CHUNK, &bytesRead, nullptr) && bytesRead > 0)
        BCryptHashData(hHash, buf.data(), bytesRead, 0);

    BCryptFinishHash(hHash, hashVal.data(), hashLen, 0);

    BCryptDestroyHash(hHash);
    BCryptCloseAlgorithmProvider(hAlg, 0);
    CloseHandle(hFile);

    return ToHex(hashVal);
}
#endif

// ── Public API ───────────────────────────────────────────────

std::string HashSHA256(const std::string& path) {
#ifdef _WIN32
    return HashFile(path, BCRYPT_SHA256_ALGORITHM);
#else
    return "unsupported";
#endif
}

std::string HashMD5(const std::string& path) {
#ifdef _WIN32
    return HashFile(path, BCRYPT_MD5_ALGORITHM);
#else
    return "unsupported";
#endif
}

std::string GetFileInfoJson(const std::string& path) {
#ifdef _WIN32
    WIN32_FILE_ATTRIBUTE_DATA attrs;
    if (!GetFileAttributesExA(path.c_str(), GetFileExInfoStandard, &attrs)) {
        return "{\"error\":\"File not found or inaccessible\"}";
    }

    // File size
    ULARGE_INTEGER size;
    size.HighPart = attrs.nFileSizeHigh;
    size.LowPart  = attrs.nFileSizeLow;
    unsigned long long fileSize = size.QuadPart;

    // Type (dir vs file, extension)
    bool isDir = (attrs.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0;
    std::string type = isDir ? "directory" : "file";

    // Extension
    std::string ext;
    size_t dot = path.rfind('.');
    if (!isDir && dot != std::string::npos)
        ext = path.substr(dot);

    // Timestamps (FILETIME → ISO-ish string)
    auto FTtoStr = [](const FILETIME& ft) -> std::string {
        SYSTEMTIME st;
        FileTimeToSystemTime(&ft, &st);
        char buf[32];
        snprintf(buf, sizeof(buf), "%04d-%02d-%02d %02d:%02d:%02d UTC",
            st.wYear, st.wMonth, st.wDay,
            st.wHour, st.wMinute, st.wSecond);
        return buf;
    };

    // Attributes string
    std::string attribs;
    if (attrs.dwFileAttributes & FILE_ATTRIBUTE_READONLY)  attribs += "readonly ";
    if (attrs.dwFileAttributes & FILE_ATTRIBUTE_HIDDEN)    attribs += "hidden ";
    if (attrs.dwFileAttributes & FILE_ATTRIBUTE_SYSTEM)    attribs += "system ";
    if (attrs.dwFileAttributes & FILE_ATTRIBUTE_ARCHIVE)   attribs += "archive ";
    if (attribs.empty()) attribs = "normal";
    else while (!attribs.empty() && attribs.back() == ' ') attribs.pop_back();

    std::ostringstream json;
    json << "{"
         << "\"path\":\""       << EscapeJson(path)              << "\","
         << "\"type\":\""       << type                          << "\","
         << "\"extension\":\"" << EscapeJson(ext)               << "\","
         << "\"size_bytes\":"  << fileSize                       << ","
         << "\"size\":\""      << [&]() -> std::string {
                std::ostringstream s;
                s << std::fixed << std::setprecision(2);
                if (fileSize >= 1073741824ULL) s << (fileSize / 1073741824.0) << " GB";
                else if (fileSize >= 1048576ULL) s << (fileSize / 1048576.0) << " MB";
                else if (fileSize >= 1024ULL)    s << (fileSize / 1024.0) << " KB";
                else s << fileSize << " B";
                return s.str();
            }()                                                   << "\","
         << "\"created\":\""   << FTtoStr(attrs.ftCreationTime)  << "\","
         << "\"modified\":\""  << FTtoStr(attrs.ftLastWriteTime) << "\","
         << "\"accessed\":\""  << FTtoStr(attrs.ftLastAccessTime)<< "\","
         << "\"attributes\":\"" << EscapeJson(attribs)           << "\""
         << "}";
    return json.str();
#else
    return R"({"error":"Unsupported platform"})";
#endif
}

} // namespace arqyon::file
