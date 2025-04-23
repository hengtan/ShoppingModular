namespace ShoppingModular.Domain.Products;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public string Category { get; set; } = default!;
    public List<string> Tags { get; set; } = new();
    public List<string> Images { get; set; } = new();

    public bool IsActive { get; set; } = true;
    public double Rating { get; set; } = 0.0;
    public int ReviewCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Métodos de domínio ricos
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void UpdateStock(int quantity)
    {
        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}