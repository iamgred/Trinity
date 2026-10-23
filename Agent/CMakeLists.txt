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

target_include_directories(Agent PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/external)
target_include_directories(Agent PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include)
target_include_directories(Agent PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/src)

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET Agent PROPERTY CXX_STANDARD 20)
endif()

# TODO: Add tests and install targets if needed.
