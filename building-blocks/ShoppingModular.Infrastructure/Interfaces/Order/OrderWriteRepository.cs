using ShoppingModular.Infrastructure.Common.Postgres;
using ShoppingModular.Infrastructure.Orders;
using DomainOrder = ShoppingModular.Domain.Orders.Order;

namespace ShoppingModular.Infrastructure.Interfaces.Order;

public class OrderWriteRepository(OrderDbContext context)
    : BasePostgresWriteRepository<DomainOrder>(context), IOrderWriteRepository;