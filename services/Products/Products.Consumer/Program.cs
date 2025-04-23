using KafkaProducerService;
using MongoDB.Driver;
using Products.Consumer;
using ShoppingModular.Domain.Products;
using ShoppingModular.Infrastructure.Interfaces;
using ShoppingModular.Infrastructure.Products;

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

        // Projeções e cache
        services.AddScoped<IReadRepository<ProductReadModel>, ProductReadRepository>();
        services.AddScoped<ICacheService<ProductReadModel>, ProductCacheService>();

        // Kafka Consumer
        services.AddHostedService<ProductCreatedConsumerWorker>();

        services.AddSingleton<IKafkaProducerService, KafkaProducerService.KafkaProducerService>();
    })
    .Build()
    .Run();