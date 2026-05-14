# syntax=docker/dockerfile:1.7

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80


# Build image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy csproj dulu agar restore bisa dicache
COPY ["QuilvianSystemBackendDev.csproj", "./"]

# Restore dependency dengan cache NuGet BuildKit
RUN --mount=type=cache,id=nuget-quilvian-backend-dev,target=/root/.nuget/packages \
    dotnet restore "QuilvianSystemBackendDev.csproj"

# Copy source setelah restore
COPY . .

# Publish aplikasi
# Optimasi:
# --no-restore              : tidak restore ulang
# UseAppHost=false          : output lebih kecil
# DebugType=None            : tidak generate debug info
# DebugSymbols=false        : tidak generate pdb/symbol
# RunAnalyzers=false        : mempercepat build jika analyzer aktif
# clp:ErrorsOnly            : log lebih bersih
RUN --mount=type=cache,id=nuget-quilvian-backend-dev,target=/root/.nuget/packages \
    dotnet publish "QuilvianSystemBackendDev.csproj" \
    -c Release \
    -o /app/out \
    --no-restore \
    /p:UseAppHost=false \
    /p:DebugType=None \
    /p:DebugSymbols=false \
    /p:RunAnalyzers=false \
    /p:ContinuousIntegrationBuild=true \
    /clp:ErrorsOnly


# Final image
FROM base AS final
WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "QuilvianSystemBackendDev.dll"]
