FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

# 1. COPY project file dulu (cache layer)
COPY QuilvianSystemBackendDev.csproj ./

# 2. restore dengan project spesifik
RUN dotnet restore QuilvianSystemBackendDev.csproj

# 3. baru copy source
COPY . ./

# 4. publish lebih stabil (anti RAM spike)
RUN dotnet publish QuilvianSystemBackendDev.csproj \
  -c Release \
  -o /app/out \
  --no-restore \
  /m:1 \
  /p:UseSharedCompilation=false \
  /p:Deterministic=true \
  /p:DebugType=None \
  /p:DebugSymbols=false \
  /p:WarningLevel=0 \
  /p:ContinuousIntegrationBuild=true

# runtime image kecil
FROM mcr.microsoft.com/dotnet/aspnet:6.0

WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "QuilvianSystemBackendDev.dll"]




# FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# # Set the working directory inside the container
# WORKDIR /app

# # Copy the main project file
# COPY QuilvianSystemBackendDev.csproj ./

# # Restore dependencies
# RUN dotnet restore QuilvianSystemBackendDev.csproj

# # Copy the rest of the application code
# COPY . ./

# # Publish the app to a directory in the container
# #RUN dotnet publish -c Release -o out
# RUN dotnet publish -c Release -o out /maxcpucount:2
# dotnet publish "QuilvianSystemBackendDev.csproj" \
#   -c Release \
#   -o Out \
#   --no-restore \
#   /m:1 \
#   /p:UseSharedCompilation=false \
#   /p:Deterministic=true \
#   /p:DebugType=None \
#   /p:DebugSymbols=false
# # Sesudah
# #RUN dotnet publish -c Release -o out --no-restore /maxcpucount:5
# # Use the official ASP.NET image for runtime
# FROM mcr.microsoft.com/dotnet/aspnet:6.0

# # Set the working directory inside the container
# WORKDIR /app

# # Copy the published app from the previous image
# COPY --from=build /app/out .

# # Set the entry point to start the application
# ENTRYPOINT ["dotnet", "QuilvianSystemBackendDev.dll"]



# Use the official .NET SDK images quilviandev tes

#FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory inside the container
#WORKDIR /app

# Copy the main project file
# COPY QuilvianSystemBackendDev.csproj ./

# Restore dependencies
#RUN dotnet restore QuilvianSystemBackendDev.csproj

# Copy the rest of the application code
#COPY . ./

# Publish the app to a directory in the container
#RUN dotnet publish -c Release -o out
#RUN dotnet publish -c Release -o out /maxcpucount:2
# Sesudah
#RUN dotnet publish -c Release -o out --no-restore /maxcpucount:5
# Use the official ASP.NET image for runtime
#FROM mcr.microsoft.com/dotnet/aspnet:6.0

# Set the working directory inside the container
#WORKDIR /app

# Copy the published app from the previous image
#COPY --from=build /app/out .

# Set the entry point to start the application
#ENTRYPOINT ["dotnet", "QuilvianSystemBackendDev.dll"]
