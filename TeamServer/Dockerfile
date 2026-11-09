# syntax=docker/dockerfile:1
# Multi-stage Dockerfile for the Trinity TeamServer.
# The runtime stage also carries the cross-compiler the payload builder shells out to
# (TeamServer/Services/CMakePayloadBuilder.cs), so a payload can be built inside the container.

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore from the project files so source-only edits reuse the NuGet layer.
COPY TeamServer/TeamServer.csproj TeamServer/
COPY Trinity.Shared/Trinity.Shared.csproj Trinity.Shared/
RUN dotnet restore TeamServer/TeamServer.csproj

COPY Trinity.Shared/ Trinity.Shared/
COPY TeamServer/ TeamServer/
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish TeamServer/TeamServer.csproj -c $BUILD_CONFIGURATION -o /app/publish \
    --no-restore /p:UseAppHost=true

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl for the healthcheck; the rest is the agent toolchain (clang + lld + MinGW-w64 + cmake/ninja).
# PayloadBuilder:Toolchains:x64 selects Agent/cmake/toolchains/mingw-w64-clang.cmake, which needs
# all of them. Any new agent build dependency belongs in this stage as well.
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl cmake ninja-build clang lld g++-mingw-w64-x86-64 mingw-w64-tools \
    && rm -rf /var/lib/apt/lists/*

# Agent sources are mounted at /Agent by the compose file; payload output lands in /app/Payloads.
RUN mkdir -p /Agent /app/Payloads

COPY --from=build /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "TeamServer.dll"]
