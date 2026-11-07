// ============================================================
//  ARQYON CORE — py_runner.cpp
//  Runs Python scripts via the embedded Python C API.
//  Requires Python 3 dev headers and libs at build time.
//  Link: python3x.lib (Windows) or -lpython3.x (Linux/Mac)
// ============================================================

#include "py_runner.h"

// Python.h must come before any system headers to avoid conflicts
#ifdef ARQYON_HAS_PYTHON
  #include <Python.h>
#endif

#include <string>
#include <sstream>

namespace arqyon::python {

std::string GetVersion() {
#ifdef ARQYON_HAS_PYTHON
    if (!Py_IsInitialized()) Py_Initialize();
    std::string ver = Py_GetVersion();
    // Trim to first line only
    size_t nl = ver.find('\n');
    if (nl != std::string::npos) ver = ver.substr(0, nl);
    return ver;
#else
    return "Python support not compiled in";
#endif
}

int RunScript(const std::string& scriptPath, const std::string& args) {
#ifdef ARQYON_HAS_PYTHON
    if (scriptPath.empty()) return -1;

    if (!Py_IsInitialized()) Py_Initialize();

    // Build sys.argv
    std::wstring wScript(scriptPath.begin(), scriptPath.end());
    wchar_t* argv0 = const_cast<wchar_t*>(wScript.c_str());
    PySys_SetArgvEx(1, &argv0, 0);

    // Open and run the script file
    FILE* f = nullptr;
#ifdef _WIN32
    fopen_s(&f, scriptPath.c_str(), "r");
#else
    f = fopen(scriptPath.c_str(), "r");
#endif
    if (!f) return -2;

    int result = PyRun_SimpleFile(f, scriptPath.c_str());
    fclose(f);

    if (PyErr_Occurred()) {
        PyErr_Print();
        PyErr_Clear();
    }

    return result; // 0 = success, -1 = error
#else
    // Fallback: shell out to system python
    std::string cmd = "python \"" + scriptPath + "\"";
    if (!args.empty()) cmd += " " + args;
    return system(cmd.c_str());
#endif
}

} // namespace arqyon::python
