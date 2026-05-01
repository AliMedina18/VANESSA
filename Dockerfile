# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia el proyecto y restaura paquetes
COPY ["Vanessa/Vanessa/Vanessa.csproj", "Vanessa/Vanessa/"]
RUN dotnet restore "Vanessa/Vanessa/Vanessa.csproj"

# Copia todo el resto del repositorio y publica
COPY . .
WORKDIR /src/Vanessa/Vanessa
RUN dotnet publish "Vanessa.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Vanessa.dll"]
