using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Orders.API.Commands;
using ShoppingModular.Infrastructure.DependencyInjection;
using ShoppingModular.Infrastructure.Orders;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// 1. 🔧 Configurações e Injeção de Dependência
// ─────────────────────────────────────────────────────────────

// Swagger para testes interativos
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Controllers baseadas em classes
builder.Services.AddControllers();

// MediatR para CQRS (Commands & Queries)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateOrderCommand>());

// PostgreSQL - gravação (EF Core)
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// MongoDB - leitura
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

// Redis - cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Registra infraestrutura compartilhada (repositories, facades, cache, etc)
builder.Services.AddInfrastructure();

// ─────────────────────────────────────────────────────────────
// 2. ⚙️ Configuração do Pipeline HTTP
// ─────────────────────────────────────────────────────────────

var app = builder.Build();

// Ativa Swagger apenas em dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Roteamento padrão
app.MapControllers();

// Health Check
app.MapGet("/", () => Results.Ok("Orders.API is running 🚀"));

app.Run();