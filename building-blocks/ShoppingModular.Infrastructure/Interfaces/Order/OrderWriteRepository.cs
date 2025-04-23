using DomainOrder = ShoppingModular.Domain.Orders.Order;

using ShoppingModular.Infrastructure.Common.Postgres;
using ShoppingModular.Infrastructure.Orders;

namespace ShoppingModular.Infrastructure.Interfaces.Order;

public class OrderWriteRepository(OrderDbContext context)
    : BasePostgresWriteRepository<DomainOrder>(context), IOrderWriteRepository;