using KafkaProducerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Orders.Consumer;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces;
using ShoppingModular.Infrastructure.Orders;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // MongoDB
        services.AddSingleton<IMongoClient>(_ =>
            new MongoClient(configuration["MongoSettings:ConnectionString"]));

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(configuration["MongoSettings:Database"]);
        });

        // Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        // Handlers e cache services
        services.AddScoped<IReadRepository<OrderReadModel>, OrderReadRepository>();
        services.AddScoped<ICacheService<OrderReadModel>, OrderCacheService>();

        // Kafka Consumer Worker
        services.AddHostedService<OrderCreatedConsumerWorker>();

        services.AddSingleton<IKafkaProducerService, KafkaProducerService.KafkaProducerService>();
    })
    .Build()
    .Run();