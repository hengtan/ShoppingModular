using Microsoft.Extensions.Caching.Distributed;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Common.Redis;

// seu read model

namespace ShoppingModular.Infrastructure.Orders;

public class OrderCacheService(IDistributedCache cache) : BaseRedisCacheService<OrderReadModel>(cache);