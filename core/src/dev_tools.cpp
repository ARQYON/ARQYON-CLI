// ============================================================
//  ARQYON CORE — dev_tools.cpp
//  Developer environment detection.
//  Probes PATH for known runtimes and returns JSON.
// ============================================================

#include "dev_tools.h"

#ifdef _WIN32
#include <windows.h>
#endif

#include <sstream>
#include <string>
#include <vector>
#include <array>
#include <cstdio>

namespace arqyon::dev {

// ── Helpers ──────────────────────────────────────────────────

static std::string EscapeJson(const std::string& s) {
    std::string out;
    for (char c : s) {
        if (c == '"')       out += "\\\"";
        else if (c == '\\') out += "\\\\";
        else if (c == '\n') out += "\\n";
        else if (c == '\r') {}
        else                out += c;
    }
    return out;
}

// Run a command and capture one line of stdout
static std::string RunCapture(const std::string& cmd) {
#ifdef _WIN32
    std::string full = cmd + " 2>NUL";
    FILE* pipe = _popen(full.c_str(), "r");
#else
    FILE* pipe = popen((cmd + " 2>/dev/null").c_str(), "r");
#endif
    if (!pipe) return "";
    char buf[256] = {};
    std::string result;
    while (fgets(buf, sizeof(buf), pipe)) result += buf;
#ifdef _WIN32
    _pclose(pipe);
#else
    pclose(pipe);
#endif
    // Trim trailing whitespace
    while (!result.empty() && (result.back() == '\n' ||
           result.back() == '\r' || result.back() == ' '))
        result.pop_back();
    return result;
}

// Find an executable on PATH, return its full path or empty
static std::string WhichExe(const std::string& name) {
#ifdef _WIN32
    std::string cmd = "where " + name;
    std::string result = RunCapture(cmd);
    // "where" may return multiple lines — take the first
    size_t nl = result.find('\n');
    if (nl != std::string::npos) result = result.substr(0, nl);
    while (!result.empty() && (result.back() == '\r' || result.back() == '\n' || result.back() == ' '))
        result.pop_back();
    return result;
#else
    return RunCapture("which " + name);
#endif
}

struct LangProbe {
    std::string name;
    std::string exe;
    std::string versionCmd; // full command to get version line
    std::string versionArg; // arg to append to exe for version
};

// ── Language Detection ───────────────────────────────────────

std::string DetectLanguagesJson() {
    // Each entry: display name, executable, version flag
    struct Entry { std::string name, exe, flag; };
    std::vector<Entry> probes = {
        { "Python",     "python",    "--version"  },
        { "Python 3",   "python3",   "--version"  },
        { "Node.js",    "node",      "--version"  },
        { "npm",        "npm",       "--version"  },
        { "Bun",        "bun",       "--version"  },
        { "Rust",       "rustc",     "--version"  },
        { "Cargo",      "cargo",     "--version"  },
        { "Go",         "go",        "version"    },
        { "Java",       "java",      "-version"   },
        { "Kotlin",     "kotlinc",   "-version"   },
        { "GCC",        "gcc",       "--version"  },
        { "Clang",      "clang",     "--version"  },
        { "MSVC",       "cl",        ""           },
        { "C#/.NET",    "dotnet",    "--version"  },
        { "PHP",        "php",       "--version"  },
        { "Ruby",       "ruby",      "--version"  },
        { "Swift",      "swift",     "--version"  },
        { "Deno",       "deno",      "--version"  },
        { "Perl",       "perl",      "--version"  },
        { "PowerShell", "pwsh",      "--version"  },
        { "Git",        "git",       "--version"  },
        { "Docker",     "docker",    "--version"  },
        { "kubectl",    "kubectl",   "version --client --short" },
    };

    std::ostringstream json;
    json << "[";
    bool first = true;

    for (auto& p : probes) {
        std::string path = WhichExe(p.exe);
        if (path.empty()) continue;

        std::string version;
        if (!p.flag.empty()) {
            std::string cmd = p.exe + " " + p.flag;
            version = RunCapture(cmd);
            // Grab only first line, strip exe prefix if present
            size_t nl = version.find('\n');
            if (nl != std::string::npos) version = version.substr(0, nl);
            // Some tools prefix with the exe name, clean up
            if (version.rfind(p.exe, 0) == 0) {
                size_t sp = version.find(' ');
                if (sp != std::string::npos) version = version.substr(sp + 1);
            }
            while (!version.empty() && (version.back() == '\r' || version.back() == ' '))
                version.pop_back();
        }

        if (!first) json << ",";
        first = false;
        json << "{"
             << "\"name\":\""    << EscapeJson(p.name)    << "\","
             << "\"version\":\"" << EscapeJson(version)    << "\","
             << "\"path\":\""    << EscapeJson(path)       << "\""
             << "}";
    }

    json << "]";
    return json.str();
}

// ── PATH Scan ────────────────────────────────────────────────

std::string ScanEnvPathJson() {
    std::string pathVar;

#ifdef _WIN32
    char buf[32768] = {};
    DWORD sz = GetEnvironmentVariableA("PATH", buf, sizeof(buf));
    if (sz > 0) pathVar = buf;
    char delimiter = ';';
#else
    const char* p = getenv("PATH");
    if (p) pathVar = p;
    char delimiter = ':';
#endif

    std::ostringstream json;
    json << "[";
    bool first = true;
    std::string token;
    std::istringstream stream(pathVar);
    while (std::getline(stream, token, delimiter)) {
        if (token.empty()) continue;
        if (!first) json << ",";
        first = false;
        json << "\"" << EscapeJson(token) << "\"";
    }
    json << "]";
    return json.str();
}

} // namespace arqyon::dev
