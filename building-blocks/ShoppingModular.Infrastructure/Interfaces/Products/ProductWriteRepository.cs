using ShoppingModular.Domain.Products;
using ShoppingModular.Infrastructure.Common.Postgres;
using ShoppingModular.Infrastructure.Products;

namespace ShoppingModular.Infrastructure.Interfaces.Products;

public class ProductWriteRepository(ProductDbContext context)
    : BasePostgresWriteRepository<Product>(context), IProductWriteRepository;