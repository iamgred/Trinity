# syntax=docker/dockerfile:1
# Multi-stage Dockerfile for Trinity Client
# Builds the .NET 10 preview Blazor client app

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files (including shared library)
COPY Client/Client.csproj Client/
COPY Trinity.Shared/ Trinity.Shared/
RUN dotnet restore Client/Client.csproj

# Copy source and build
COPY Client/ Client/
RUN dotnet publish Client/Client.csproj -c Release -o /app/publish \
    /p:UseAppHost=true

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Install required dependencies (webview2 runtime, fonts)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgl1 libegl1 libxkbcommon0 libnss3 libatk1.0-0 libatk-bridge2.0-0 \
    libcups2 libdrm2 libgbm1 libasound2t64 libgtk-3-0 fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Expose the client port
EXPOSE 5001

ENTRYPOINT ["dotnet", "Client.dll"]
