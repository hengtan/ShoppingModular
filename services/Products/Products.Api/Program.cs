using KafkaProducerService;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Products.Api.Extensions;
using ShoppingModular.Application.Products.Commands;
using ShoppingModular.Infrastructure.DependencyInjection;
using ShoppingModular.Infrastructure.Interfaces.Products;
using ShoppingModular.Infrastructure.Products;
using StackExchange.Redis;
using ProductWriteRepository = ShoppingModular.Infrastructure.Products.ProductWriteRepository;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// 0. 🧠 Corrige carregamento de appsettings (essencial no Docker/CI)
// ─────────────────────────────────────────────────────────────
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ─────────────────────────────────────────────────────────────
// 1. 🔧 Configuração de Serviços e Injeções de Dependência
// ─────────────────────────────────────────────────────────────

// Controllers + Swagger + API Explorer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR (Application Layer)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateProductCommand>());

// PostgreSQL (EF Core)
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// MongoDB (projeção de leitura)
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

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

// Kafka
builder.Services.AddScoped<IKafkaProducerService, KafkaProducerService.KafkaProducerService>();

// Injeções específicas de produtos
builder.Services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
builder.Services.AddInfrastructure(); // se estiver com uma extensão para injeções genéricas

// ─────────────────────────────────────────────────────────────
// 2. 🚀 Configuração e Execução da Aplicação
// ─────────────────────────────────────────────────────────────

var app = builder.Build();

// Aponta para execução via Docker
builder.WebHost.UseUrls("http://0.0.0.0:5002");

// Aplica migrações do banco
await app.ApplyMigrationsAsync<ProductDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/", () => Results.Ok("Products.API is running 🚀"));

app.Run();