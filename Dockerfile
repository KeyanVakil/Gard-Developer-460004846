# syntax=docker/dockerfile:1

# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

COPY src/GardPortal/GardPortal.csproj src/GardPortal/
RUN dotnet restore src/GardPortal/GardPortal.csproj

COPY src/GardPortal/ src/GardPortal/
RUN dotnet publish src/GardPortal/GardPortal.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "GardPortal.dll"]
