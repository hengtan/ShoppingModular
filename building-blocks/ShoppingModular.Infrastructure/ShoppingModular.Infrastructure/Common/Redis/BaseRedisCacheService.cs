using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using ShoppingModular.Infrastructure.Interfaces;

namespace ShoppingModular.Infrastructure.Common.Redis;

public abstract class BaseRedisCacheService<T>(IDistributedCache cache) : ICacheService<T>
{
    public virtual async Task<T?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var cached = await cache.GetStringAsync(key, cancellationToken);
        return cached is null ? default : JsonSerializer.Deserialize<T>(cached);
    }

    public virtual async Task SetAsync(string key, T data, TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
        };

        var json = JsonSerializer.Serialize(data);
        await cache.SetStringAsync(key, json, options, cancellationToken);
    }

    public virtual async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(key, cancellationToken);
    }
}