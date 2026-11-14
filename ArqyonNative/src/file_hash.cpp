#define ARQYON_EXPORTS
#include "../include/arqyon_native.h"

#include <windows.h>
#include <wincrypt.h>
#include <cstdio>
#include <cstring>

#pragma comment(lib, "advapi32.lib")

static int hash_file(const char* filepath, char* out, int out_len, ALG_ID alg_id, int hash_bytes) {
    HCRYPTPROV hProv = 0;
    HCRYPTHASH hHash = 0;
    HANDLE hFile     = INVALID_HANDLE_VALUE;
    BYTE buf[4096];
    DWORD bytesRead;
    int result = 0;

    hFile = CreateFileA(filepath, GENERIC_READ, FILE_SHARE_READ,
                        nullptr, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, nullptr);
    if (hFile == INVALID_HANDLE_VALUE) return -1;

    if (!CryptAcquireContextA(&hProv, nullptr, nullptr, PROV_RSA_AES, CRYPT_VERIFYCONTEXT)) {
        CloseHandle(hFile); return -1;
    }
    if (!CryptCreateHash(hProv, alg_id, 0, 0, &hHash)) {
        CryptReleaseContext(hProv, 0); CloseHandle(hFile); return -1;
    }

    while (ReadFile(hFile, buf, sizeof(buf), &bytesRead, nullptr) && bytesRead > 0) {
        if (!CryptHashData(hHash, buf, bytesRead, 0)) { result = -1; break; }
    }

    if (result == 0) {
        DWORD sz = (DWORD)hash_bytes;
        BYTE* hash_buf = new BYTE[hash_bytes];
        if (CryptGetHashParam(hHash, HP_HASHVAL, hash_buf, &sz, 0)) {
            char* ptr = out;
            for (int i = 0; i < hash_bytes && (ptr - out + 3) < out_len; i++) {
                ptr += snprintf(ptr, 3, "%02x", hash_buf[i]);
            }
            *ptr = '\0';
        }
        delete[] hash_buf;
    }

    CryptDestroyHash(hHash);
    CryptReleaseContext(hProv, 0);
    CloseHandle(hFile);
    return result;
}

ARQYON_API int hash_md5(const char* filepath, char* out, int out_len) {
    return hash_file(filepath, out, out_len, CALG_MD5, 16);
}

ARQYON_API int hash_sha256(const char* filepath, char* out, int out_len) {
    return hash_file(filepath, out, out_len, CALG_SHA_256, 32);
}
