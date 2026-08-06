# Imagem de produção da API .NET 8 (build multi-stage, runtime enxuta).
# Contexto de build: a pasta backend/.

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os csproj primeiro para aproveitar o cache de restore.
COPY src/Crm.Domain/Crm.Domain.csproj src/Crm.Domain/
COPY src/Crm.Application/Crm.Application.csproj src/Crm.Application/
COPY src/Crm.Infrastructure/Crm.Infrastructure.csproj src/Crm.Infrastructure/
COPY src/Crm.Api/Crm.Api.csproj src/Crm.Api/
RUN dotnet restore src/Crm.Api/Crm.Api.csproj

# Copia o restante do código e publica.
COPY src/ src/
RUN dotnet publish src/Crm.Api/Crm.Api.csproj -c Release -o /app /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Pasta persistente das fotos (montada como volume no compose).
RUN mkdir -p /data/uploads

COPY --from=build /app ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Crm.Api.dll"]
