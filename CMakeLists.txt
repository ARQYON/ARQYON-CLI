cmake_minimum_required(VERSION 3.20)
project(ARQYON VERSION 0.1.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Build the C++ core DLL
add_subdirectory(core)

# ── Post-build: copy DLL next to C# output ───────────────────
# Adjust the destination path to match your dotnet build output.
# Example for Debug builds:
#
# add_custom_command(TARGET arqyon_core POST_BUILD
#     COMMAND ${CMAKE_COMMAND} -E copy_if_different
#         $<TARGET_FILE:arqyon_core>
#         "${CMAKE_SOURCE_DIR}/shell/bin/Debug/net8.0/arqyon_core.dll"
#     COMMENT "Copying arqyon_core.dll to shell output"
# )
