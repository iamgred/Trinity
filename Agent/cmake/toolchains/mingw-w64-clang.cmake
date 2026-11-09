# ========================================== { START OF FILE } ========================================== #
# mingw-w64-clang.cmake — Clang/LLD toolchain for cross-compiling Agent/ to Windows x86_64.
#
# Selected by PayloadBuilder:Toolchains:<arch> (see TeamServer/Services/CMakePayloadBuilder.cs).
# It drives Clang with LLD against the MinGW-w64 ABI,
# (different front-end codegen, different linker, different CRT/import layout) from the GCC
# toolchain (mingw-w64-x86_64.cmake), which remains available as the alternative. Behaviour is
# identical: same wire protocol, same hashed API resolution. Static signatures that key on
# GCC/MinGW codegen or GNU ld's PE layout no longer match.
#
# Clang/LLD also emit CodeView + a PDB in Debug/RelWithDebInfo builds (see the config block at
# the end of this file), which is what makes the payload debuggable with cdb/WinDbg.
#
# Clang auto-detects the mingw-w64 installation (Debian/Ubuntu), so no --sysroot or
# --gcc-toolchain flags are needed (passing them actually breaks header discovery):
#   - CRT/Windows headers under /usr/x86_64-w64-mingw32/include
#   - libstdc++ under /usr/lib/gcc/x86_64-w64-mingw32/<thread-model>/include/c++/
#
# Build with:
#   cmake --preset mingw-clang-x86_64
#   cmake --build out/build/mingw-clang-x86_64
# Inside the TeamServer image (see docs/agent-cross-compile.md) the same file is passed with
# -DCMAKE_TOOLCHAIN_FILE by the payload builder.
# ========================================== { END OF FILE } ========================================== #

set(CMAKE_SYSTEM_NAME Windows)
set(CMAKE_SYSTEM_PROCESSOR x86_64)

# Don't search for programs *on the target* (there is none). Set before find_program so
# the compiler lookup below is unaffected by the "ONLY" root-path modes.
set(CMAKE_C_FIND_ROOT_PATH /usr/x86_64-w64-mingw32)
set(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)
set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE ONLY)

# Resolve the compiler version-portably: Debian ships clang-19, Ubuntu ships clang (18).
# Probed in this order so a pinned newer toolchain wins when both exist.
find_program(AGENT_CLANG_C NAMES clang-19 clang REQUIRED)
find_program(AGENT_CLANG_CXX NAMES clang++-19 clang++ REQUIRED)
set(CMAKE_C_COMPILER "${AGENT_CLANG_C}")
set(CMAKE_CXX_COMPILER "${AGENT_CLANG_CXX}")

# Cross-compile to Windows using the MinGW-w64 (Windows GNU) ABI.
set(CMAKE_C_COMPILER_TARGET x86_64-w64-windows-gnu)
set(CMAKE_CXX_COMPILER_TARGET x86_64-w64-windows-gnu)

# Self-contained PE; LLD links, no mingw runtime DLLs required on the target.
# --no-insert-timestamp keeps the COFF header free of the build clock, so two builds of the same
# sources produce the same bytes and the digest in the documentation is checkable.
set(CMAKE_EXE_LINKER_FLAGS_INIT "-fuse-ld=lld -static -Wl,--no-insert-timestamp")

# Release/MinSizeRel: whole-program LTO via LLD. Cross-translation-unit inlining +
# dead-code elimination reshape the call graph and eliminate the per-TU codegen
# boundaries that static signatures rely on. LLD/COFF supports LTO for the MinGW target.
# `-s` drops the symbol table and debug sections (~350 KB) the mingw CRT objects carry.
set(CMAKE_C_FLAGS_RELEASE_INIT "-flto")
set(CMAKE_CXX_FLAGS_RELEASE_INIT "-flto")
set(CMAKE_C_FLAGS_MINSIZEREL_INIT "-flto")
set(CMAKE_CXX_FLAGS_MINSIZEREL_INIT "-flto")
set(CMAKE_EXE_LINKER_FLAGS_RELEASE_INIT "-flto -s")
set(CMAKE_EXE_LINKER_FLAGS_MINSIZEREL_INIT "-flto -s")

