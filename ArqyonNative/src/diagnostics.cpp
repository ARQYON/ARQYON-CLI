#define ARQYON_EXPORTS
#include "../include/arqyon_native.h"

// Keep legacy Winsock declarations out of windows.h.
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <winsock2.h>
#include <windows.h>
#include <ws2tcpip.h>
#include <cstring>
#include <cstdio>

#pragma comment(lib, "ws2_32.lib")

// ─── Internet Check ───────────────────────────────────────────────────────────
// Returns 1 if reachable, 0 if not
ARQYON_API int check_internet(const char* host) {
    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) return 0;

    struct addrinfo hints = {}, *res = nullptr;
    hints.ai_family   = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;

    int rc = getaddrinfo(host, "80", &hints, &res);
    if (rc != 0) { WSACleanup(); return 0; }

    SOCKET sock = socket(res->ai_family, res->ai_socktype, res->ai_protocol);
    if (sock == INVALID_SOCKET) {
        freeaddrinfo(res); WSACleanup(); return 0;
    }

    // 2-second timeout
    DWORD timeout = 2000;
    setsockopt(sock, SOL_SOCKET, SO_RCVTIMEO, (char*)&timeout, sizeof(timeout));
    setsockopt(sock, SOL_SOCKET, SO_SNDTIMEO, (char*)&timeout, sizeof(timeout));

    int connected = (connect(sock, res->ai_addr, (int)res->ai_addrlen) == 0) ? 1 : 0;

    closesocket(sock);
    freeaddrinfo(res);
    WSACleanup();
    return connected;
}

// ─── Env Var ──────────────────────────────────────────────────────────────────
ARQYON_API void get_env_var(const char* name, char* out, int out_len) {
    DWORD result = GetEnvironmentVariableA(name, out, (DWORD)out_len);
    if (result == 0)
        strncpy_s(out, out_len, "(not set)", _TRUNCATE);
}
