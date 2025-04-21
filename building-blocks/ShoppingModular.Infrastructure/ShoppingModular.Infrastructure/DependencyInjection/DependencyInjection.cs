using Microsoft.Extensions.DependencyInjection;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces;
using ShoppingModular.Infrastructure.Orders;

namespace ShoppingModular.Infrastructure.DependencyInjection;

/// <summary>
///     Responsável por registrar os serviços de infraestrutura que utilizam Redis, Mongo e Postgres.
///     As configurações (connection strings) ficam a cargo da camada API.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Repositório de leitura (MongoDB)
        services.AddScoped<IReadRepository<OrderReadModel>, OrderReadRepository>();

        // Repositório de escrita (PostgreSQL)
        services.AddScoped<IWriteRepository<Order>, OrderWriteRepository>();

        // Cache em Redis
        services.AddScoped<ICacheService<OrderReadModel>, OrderCacheService>();

        // Fachada com cache-aside
        services.AddScoped<OrderReadFacade>();

        return services;
    }
}