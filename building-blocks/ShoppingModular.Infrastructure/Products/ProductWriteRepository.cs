using ShoppingModular.Domain.Products;
using ShoppingModular.Infrastructure.Common.Postgres;
using ShoppingModular.Infrastructure.Interfaces.Products;

namespace ShoppingModular.Infrastructure.Products;

/// <summary>
///     Repositório de escrita para produtos no PostgreSQL.
/// </summary>
public class ProductWriteRepository(ProductDbContext context)
    : BasePostgresWriteRepository<Product>(context), IProductWriteRepository;