FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

# Garante que os arquivos de configuração estejam presentes para o publish
COPY services/Products/Products.Api/appsettings.json ./services/Products/Products.Api/
COPY services/Products/Products.Api/appsettings.CI.json ./services/Products/Products.Api/

WORKDIR /src/services/Products/Products.Api
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Products.Api.dll"]