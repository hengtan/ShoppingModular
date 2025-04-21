using Microsoft.EntityFrameworkCore;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Common.Postgres;
// sua entidade real

namespace ShoppingModular.Infrastructure.Orders;

public class OrderWriteRepository(DbContext context) : BasePostgresWriteRepository<Order>(context);