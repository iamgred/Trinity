# syntax=docker/dockerfile:1
# Multi-stage Dockerfile for Trinity TeamServer
# Builds the .NET 10 preview API + mingw cross-compiler for agent payloads

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Install mingw cross-compiler for building Windows agent payloads
RUN apt-get update && apt-get install -y --no-install-recommends \
    g++-mingw-w64-x86-64 mingw-w64-tools \
    && rm -rf /var/lib/apt/lists/*

# Copy project files
COPY TeamServer/TeamServer.csproj TeamServer/
COPY Trinity.Shared/ Trinity.Shared/
RUN dotnet restore TeamServer/TeamServer.csproj

# Copy source and build
COPY TeamServer/ TeamServer/
RUN dotnet publish TeamServer/TeamServer.csproj -c Release -o /app/publish \
    /p:UseAppHost=true

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Install mingw + cmake for runtime agent compilation
RUN apt-get update && apt-get install -y --no-install-recommends \
    g++-mingw-w64-x86-64 mingw-w64-tools cmake ninja-build curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Agent source + payload output
RUN mkdir -p /Agent /app/Payloads

EXPOSE 5000

ENTRYPOINT ["dotnet", "TeamServer.dll"]
