#pragma once
#include <string>

namespace arqyon::system {
    std::string GetCpuInfoJson();
    std::string GetRamInfoJson();
    std::string GetOsInfoJson();
    std::string GetDiskInfoJson();
}
