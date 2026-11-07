
// ============================================================
// ARQYON CORE — diagnostics.cpp
// Aggregates subsystem outputs into a full diagnostic JSON report.
// ============================================================

#include "diagnostics.h"
#include "system_info.h"
#include "dev_tools.h"

#ifdef _WIN32

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>

#endif

#include <sstream>
#include <string>
#include <ctime>
#include <iomanip>
#include <cstdint>

namespace arqyon::diag
{

    // ------------------------------------------------------------
    // JSON String Escaping
    // ------------------------------------------------------------

    static std::string EscapeJson(const std::string &value)
    {
        std::ostringstream escaped;

        for (unsigned char c : value)
        {
            switch (c)
            {
            case '"':
                escaped << "\\\"";
                break;
            case '\\':
                escaped << "\\\\";
                break;
            case '\b':
                escaped << "\\b";
                break;
            case '\f':
                escaped << "\\f";
                break;
            case '\n':
                escaped << "\\n";
                break;
            case '\r':
                escaped << "\\r";
                break;
            case '\t':
                escaped << "\\t";
                break;

            default:
                if (c < 0x20)
                {
                    escaped << "\\u"
                            << std::hex
                            << std::setw(4)
                            << std::setfill('0')
                            << static_cast<int>(c)
                            << std::dec;
                }
                else
                {
                    escaped << static_cast<char>(c);
                }
            }
        }

        return escaped.str();
    }

    // ------------------------------------------------------------
    // ISO 8601 UTC Timestamp
    // ------------------------------------------------------------

    static std::string IsoTimestamp()
    {
        time_t now = time(nullptr);
        struct tm tm_utc = {};

#ifdef _WIN32
        gmtime_s(&tm_utc, &now);
#else
        gmtime_r(&now, &tm_utc);
#endif

        char buffer[32] = {};

        strftime(
            buffer,
            sizeof(buffer),
            "%Y-%m-%dT%H:%M:%SZ",
            &tm_utc);

        return buffer;
    }

    // ------------------------------------------------------------
    // Network Connectivity Check
    // ------------------------------------------------------------

    static std::string CheckNetworkConnectivity()
    {

#ifdef _WIN32

        WSADATA wsaData = {};

        if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
        {
            return "unknown";
        }

        struct addrinfo hints = {};
        struct addrinfo *result = nullptr;

        hints.ai_family = AF_UNSPEC;
        hints.ai_socktype = SOCK_STREAM;
        hints.ai_protocol = IPPROTO_TCP;

        int status = getaddrinfo(
            "8.8.8.8",
            "53",
            &hints,
            &result);

        bool reachable = (status == 0);

        if (result != nullptr)
        {
            freeaddrinfo(result);
        }

        WSACleanup();

        return reachable ? "reachable" : "unreachable";

#else

        return "unknown";

#endif
    }

    // ------------------------------------------------------------
    // System Uptime
    // ------------------------------------------------------------

    static std::string GetUptimeSeconds()
    {

#ifdef _WIN32

        ULONGLONG milliseconds = GetTickCount64();

        return std::to_string(milliseconds / 1000);

#else

        return "0";

#endif
    }

    // ------------------------------------------------------------
    // Full System Diagnostics
    // ------------------------------------------------------------

    std::string RunFullDiagnostics()
    {

        const std::string timestamp = IsoTimestamp();
        const std::string cpu = arqyon::system::GetCpuInfoJson();
        const std::string ram = arqyon::system::GetRamInfoJson();
        const std::string os = arqyon::system::GetOsInfoJson();
        const std::string disk = arqyon::system::GetDiskInfoJson();
        const std::string languages = arqyon::dev::DetectLanguagesJson();
        const std::string network = CheckNetworkConnectivity();
        const std::string uptime = GetUptimeSeconds();

        std::ostringstream json;

        json << "{"
             << "\"timestamp\":\"" << EscapeJson(timestamp) << "\","
             << "\"uptime_sec\":" << uptime << ","
             << "\"network\":\"" << EscapeJson(network) << "\","
             << "\"os\":" << os << ","
             << "\"cpu\":" << cpu << ","
             << "\"ram\":" << ram << ","
             << "\"disks\":" << disk << ","
             << "\"languages\":" << languages
             << "}";

        return json.str();
    }

} // namespace arqyon::diag
