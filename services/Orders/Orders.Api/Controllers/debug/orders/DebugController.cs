using System.Text.Json;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Orders;
using StackExchange.Redis;
using Order = ShoppingModular.Domain.Orders.Order;

namespace Orders.Api.Controllers.debug.orders;

[ApiController]
[Route("api/debug")]
public class DebugController(
    OrderDbContext dbContext,
    IMongoDatabase mongo,
    IConnectionMultiplexer redis,
    IConfiguration config) : ControllerBase
{
    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders(CancellationToken ct)
    {
        var result = new
        {
            Postgres = new List<Order>(),
            Mongo = new List<OrderReadModel>(),
            Redis = new List<OrderReadModel>(),
            Kafka = new List<object>()
        };

        // 1️⃣ PostgreSQL: Busca todos os registros reais de pedidos
        var postgresOrders = await dbContext.Orders.ToListAsync(ct);
        result.Postgres.AddRange(postgresOrders);

        // 2️⃣ MongoDB
        var mongoOrders = await mongo.GetCollection<OrderReadModel>("orders")
            .Find(Builders<OrderReadModel>.Filter.Empty)
            .ToListAsync(ct);
        result.Mongo.AddRange(mongoOrders);

        // 3️⃣ Redis
        var db = redis.GetDatabase();
        var server = redis.GetServer(redis.GetEndPoints().First());
        var keys = server.Keys(pattern: "order:*").ToArray();

        foreach (var key in keys)
            try
            {
                var json = await db.StringGetAsync(key);
                if (!json.IsNullOrEmpty)
                {
                    var order = JsonSerializer.Deserialize<OrderReadModel>(json);
                    if (order != null)
                        result.Redis.Add(order);
                }
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("WRONGTYPE"))
            {
                Console.WriteLine($"⚠️ Ignorando chave inválida no Redis: {key}");
            }

        // 4️⃣ Kafka
        var consumerConfig = new ConsumerConfig
        {
            GroupId = "debug-consumer-group",
            BootstrapServers = config["Kafka:BootstrapServers"],
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnablePartitionEof = true
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Assign(new TopicPartitionOffset("orders.created", 0, Offset.Beginning));

        try
        {
            var count = 0;
            while (count < 10)
            {
                var resultKafka = consumer.Consume(TimeSpan.FromSeconds(1));
                if (resultKafka == null || resultKafka.IsPartitionEOF) break;

                var msg = JsonSerializer.Deserialize<OrderReadModel>(resultKafka.Value);

                if (msg is not null)
                    result.Kafka.Add(msg);
                else
                    result.Kafka.Add(resultKafka.Value);

                count++;
            }
        }
        catch (Exception ex)
        {
            result.Kafka.Add(new { error = ex.Message });
        }

        return Ok(result);
    }
}