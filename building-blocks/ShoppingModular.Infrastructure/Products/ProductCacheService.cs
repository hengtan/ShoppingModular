using Microsoft.Extensions.Caching.Distributed;
using ShoppingModular.Domain.Products;
using ShoppingModular.Infrastructure.Common.Redis;

namespace ShoppingModular.Infrastructure.Products;

/// <summary>
/// Serviço de cache Redis para produtos.
/// </summary>
public class ProductCacheService(IDistributedCache cache)
    : BaseRedisCacheService<ProductReadModel>(cache);