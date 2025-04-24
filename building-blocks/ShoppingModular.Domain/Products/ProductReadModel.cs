namespace ShoppingModular.Domain.Products;

/// <summary>
///     Modelo de leitura para produto, usado em projeções e cache.
/// </summary>
public class ProductReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = default!;
    public List<string> Tags { get; set; } = new();
    public List<string> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}