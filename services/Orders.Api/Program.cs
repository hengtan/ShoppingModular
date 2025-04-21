using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using ShoppingModular.Infrastructure.DependencyInjection;
using ShoppingModular.Infrastructure.Orders;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger for testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add PostgreSQL DbContext
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Add MongoDB
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

// Add Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Add custom Infrastructure services
builder.Services.AddInfrastructure();

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// Health check route
app.MapGet("/", () => Results.Ok("Orders.API is running"));

// Exemplo (vamos adicionar a rota real depois)
app.MapGet("/api/orders/{id}", async (
    Guid id,
    OrderReadFacade facade,
    CancellationToken ct) =>
{
    var order = await facade.GetByIdAsync(id, ct);
    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.Run();