# Debug/RelWithDebInfo: no LTO (it obscures line info), frame pointers kept, and CodeView
# debug info + a PDB emitted next to the PE so cdb/WinDbg can debug it and symbolise crash
# dumps (GCC can only emit DWARF, hence gdb for that toolchain). Debug builds are for the
# guest debugger only — the TeamServer's payload builder always configures Release.
set(CMAKE_C_FLAGS_DEBUG_INIT "-g -gcodeview -O0 -fno-omit-frame-pointer")
set(CMAKE_CXX_FLAGS_DEBUG_INIT "-g -gcodeview -O0 -fno-omit-frame-pointer")
set(CMAKE_C_FLAGS_RELWITHDEBINFO_INIT "-g -gcodeview -O1 -fno-omit-frame-pointer -DNDEBUG")
set(CMAKE_CXX_FLAGS_RELWITHDEBINFO_INIT "-g -gcodeview -O1 -fno-omit-frame-pointer -DNDEBUG")
set(CMAKE_EXE_LINKER_FLAGS_DEBUG_INIT "-Wl,--pdb=Agent.pdb")
set(CMAKE_EXE_LINKER_FLAGS_RELWITHDEBINFO_INIT "-Wl,--pdb=Agent.pdb")

# LLD in COFF (PE) mode rejects GNU ld's `--dependency-file` flag. CMake probes
# `ld.lld --help` in its default ELF mode, sees `--dependency-file` advertised,
# and wrongly enables linker-depfile generation for the PE target. Disable it.
set(CMAKE_C_LINKER_DEPFILE_SUPPORTED FALSE)
set(CMAKE_CXX_LINKER_DEPFILE_SUPPORTED FALSE)
set(CMAKE_C_LINK_DEPENDS_USE_LINKER FALSE)
set(CMAKE_CXX_LINK_DEPENDS_USE_LINKER FALSE)

# ---- Windows-header / MSVC-intrinsic gaps --------------------------------------------------
# 1. metahost.h: MinGW-w64 ships mscoree.h but not the CLR hosting header src/Agent.cpp includes.
#    cmake/compat/mingw/metahost.h supplies the two interfaces, their GUIDs and a CLRCreateInstance
#    resolved from mscoree.dll. It is added here rather than by CMakeLists.txt, so an MSVC build of
#    the same directory still picks up the SDK header from its own include path first.
#    -DINITGUID turns guiddef.h's DECLSPEC_SELECTANY declarations into definitions, which is what
#    lets CLSID_/IID_ symbols resolve without an mscoree import library (MinGW has none).
#    -fms-extensions is NOT used: it also makes clang act on the sources' `#pragma comment(lib,
#    "mscoree.lib")`, which fails because MinGW ships no such import library. __uuidof for the one
#    mscorlib interface this tree needs is supplied by the compat header instead.
set(CMAKE_C_FLAGS_INIT "-I${CMAKE_CURRENT_LIST_DIR}/../compat/mingw -DINITGUID")
set(CMAKE_CXX_FLAGS_INIT "-I${CMAKE_CURRENT_LIST_DIR}/../compat/mingw -DINITGUID")
# 2. Upstream carries `#pragma comment(lib, "mscoree.lib")`, an MSVC-only directive. Clang warns
#    "unknown pragma"; the symbol it would pull in is resolved at runtime instead (see above).
set(CMAKE_CXX_FLAGS_INIT "${CMAKE_CXX_FLAGS_INIT} -Wno-unknown-pragmas")
# 3. Upstream pulls its import libraries with MSVC `#pragma comment(lib, ...)`
#    (winhttp, iphlpapi, psapi, shlwapi, user32, ole32, ws2_32). Clang records nothing for that
#    pragma, so the same set is passed on the link line here. `mscoree.lib` is deliberately absent:
#    MinGW has no such import library, and the compat metahost.h resolves CLRCreateInstance from
#    mscoree.dll at runtime instead.
# (The import libraries are attached with target_link_libraries in CMakeLists.txt: CMake's
#  Windows-GNU platform module overwrites CMAKE_<LANG>_STANDARD_LIBRARIES after the toolchain file
#  is read, so setting it here had no effect — the first link failed on WinHttpOpen.)
