FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "services/Kafka/KafkaProducerService.API/KafkaProducerService.API.csproj"
RUN dotnet publish "services/Kafka/KafkaProducerService.API/KafkaProducerService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KafkaProducerService.API.dll"]