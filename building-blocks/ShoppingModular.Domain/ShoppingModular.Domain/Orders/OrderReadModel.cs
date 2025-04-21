namespace ShoppingModular.Domain.Orders;

public class OrderReadModel
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
}