using System.Text.Json;
using Confluent.Kafka;
using KafkaProducerService.Api.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using ShoppingModular.Domain.Products;
using ShoppingModular.Infrastructure.Interfaces;

namespace Products.Consumer;

/// <summary>
/// Worker que escuta o tópico "products.created" e projeta no MongoDB e Redis.
/// </summary>
public class ProductCreatedConsumerWorker(IServiceProvider serviceProvider, IConfiguration configuration)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            GroupId = "products-consumer-group",
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("products.created");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var json = result.Message.Value;

                var eventData = JsonSerializer.Deserialize<ProductCreatedEvent>(json);
                if (eventData is null) continue;

                using var scope = serviceProvider.CreateScope();
                var mongo = scope.ServiceProvider.GetRequiredService<IReadRepository<ProductReadModel>>();
                var cache = scope.ServiceProvider.GetRequiredService<ICacheService<ProductReadModel>>();
                var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

                var readModel = new ProductReadModel
                {
                    Id = eventData.Id,
                    Name = eventData.Name,
                    Description = eventData.Description,
                    Price = eventData.Price,
                    Stock = eventData.Stock,
                    Category = eventData.Category,
                    Tags = eventData.Tags,
                    Images = eventData.Images,
                    CreatedAt = eventData.CreatedAt
                };

                await db.GetCollection<ProductReadModel>("products")
                    .InsertOneAsync(readModel, cancellationToken: stoppingToken);

                await cache.SetAsync($"product:{eventData.Id}", readModel, TimeSpan.FromMinutes(10), stoppingToken);

                Console.WriteLine($"✔ Product projected: {eventData.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        consumer.Close();
    }
}