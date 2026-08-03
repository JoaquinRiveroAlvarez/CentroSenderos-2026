# Imagen base oficial de ASP.NET Core 8 para producción
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Imagen SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos toda la solución
COPY . .

# Publicamos SOLO el proyecto Server
RUN dotnet publish ./CentroSenderos-2026-Server/CentroSenderos-2026-Server.csproj -c Release -o /app/publish

# Imagen final con el runtime
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CentroSenderos-2026-Server.dll"]
