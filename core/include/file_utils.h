#pragma once
#include <string>

namespace arqyon::file {
    std::string HashSHA256(const std::string& path);
    std::string HashMD5(const std::string& path);
    std::string GetFileInfoJson(const std::string& path);
}
