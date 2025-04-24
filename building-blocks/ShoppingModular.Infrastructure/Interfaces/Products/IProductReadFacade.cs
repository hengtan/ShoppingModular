using ShoppingModular.Domain.Products;

namespace ShoppingModular.Infrastructure.Interfaces.Products;

/// <summary>
///     Interface para leitura de produtos com cache-aside.
/// </summary>
public interface IProductReadFacade
{
    Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
}