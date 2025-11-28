# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copiar la solucion completa
COPY . .

# Restaurar paquetes
RUN dotnet restore Machly.Api.csproj

# Compilar en Release
RUN dotnet publish Machly.Api.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Machly.Api.dll"]
