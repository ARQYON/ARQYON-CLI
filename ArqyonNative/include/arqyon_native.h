#pragma once

#ifdef _WIN32
  #ifdef ARQYON_EXPORTS
    #define ARQYON_API __declspec(dllexport)
  #else
    #define ARQYON_API __declspec(dllimport)
  #endif
#else
  #define ARQYON_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

// ─── System Info ────────────────────────────────────────────────────────────
ARQYON_API void  get_os_name(char* buf, int len);
ARQYON_API void  get_cpu_name(char* buf, int len);
ARQYON_API long  get_total_ram_mb();
ARQYON_API long  get_free_ram_mb();
ARQYON_API void  get_uptime(char* buf, int len);
ARQYON_API void  get_hostname(char* buf, int len);
ARQYON_API void  get_username(char* buf, int len);
ARQYON_API int   get_cpu_core_count();

// ─── Disk Info ───────────────────────────────────────────────────────────────
ARQYON_API long  get_disk_total_gb(const char* drive);
ARQYON_API long  get_disk_free_gb(const char* drive);

// ─── Dev Environment ─────────────────────────────────────────────────────────
ARQYON_API void  get_python_version(char* buf, int len);
ARQYON_API void  get_node_version(char* buf, int len);
ARQYON_API void  get_git_version(char* buf, int len);
ARQYON_API void  get_cmake_version(char* buf, int len);
ARQYON_API void  get_dotnet_version(char* buf, int len);
ARQYON_API void  get_gcc_version(char* buf, int len);
ARQYON_API void  get_clang_version(char* buf, int len);

// ─── File Hashing ────────────────────────────────────────────────────────────
ARQYON_API int   hash_md5(const char* filepath, char* out, int out_len);
ARQYON_API int   hash_sha256(const char* filepath, char* out, int out_len);

// ─── Diagnostics ─────────────────────────────────────────────────────────────
ARQYON_API int   check_internet(const char* host);
ARQYON_API void  get_env_var(const char* name, char* out, int out_len);

#ifdef __cplusplus
}
#endif
