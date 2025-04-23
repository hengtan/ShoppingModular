using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces;
using ShoppingModular.Infrastructure.Interfaces.Order;

namespace ShoppingModular.Infrastructure.Orders;

/// <summary>
///     Responsável por aplicar a lógica de Cache-aside para leitura de pedidos.
/// </summary>
public class OrderReadFacade(
    IReadRepository<OrderReadModel> mongoRepo,
    ICacheService<OrderReadModel> redisCache)
    : IOrderReadFacade
{
    public async Task<OrderReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cacheKey = $"order:{id}";

        // 1. Tenta pegar do Redis
        var cached = await redisCache.GetAsync(cacheKey, ct);
        if (cached is not null)
            return cached;

        // 2. Se não encontrou, busca do Mongo
        var order = await mongoRepo.GetByIdAsync(id, ct);
        if (order is not null) await redisCache.SetAsync(cacheKey, order, TimeSpan.FromMinutes(10), ct);

        return order;
    }
}