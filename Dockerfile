# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80


# Build image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy csproj dulu agar restore bisa dicache oleh Docker layer
COPY ["QuilvianSystemBackendDev.csproj", "./"]

# Restore dependency secara normal agar package masuk ke image layer
RUN dotnet restore "QuilvianSystemBackendDev.csproj"

# Copy semua source code setelah restore
COPY . .

# Publish aplikasi
RUN dotnet publish "QuilvianSystemBackendDev.csproj" \
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
