cmake_minimum_required(VERSION 3.20)
project(arqyon_core CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# ── Sources ──────────────────────────────────────────────────
set(SOURCES
    src/exports.cpp
    src/system_info.cpp
    src/dev_tools.cpp
    src/file_utils.cpp
    src/diagnostics.cpp
    src/py_runner.cpp
)

# ── DLL Target ───────────────────────────────────────────────
add_library(arqyon_core SHARED ${SOURCES})

target_include_directories(arqyon_core PRIVATE include)

target_compile_definitions(arqyon_core PRIVATE ARQYON_CORE_EXPORTS)

# ── Windows Libraries ────────────────────────────────────────
if(WIN32)
    target_link_libraries(arqyon_core PRIVATE
        bcrypt      # CNG hashing
        ws2_32      # Winsock (network diag)
    )
    target_compile_definitions(arqyon_core PRIVATE
        WIN32_LEAN_AND_MEAN
        NOMINMAX
        _CRT_SECURE_NO_WARNINGS
    )
endif()

# ── Optional: Python C API ───────────────────────────────────
# Uncomment and adjust path to enable embedded Python:
#
# find_package(Python3 COMPONENTS Development)
# if(Python3_FOUND)
#     target_include_directories(arqyon_core PRIVATE ${Python3_INCLUDE_DIRS})
#     target_link_libraries(arqyon_core PRIVATE ${Python3_LIBRARIES})
#     target_compile_definitions(arqyon_core PRIVATE ARQYON_HAS_PYTHON)
# endif()

# ── Output: place DLL next to the C# shell binary ────────────
set_target_properties(arqyon_core PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib"
)
