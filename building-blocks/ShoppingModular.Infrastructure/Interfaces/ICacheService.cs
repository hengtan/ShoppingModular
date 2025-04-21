namespace ShoppingModular.Infrastructure.Interfaces;

public interface ICacheService<T>
{
    Task<T?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, T data, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}