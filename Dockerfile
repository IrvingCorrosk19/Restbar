# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia todo el contenido al contenedor
COPY . .

# Entrar al subdirectorio del proyecto
WORKDIR /app/RestBar

# Restaurar y publicar en Release
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copiar los archivos publicados desde la etapa de build
COPY --from=build /app/publish .

# Puerto que se expone
EXPOSE 80

# Ejecutar la aplicación
ENTRYPOINT ["dotnet", "RestBar.dll"]
