# ---------------------------
# Etapa de build
# ---------------------------
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Variables para evitar rutas de Windows
ENV NUGET_PACKAGES=/root/.nuget/packages
ENV DOTNET_NOLOGO=true
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

# Copiar solución y proyectos
COPY *.sln ./
COPY *.csproj ./

# Copiar la carpeta libs con las DLL necesarias
COPY libs/ ./libs/
RUN ls -la /src/libs


# Restaurar dependencias
RUN dotnet restore

# Copiar el resto del código 
COPY . ./

# Publicar en modo Release
RUN dotnet publish -c Release -o /app/publish

# ---------------------------
# Etapa final (runtime)
# ---------------------------
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "APISietemasdereservas.dll"]
