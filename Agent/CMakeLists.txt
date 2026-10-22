# CMakeList.txt : CMake project for Agent, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.10)

# Enable Hot Reload for MSVC compilers if supported.
if (POLICY CMP0141)
  cmake_policy(SET CMP0141 NEW)
  set(CMAKE_MSVC_DEBUG_INFORMATION_FORMAT "$<IF:$<AND:$<C_COMPILER_ID:MSVC>,$<CXX_COMPILER_ID:MSVC>>,$<$<CONFIG:Debug,RelWithDebInfo>:EditAndContinue>,$<$<CONFIG:Debug,RelWithDebInfo>:ProgramDatabase>>")
endif()

project (Agent)

# Export compile_commands.json for clangd / clang-tidy
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

# Add source to this project's executable.
add_executable (
  Agent 
  "src/main.cpp" 
  "src/CommManager.cpp"
  "include/CommManager.h"
  "include/base64.hpp"
  "src/CommandDispatcher.cpp"
  "include/CommandDispatcher.h"
  "src/Agent.cpp"
  "include/Agent.h"
  "src/TaskQueueManager.cpp"
  "include/TaskQueueManager.h"
)

if(WIN32)
    target_link_libraries(Agent PRIVATE
        winhttp    # WinHttpOpen/Connect/OpenRequest/SendRequest/...
        iphlpapi   # GetAdaptersAddresses
        ws2_32     # WSAStartup/WSACleanup/getaddrinfo/inet_ntop
        psapi      # GetModuleBaseNameA
    )
endif()

target_include_directories(Agent PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/external)
target_include_directories(Agent PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include)
target_include_directories(Agent PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/src)

# --- Code formatting & static analysis targets -------------------------------
# Usage:
#   cmake --build build/Agent --target format   # format all sources in place
#   cmake --build build/Agent --target tidy     # run clang-tidy (needs a configured build)
file(GLOB_RECURSE AGENT_FORMAT_SOURCES
    ${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/include/*.h
    ${CMAKE_CURRENT_SOURCE_DIR}/include/*.hpp)

find_program(CLANG_FORMAT clang-format)
if(CLANG_FORMAT)
    add_custom_target(format
        COMMAND ${CLANG_FORMAT} -i ${AGENT_FORMAT_SOURCES}
        COMMENT "Running clang-format on Agent sources"
        VERBATIM)
endif()

find_program(CLANG_TIDY clang-tidy)
if(CLANG_TIDY)
    add_custom_target(tidy
        COMMAND ${CLANG_TIDY} -p ${CMAKE_BINARY_DIR} ${AGENT_FORMAT_SOURCES}
        COMMENT "Running clang-tidy on Agent sources"
        VERBATIM)
endif()

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET Agent PROPERTY CXX_STANDARD 20)
endif()

option(HOST "TeamServer host" "")
option(PAYLOAD_UUID "Payload UUID" "")
if(DEFINED HOST)
    target_compile_definitions(Agent PRIVATE HOST="${HOST}")
endif()
if(DEFINED PAYLOAD_UUID)
    target_compile_definitions(Agent PRIVATE PAYLOAD_UUID="${PAYLOAD_UUID}")
endif()

# TODO: Add tests and install targets if needed.
