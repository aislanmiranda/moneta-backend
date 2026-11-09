# Build context: raiz deste repositório (moneta-backend)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/ src/
RUN dotnet restore src/AgentOffice.Api/AgentOffice.Api.csproj

# Aplica as migrations do EF antes da API subir (serviço "migrate" no docker-compose)
FROM build AS migrate
RUN dotnet tool install --global dotnet-ef --version 10.0.13
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ef", "database", "update", \
    "--project", "src/AgentOffice.Infra", \
    "--startup-project", "src/AgentOffice.Api"]

FROM build AS publish
RUN dotnet publish src/AgentOffice.Api/AgentOffice.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
# Npgsql probes for GSSAPI on startup; without this lib it logs a noisy (harmless) error.
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AgentOffice.Api.dll"]
