using MongoDB.Driver;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Common.Mongo;

// seu read model

namespace ShoppingModular.Infrastructure.Orders;

public class OrderReadRepository(IMongoDatabase database) : BaseMongoReadRepository<OrderReadModel>(database, "orders");