using Microsoft.Extensions.Caching.Distributed;
using ShoppingModular.Infrastructure.Common.Redis;
using ShoppingModular.ReadModels.Orders;

// seu read model

namespace ShoppingModular.Infrastructure.Orders;

public class OrderCacheService(IDistributedCache cache) : BaseRedisCacheService<OrderReadModel>(cache);