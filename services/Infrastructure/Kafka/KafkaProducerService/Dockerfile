FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5011

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia os .csproj
COPY services/Infrastructure/Kafka/KafkaProducerService.Api/KafkaProducerService.Api.csproj ./services/Infrastructure/Kafka/KafkaProducerService.Api/
COPY building-blocks/ShoppingModular.Domain/ShoppingModular.Domain.csproj ./building-blocks/ShoppingModular.Domain/
COPY building-blocks/ShoppingModular.Application/ShoppingModular.Application.csproj ./building-blocks/ShoppingModular.Application/
COPY building-blocks/ShoppingModular.Infrastructure/ShoppingModular.Infrastructure.csproj ./building-blocks/ShoppingModular.Infrastructure/

# Restore
RUN dotnet restore ./services/Infrastructure/Kafka/KafkaProducerService.Api/KafkaProducerService.Api.csproj

# Copia todo o código
COPY services/Infrastructure/Kafka/KafkaProducerService.Api/ ./services/Infrastructure/Kafka/KafkaProducerService.Api/
COPY building-blocks/ShoppingModular.Domain/ ./building-blocks/ShoppingModular.Domain/
COPY building-blocks/ShoppingModular.Application/ ./building-blocks/ShoppingModular.Application/
COPY building-blocks/ShoppingModular.Infrastructure/ ./building-blocks/ShoppingModular.Infrastructure/

COPY services/Infrastructure/Kafka/KafkaProducerService.Api/appsettings.json ./services/Infrastructure/Kafka/KafkaProducerService.Api/

WORKDIR /src/services/Infrastructure/Kafka/KafkaProducerService.Api
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KafkaProducerService.Api.dll"]