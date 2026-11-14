cmake_minimum_required(VERSION 3.20)
project(ArqyonNative VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# ─── Sources ──────────────────────────────────────────────────────────────────
set(SOURCES
    src/sys_info.cpp
    src/disk_info.cpp
    src/dev_detect.cpp
    src/file_hash.cpp
    src/diagnostics.cpp
)

# ─── Shared DLL ───────────────────────────────────────────────────────────────
add_library(ArqyonNative SHARED ${SOURCES})

target_include_directories(ArqyonNative PRIVATE include)

target_compile_definitions(ArqyonNative PRIVATE ARQYON_EXPORTS)

# Windows-specific system libraries
if(WIN32)
    target_link_libraries(ArqyonNative PRIVATE
        advapi32   # Registry, CryptAPI
        ws2_32     # Winsock
    )
endif()

# Output the DLL next to where the C# project can find it
set_target_properties(ArqyonNative PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/../ArqyonCLI/bin"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/../ArqyonCLI/bin"
)
