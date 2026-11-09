# Building the agent payload inside the container

`Agent/` is a Windows implant that has to be cross-compiled on Linux: it includes `<windows.h>`,
`<metahost.h>`, `#include "mscorlib.h"` (the header generated from `mscorlib.tlb`) and names its
import libraries with MSVC `#pragma comment(lib, …)`. None of that works with the MSVC toolchain
alone on a container, and none of it works with MinGW-w64 out of the box either — five gaps are
listed below with their fixes. All five are now closed in-tree, so the TeamServer image can build a
payload without any host toolchain.

## What the image carries

`TeamServer/Dockerfile`, runtime stage: `clang`, `lld`, `cmake`, `ninja-build`,
`g++-mingw-w64-x86-64`, `mingw-w64-tools`. The cross build itself is described by
`Agent/cmake/toolchains/mingw-w64-clang.cmake`.

## Selecting the toolchain

There is one agent tree, `Agent/`; this change is a toolchain change, not a second source.

| Key                             | Values                                            | Default                  |
| ------------------------------- | ------------------------------------------------- | ------------------------ |
| `PayloadBuilder:Toolchains:x64` | `mingw-w64-clang.cmake` (or unset)                | `mingw-w64-clang.cmake`  |
| `PayloadBuilder:Compilers:x64`  | compiler path, used only when no toolchain is set | `x86_64-w64-mingw32-g++` |

With no toolchain configured (`PayloadBuilder:Toolchains:x64` cleared) the builder passes
`-DCMAKE_CXX_COMPILER=<compiler>` exactly as before. With the shipped default,
`PayloadBuilder:Toolchains:x64=mingw-w64-clang.cmake`, it passes
`-DCMAKE_TOOLCHAIN_FILE=<agent>/cmake/toolchains/<file>` instead, and the toolchain decides the
compiler, linker and flags. Each build logs one line so a payload traces to the compiler that produced it:

```
payload build: toolchain=mingw-w64-clang.cmake arch=x64 build=/app/PayloadBuilds/<guid>
```

## The compose stack

`docker-compose.yml` brings the image up with `Agent/` mounted at `/Agent`, which is where
`CMakePayloadBuilder` looks for sources, and with `Payloads/` and `storage/` bind-mounted so a
rebuild does not discard them. Name the file: plain `docker compose` prefers `compose.yaml`, the
onion-stack sample, which references a project that is not in this tree.

```bash
docker compose -f docker-compose.yml up -d --build teamserver
curl -fsS http://localhost:5000/health
```

The schema is not created on start (see Limits): bring the database up with the server, then apply
the migrations from the host before generating a payload through the API.

```bash
docker compose -f docker-compose.yml up -d postgres
ConnectionStrings__Postgres='Host=localhost;Database=postgres;Username=postgres;Password=JAMZ' \
  dotnet ef database update --project TeamServer/TeamServer.csproj
```

`docker compose -f docker-compose.yml run --rm --entrypoint /bin/bash teamserver` gives the same
container as the `docker run` below, with the mounts already in place.

## Building by hand

Inside a container from this image, with the repository mounted:

```bash
docker run --rm --entrypoint /bin/bash -v "$PWD:/src" -w /src/Agent trinity-teamserver:latest -lc '
  cmake --preset mingw-clang-x86_64 \
    -DINPUT_HOSTS=192.168.122.20 -DINPUT_PAYLOAD_UUID=<uuid> -DPORT=5070 \
    -DINPUT_USER_AGENT="Trinity Agent Client/1.0" \
    -DINPUT_REQUEST_HEADER="Content-Type: application/json" &&
  cmake --build out/build/mingw-clang-x86_64 &&
  objdump -f out/build/mingw-clang-x86_64/Agent.exe'
```

The artifact is `Agent/out/build/mingw-clang-x86_64/Agent.exe`, a PE32+ x86-64 executable.
`Agent/out/build/` is already gitignored.

## The five MinGW gaps and their fixes

1. **`metahost.h` does not exist in MinGW-w64.** `src/Agent.cpp` includes it for `ICLRMetaHost` /
   `ICLRRuntimeInfo` and calls `CLRCreateInstance`. `cmake/compat/mingw/metahost.h` declares both
   interfaces in SDK method order with the documented GUIDs, and resolves `CLRCreateInstance` from
   `mscoree.dll` at runtime. It is added to the include path by the toolchain file only, so an MSVC
   build still sees the SDK header.
2. **No `mscoree` import library.** MinGW ships no `libmscoree.a`, so the sources'
   `#pragma comment(lib, "mscoree.lib")` cannot be honoured. The compat header resolves the one
   symbol it needs at runtime instead.
3. **`include/mscorlib.h` names an interface `_Nullable`,** which clang treats as a nullability
   keyword (`error: expected unqualified-id`). The compat header `#define`s it to a legal identifier
   for the translation unit; the generated header stays unmodified.
4. **`__uuidof(mscorlib::_AppDomain)` cannot be resolved by MinGW** for a type it did not declare
   with `__declspec(uuid)`: `src/Agent.cpp` uses `IID_PPV_ARGS(&AppDomain)`, which mangles to an
   undefined `__mingw_uuidof<…>` symbol. The compat header adds a `__CRT_UUID_DECL` with the IID
   taken from `include/mscorlib.h` itself (`05F696DC-2B29-3663-AD8B-C4389CF2A713`).
5. **MSVC `#pragma comment(lib, …)` is inert under clang.** `Agent/CMakeLists.txt` names the same
   import libraries in a `if(MINGW)` block: winhttp, iphlpapi, psapi, shlwapi, user32, ole32,
   oleaut32, uuid, ws2_32, mstask, taskschd, comctl32, advapi32, shell32. `-fms-extensions` is
   deliberately _not_ used: it would make clang act on the `mscoree.lib` pragma and fail.

## Limits

- x64 only: the toolchain targets `x86_64-w64-windows-gnu`; there is no `x86` toolchain file.
- The payload is built, not executed here; nothing in this page claims a run on a target.
- The TeamServer image does not apply database migrations on start; a stack that also runs the
  server still needs the schema created (`dotnet ef database update`), which this change does not
  alter.
