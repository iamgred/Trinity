# syntax=docker/dockerfile:1
# Multi-stage Dockerfile for Trinity TeamServer
# Builds the .NET 10 API + mingw cross-compiler for agent payloads

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore from project files so source-only edits reuse the NuGet restore layer.
COPY TeamServer/TeamServer.csproj TeamServer/
COPY Trinity.Shared/Trinity.Shared.csproj Trinity.Shared/
RUN dotnet restore TeamServer/TeamServer.csproj

# Copy source and build
COPY Trinity.Shared/ Trinity.Shared/
COPY TeamServer/ TeamServer/
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish TeamServer/TeamServer.csproj -c $BUILD_CONFIGURATION -o /app/publish \
    --no-restore /p:UseAppHost=true

# Build migrations separately so runtime images do not include EF tooling.
FROM build AS migrator-build
RUN dotnet tool install --global dotnet-ef --version 10.0.12
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations bundle --project TeamServer/TeamServer.csproj --self-contained -r linux-x64 -o /app/efbundle

# ---- Migrator stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS migrator
WORKDIR /app
COPY --from=migrator-build /app/efbundle .
ENTRYPOINT [ "./efbundle" ]

# ---- Runtime base stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime-base
WORKDIR /app

# Install curl for healthcheck and mingw + cmake for runtime agent compilation
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl g++-mingw-w64-x86-64 mingw-w64-tools cmake ninja-build \
    && rm -rf /var/lib/apt/lists/*

# Agent source + payload output
RUN mkdir -p /Agent /app/Payloads

EXPOSE 5000

# ---- Runtime stage ----
FROM runtime-base AS runtime
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TeamServer.dll"]

# Debug-only image target; normal runtime images do not include vsdbg.
FROM runtime-base AS debug
RUN apt-get update && apt-get install -y --no-install-recommends curl unzip \
    && rm -rf /var/lib/apt/lists/* \
    && curl --fail --location --output /tmp/vsdbg.zip \
        https://download.visualstudio.microsoft.com/download/pr/dffb86a2-891d-47e3-80bd-fb847397c759/coreclr-debug-linux-x64.zip \
    && echo "A4C93FB850211AE2138FD8FD1DACDBD2571D65D258A357135740F619DA14E08D  /tmp/vsdbg.zip" | sha256sum --check - \
    && mkdir -p /vsdbg \
    && unzip -q /tmp/vsdbg.zip -d /vsdbg \
    && chmod 755 /vsdbg/vsdbg /vsdbg/vsdbg-ui \
    && rm /tmp/vsdbg.zip
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TeamServer.dll"]
