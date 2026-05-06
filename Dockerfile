FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

COPY ["QuilvianSystemBackendDev.csproj", "./"]

RUN dotnet restore "QuilvianSystemBackendDev.csproj"

COPY . .

RUN dotnet publish "QuilvianSystemBackendDev.csproj" \
    -c Release \
    -o /app/out \
    --no-restore \
    /p:UseAppHost=false \
    /p:DebugType=None \
    /p:DebugSymbols=false \
    /clp:ErrorsOnly

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final

WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "QuilvianSystemBackendDev.dll"]
