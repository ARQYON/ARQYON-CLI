#pragma once
#include <string>

namespace arqyon::python {
    int  RunScript(const std::string& scriptPath, const std::string& args);
    std::string GetVersion();
}
