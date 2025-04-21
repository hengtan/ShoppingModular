using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaProducerService.Api.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces;

namespace Orders.Consumer;

/// <summary>
///     Worker que escuta o tópico "orders.created" e projeta no MongoDB e Redis.
/// </summary>
public class OrderCreatedConsumerWorker(IServiceProvider serviceProvider, IConfiguration configuration)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            GroupId = "orders-consumer-group",
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("orders.created");

        while (!stoppingToken.IsCancellationRequested)
            try
            {
                var result = consumer.Consume(stoppingToken);
                var json = result.Message.Value;

                var eventData = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

                if (eventData is null) continue;
                using var scope = serviceProvider.CreateScope();
                var mongo = scope.ServiceProvider.GetRequiredService<IReadRepository<OrderReadModel>>();
                var cache = scope.ServiceProvider.GetRequiredService<ICacheService<OrderReadModel>>();
                var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

                var readModel = new OrderReadModel
                {
                    Id = eventData.Id,
                    CustomerName = eventData.CustomerName,
                    CreatedAt = eventData.CreatedAt,
                    TotalAmount = eventData.TotalAmount
                };

                // Projeção Mongo
                await db.GetCollection<OrderReadModel>("orders")
                    .InsertOneAsync(readModel, cancellationToken: stoppingToken);

                // Cache Redis
                await cache.SetAsync($"order:{eventData.Id}", readModel, TimeSpan.FromMinutes(10), stoppingToken);

                Console.WriteLine($"✔ Order projected: {eventData.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }

        consumer.Close();
    }
}