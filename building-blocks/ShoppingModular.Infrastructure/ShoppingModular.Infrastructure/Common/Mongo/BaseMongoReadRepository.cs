using MongoDB.Driver;
using ShoppingModular.Infrastructure.Interfaces;

namespace ShoppingModular.Infrastructure.Common.Mongo;

public abstract class BaseMongoReadRepository<T>(IMongoDatabase database, string collectionName) : IReadRepository<T>
{
    private readonly IMongoCollection<T> _collection = database.GetCollection<T>(collectionName);

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(Builders<T>.Filter.Eq("Id", id)).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).ToListAsync(cancellationToken);
    }
}