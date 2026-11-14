#define ARQYON_EXPORTS
#include "../include/arqyon_native.h"

#include <cstdio>
#include <cstring>
#include <array>
#include <string>

// Helper: run a command and capture its first line of output
static std::string run_command(const char* cmd) {
    std::string result;
    char buf[256] = {};
    FILE* pipe = _popen(cmd, "r");
    if (!pipe) return "not found";
    if (fgets(buf, sizeof(buf), pipe)) {
        result = buf;
        // Strip trailing newline/CR
        while (!result.empty() && (result.back() == '\n' || result.back() == '\r'))
            result.pop_back();
    }
    _pclose(pipe);
    return result.empty() ? "not found" : result;
}

ARQYON_API void get_python_version(char* buf, int len) {
    auto v = run_command("python --version 2>&1");
    strncpy_s(buf, len, v.c_str(), _TRUNCATE);
}

ARQYON_API void get_node_version(char* buf, int len) {
    auto v = run_command("node --version 2>&1");
    strncpy_s(buf, len, v.c_str(), _TRUNCATE);
}

ARQYON_API void get_git_version(char* buf, int len) {
    auto v = run_command("git --version 2>&1");
    strncpy_s(buf, len, v.c_str(), _TRUNCATE);
}

ARQYON_API void get_cmake_version(char* buf, int len) {
    auto v = run_command("cmake --version 2>&1");
    // cmake outputs a multiline header; just take first line
    strncpy_s(buf, len, v.c_str(), _TRUNCATE);
}

ARQYON_API void get_dotnet_version(char* buf, int len) {
    auto v = run_command("dotnet --version 2>&1");
    strncpy_s(buf, len, v.c_str(), _TRUNCATE);
}

ARQYON_API void get_gcc_version(char* buf, int len) {
    auto v = run_command("gcc --version 2>&1");
    strncpy_s(buf, len, v.c_str(), _TRUNCATE);
}

ARQYON_API void get_clang_version(char* buf, int len) {
    auto v = run_command("clang --version 2>&1");
    strncpy_s(buf, len, v.c_str(), _TRUNCATE);
}
