using KafkaProducerService;
using KafkaProducerService.Api;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Orders.Api.Extensions;
using ShoppingModular.Application.Orders.Commands;
using ShoppingModular.Infrastructure.DependencyInjection;
using ShoppingModular.Infrastructure.Orders;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// 1. 🔧 Configurações e Injeção de Dependência
// ─────────────────────────────────────────────────────────────

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateOrderCommand>());

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var config = builder.Configuration.GetSection("MongoSettings");
    return new MongoClient(config["ConnectionString"]);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var config = builder.Configuration.GetSection("MongoSettings");
    return client.GetDatabase(config["Database"]);
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddInfrastructure();

builder.Services.AddScoped<IKafkaProducerService, KafkaProducerService.KafkaProducerService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

var app = builder.Build();

builder.WebHost.UseUrls("http://0.0.0.0:5001");

// ✅ Aplica migrations automaticamente
await app.ApplyMigrationsAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGet("/", () => Results.Ok("Orders.API is running 🚀"));

app.Run();