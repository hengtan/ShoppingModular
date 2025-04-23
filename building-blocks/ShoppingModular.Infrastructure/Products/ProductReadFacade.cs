using ShoppingModular.Domain.Products;
using ShoppingModular.Infrastructure.Interfaces;
using ShoppingModular.Infrastructure.Interfaces.Products;

namespace ShoppingModular.Infrastructure.Products;

/// <summary>
/// Responsável por aplicar o padrão cache-aside na leitura de produtos.
/// </summary>
public class ProductReadFacade(
    IReadRepository<ProductReadModel> mongoRepo,
    ICacheService<ProductReadModel> redisCache)
    : IProductReadFacade
{
    public async Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cacheKey = $"product:{id}";

        // 1. Tenta recuperar do cache (Redis)
        var cached = await redisCache.GetAsync(cacheKey, ct);
        if (cached is not null)
            return cached;

        // 2. Se não encontrou, busca do MongoDB
        var product = await mongoRepo.GetByIdAsync(id, ct);
        if (product is not null)
        {
            await redisCache.SetAsync(cacheKey, product, TimeSpan.FromMinutes(10), ct);
        }

        return product;
    }
}