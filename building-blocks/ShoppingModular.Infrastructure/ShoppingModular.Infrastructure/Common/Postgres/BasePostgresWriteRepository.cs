using Microsoft.EntityFrameworkCore;
using ShoppingModular.Infrastructure.Interfaces;

namespace ShoppingModular.Infrastructure.Common.Postgres;

public abstract class BasePostgresWriteRepository<T>(DbContext context) : IWriteRepository<T>
    where T : class
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync([id], cancellationToken);
        if (entity is null) return;
        _dbSet.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}