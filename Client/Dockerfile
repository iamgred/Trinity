# syntax=docker/dockerfile:1
# Multi-stage Dockerfile for Trinity Client
# Builds the .NET 10 Blazor client app

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose the client port
EXPOSE 5001

ENTRYPOINT ["dotnet", "Client.dll"]
