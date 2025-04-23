using ShoppingModular.Application.Interfaces.Persistence;
using ShoppingModular.Domain.Products;

namespace ShoppingModular.Infrastructure.Products;

public class ProductWriteRepository : IProductWriteRepository
{
    private readonly ProductDbContext _context;

    public ProductWriteRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(Product product, CancellationToken cancellationToken)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
    }
}