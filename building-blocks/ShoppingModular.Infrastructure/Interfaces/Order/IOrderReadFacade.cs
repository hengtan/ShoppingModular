using ShoppingModular.Domain.Orders;

namespace ShoppingModular.Infrastructure.Interfaces.Order;

public interface IOrderReadFacade
{
    Task<OrderReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
}