using DomainOrder = ShoppingModular.Domain.Orders.Order;

namespace ShoppingModular.Infrastructure.Interfaces.Order;

public interface IOrderWriteRepository : IWriteRepository<DomainOrder>
{
}