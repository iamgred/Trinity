# Evidence — agent payload cross-build

Recorded 2026-11-09 by `dev` on the lab host. Runner: `dotnet 10.0.403`, Docker 27.x, image built from
this branch's `TeamServer/Dockerfile` (`sha256:a0c9491a1f07…`, built 2026-11-09T20:09+02:00).

## Base branches

| commit    | what it is                                 | `dotnet build TeamServer/TeamServer.slnx -c Release`                                                                                                                                                                                            | `dotnet test TeamServer.Tests`                                       |
| --------- | ------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| `9d51428` | upstream `test` at the time of the request | **FAILED** — 7 errors, all `Client/`: `GeneratePayloadDialog.razor` (2 × `CS0029` string→enum, 1 × `CS0117` `OutputFormat`), `Interact.razor` (2 × `CS1061` `ProcessRawInput`/`GetCommandEndpoint`), `Deployment.razor` (2 × the same `CS1061`) | **does not compile** — 9 × `CS0234` (`TeamServer.Interface` missing) |
| `3d470cb` | `autoskid` base, current upstream `test`   | **succeeded** — 0 errors, 19 warnings                                                                                                                                                                                                           | **does not compile** — 6 × `CS0234` (`TeamServer.Interface` missing) |

So the base branch builds as a solution but its test project does not compile; a PR against it cannot
show a green test signal, and this change does not repair the tests.

## Build and tests on this branch

```
dotnet build TeamServer/TeamServer.slnx --configuration Release   -> succeeded, 0 errors, 19 warnings
dotnet test  TeamServer.Tests/TeamServer.Tests.csproj             -> fails to compile, 6 x CS0234 (base defect)
```

## Payload built in-container

Command, in a container from this branch's image with the repository mounted:

```
docker run --rm --entrypoint /bin/bash -v "$PWD:/src" -w /src/Agent <image> -lc '
  cmake --preset mingw-clang-x86_64 \
    -DINPUT_HOSTS=192.168.122.20 -DINPUT_PAYLOAD_UUID=<uuid> -DPORT=5070 \
    -DINPUT_USER_AGENT="Trinity Agent Client/1.0" \
    -DINPUT_REQUEST_HEADER="Content-Type: application/json" &&
  cmake --build out/build/mingw-clang-x86_64'
```

```
CONFIGURE=0   BUILD=0
-rwxr-xr-x 1 root root 1142272 out/build/mingw-clang-x86_64/Agent.exe
out/build/mingw-clang-x86_64/Agent.exe:     file format pei-x86-64
Magic			020b	(PE32+)
sha256 f8f3f1f2c8f8b7e3da84e8e8f5177b3d9ec66e453632fb14d1505cc5327c96f9
```

That is the single `Agent/` tree with `toolchain=mingw-w64-clang.cmake`; the server logs the same
per build: `payload build: toolchain=mingw-w64-clang.cmake arch=x64 build=…`.

The digest is stable across rebuilds because the toolchain links with `-Wl,--no-insert-timestamp`;
two consecutive clean builds of the same tree produced the same bytes and the same
`f8f3f1f2…` digest. Without that flag the COFF header carries the build clock and every build
differs, which is why size and `pei-x86-64`/`PE32+` are the comparison for older builds.

## The pre-existing compiler path, measured

`PayloadBuilder:Compilers:x64` (default `x86_64-w64-mingw32-g++`) is passed as
`-DCMAKE_CXX_COMPILER` with no toolchain file. In the container that configure has no Windows system
name, so CMake does not set `MINGW`, the compat include is not applied, and the build stops at:

```
/src/Agent/src/Agent.cpp:16:10: fatal error: metahost.h: No such file or directory
```

For completeness: adding `-DCMAKE_SYSTEM_NAME=Windows -DCMAKE_C_COMPILER=x86_64-w64-mingw32-gcc` to
the same invocation does build (2,141,316 B, `pei-x86-64`), which is why the toolchain file — not the
bare compiler path — is the supported way to cross-compile. The `Compilers` key is untouched and still
takes effect when no toolchain is configured.

## Limits

- x64 only; `mingw-w64-clang.cmake` targets `x86_64-w64-windows-gnu`.
- The payload is built, not executed; no agent run is claimed.
- The tests do not compile on the base commit; that is reported, not fixed, here.
