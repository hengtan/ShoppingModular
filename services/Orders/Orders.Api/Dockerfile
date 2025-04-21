FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar csproj
COPY services/Orders/Orders.Api/Orders.Api.csproj ./services/Orders/Orders.Api/
COPY building-blocks/ShoppingModular.Application/ShoppingModular.Application.csproj ./building-blocks/ShoppingModular.Application/
COPY building-blocks/ShoppingModular.Domain/ShoppingModular.Domain.csproj ./building-blocks/ShoppingModular.Domain/
COPY building-blocks/ShoppingModular.Infrastructure/ShoppingModular.Infrastructure.csproj ./building-blocks/ShoppingModular.Infrastructure/
COPY services/Infrastructure/Kafka/KafkaProducerService/KafkaProducerService.csproj ./services/Infrastructure/Kafka/KafkaProducerService/

# Restore
RUN dotnet restore ./services/Orders/Orders.Api/Orders.Api.csproj

# Copiar todos os projetos necessários
COPY services/Orders/Orders.Api/ ./services/Orders/Orders.Api/
COPY building-blocks/ShoppingModular.Application/ ./building-blocks/ShoppingModular.Application/
COPY building-blocks/ShoppingModular.Domain/ ./building-blocks/ShoppingModular.Domain/
COPY building-blocks/ShoppingModular.Infrastructure/ ./building-blocks/ShoppingModular.Infrastructure/
COPY services/Infrastructure/Kafka/KafkaProducerService/ ./services/Infrastructure/Kafka/KafkaProducerService/

# 🚫 Remove appsettings.json duplicado que causa conflito no publish
RUN rm -f /src/services/Infrastructure/Kafka/KafkaProducerService.Api/appsettings.json

# Publica só o projeto principal
WORKDIR /src/services/Orders/Orders.Api
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Orders.Api.dll"]