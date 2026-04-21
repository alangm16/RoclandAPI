# 1. Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0-nanoserver-ltsc2022 AS build
WORKDIR /src

# Copiar archivos de solución y proyectos para restaurar
COPY ["Rocland.slnx", "./"]
COPY ["Rocland.Api/Rocland.Api.csproj", "Rocland.Api/"]
COPY ["RCD.Shared.Kernel/RCD.Shared.Kernel.csproj", "RCD.Shared.Kernel/"]
COPY ["RCD.Web.AccesoControl.Module/RCD.Web.AccesoControl.Module.csproj", "RCD.Web.AccesoControl.Module/"]
COPY ["RCD.Mob.AccesoControl.Module/RCD.Mob.AccesoControl.Module.csproj", "RCD.Mob.AccesoControl.Module/"]
# ... (añadir aquí otros proyectos de infraestructura/aplicación que sean dependencias)

RUN dotnet restore "Rocland.Api/Rocland.Api.csproj"

# Copiar todo el código
COPY . .

# --- PUBLICACIÓN ---

# A. Publicar el Host Principal
WORKDIR "/src/Rocland.Api"
RUN dotnet publish "Rocland.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# B. Publicar Módulo Web
WORKDIR "/src/RCD.Web.AccesoControl.Module"
RUN dotnet publish -c Release -o /app/publish/Modules/AccesoControlWeb 

# C. Publicar Módulo Mobile (Asegúrate que el nombre coincida con lo que busca el Host)
WORKDIR "/src/RCD.Mob.AccesoControl.Module"
RUN dotnet publish -c Release -o /app/publish/Modules/AccesoControlMobile

# 2. Etapa final: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-nanoserver-ltsc2022 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Rocland.Api.dll"]