# syntax=docker/dockerfile:1

# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first (better layer caching) by copying only project files.
COPY ["backend/src/AssetFlow.Api/AssetFlow.Api.csproj", "backend/src/AssetFlow.Api/"]
COPY ["backend/src/AssetFlow.Application/AssetFlow.Application.csproj", "backend/src/AssetFlow.Application/"]
COPY ["backend/src/AssetFlow.Domain/AssetFlow.Domain.csproj", "backend/src/AssetFlow.Domain/"]
COPY ["backend/src/AssetFlow.Infrastructure/AssetFlow.Infrastructure.csproj", "backend/src/AssetFlow.Infrastructure/"]
RUN dotnet restore "backend/src/AssetFlow.Api/AssetFlow.Api.csproj"

# Copy the rest of the backend and publish.
COPY backend/ backend/
WORKDIR "/src/backend/src/AssetFlow.Api"
RUN dotnet publish "AssetFlow.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Run as the non-root user baked into the .NET 8 runtime image.
USER $APP_UID

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AssetFlow.Api.dll"]
