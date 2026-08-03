# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
#USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CentroSenderos-2026-Server/CentroSenderos-2026-Server/CentroSenderos-2026-Server.csproj", "CentroSenderos-2026-Server/CentroSenderos-2026-Server/"]
COPY ["CentroSenderos-2026-BD/CentroSenderos-2026-BD.csproj", "CentroSenderos-2026-BD/"]
COPY ["CentroSenderos-2026-Shared/CentroSenderos-2026-Shared.csproj", "CentroSenderos-2026-Shared/"]
COPY ["CentroSenderos-2026-Repositorio/CentroSenderos-2026-Repositorio.csproj", "CentroSenderos-2026-Repositorio/"]
COPY ["CentroSenderos-2026-Server/CentroSenderos-2026-Server.Client/CentroSenderos-2026-Server.Client.csproj", "CentroSenderos-2026-Server/CentroSenderos-2026-Server.Client/"]
COPY ["CentroSenderos-2026-Servicio/CentroSenderos-2026-Servicio.csproj", "CentroSenderos-2026-Servicio/"]
RUN dotnet restore "./CentroSenderos-2026-Server/CentroSenderos-2026-Server/CentroSenderos-2026-Server.csproj"
COPY . .
WORKDIR "/src/CentroSenderos-2026-Server/CentroSenderos-2026-Server"
RUN dotnet build "./CentroSenderos-2026-Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CentroSenderos-2026-Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CentroSenderos-2026-Server.dll"]
