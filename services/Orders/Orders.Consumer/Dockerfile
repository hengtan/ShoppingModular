FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar todos os .csproj usados
COPY services/Orders/Orders.Consumer/Orders.Consumer.csproj ./services/Orders/Orders.Consumer/
COPY building-blocks/ShoppingModular.Infrastructure/ShoppingModular.Infrastructure.csproj ./building-blocks/ShoppingModular.Infrastructure/
COPY building-blocks/ShoppingModular.Application/ShoppingModular.Application.csproj ./building-blocks/ShoppingModular.Application/
COPY building-blocks/ShoppingModular.Domain/ShoppingModular.Domain.csproj ./building-blocks/ShoppingModular.Domain/
COPY services/Infrastructure/Kafka/KafkaProducerService/KafkaProducerService.csproj ./services/Infrastructure/Kafka/KafkaProducerService/

# Restore
RUN dotnet restore ./services/Orders/Orders.Consumer/Orders.Consumer.csproj

# Copiar o restante dos projetos
COPY services/Orders/Orders.Consumer/ ./services/Orders/Orders.Consumer/
COPY building-blocks/ShoppingModular.Infrastructure/ ./building-blocks/ShoppingModular.Infrastructure/
COPY building-blocks/ShoppingModular.Application/ ./building-blocks/ShoppingModular.Application/
COPY building-blocks/ShoppingModular.Domain/ ./building-blocks/ShoppingModular.Domain/
COPY services/Infrastructure/Kafka/KafkaProducerService/ ./services/Infrastructure/Kafka/KafkaProducerService/

WORKDIR /src/services/Orders/Orders.Consumer
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Orders.Consumer.dll"]