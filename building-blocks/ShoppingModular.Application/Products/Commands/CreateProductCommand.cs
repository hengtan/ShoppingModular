using MediatR;

namespace ShoppingModular.Application.Products.Commands;

/// <summary>
/// Comando para criar um novo produto.
/// </summary>
public class CreateProductCommand : IRequest<Guid>
{
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public string Category { get; init; } = default!;
    public List<string> Tags { get; init; } = new();
    public List<string> Images { get; init; } = new();
}