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
set(INPUT_PAYLOAD_UUID "0c71632c-217f-46ce-bb81-dbfd32dd18ac" CACHE STRING "The payload unique identifier")
string(STRIP "${INPUT_PAYLOAD_UUID}" INPUT_PAYLOAD_UUID)

# Port Configuration
set(PORT 5069 CACHE STRING "The primary target connection port")
string(STRIP "${PORT}" CLEAN_PORT)

# User Agent Configuration
set(INPUT_USER_AGENT "Trinity Agent Client 1.0" CACHE STRING "The user agent for requests")
set(WIDE_USER_AGENT L${INPUT_USER_AGENT} INPUT_USER_AGENT )

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
target_compile_definitions(Agent PRIVATE 
    COMM_HOSTS_LIST=${WIDE_HOSTS_LIST}
    COMM_PAYLOAD_UUID=${INPUT_PAYLOAD_UUID}
    COMM_PORT=${CLEAN_PORT}
    COMM_USER_AGENT=${WIDE_USER_AGENT}
)

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET Agent PROPERTY CXX_STANDARD 20)
endif()

# TODO: Add tests and install targets if needed.
