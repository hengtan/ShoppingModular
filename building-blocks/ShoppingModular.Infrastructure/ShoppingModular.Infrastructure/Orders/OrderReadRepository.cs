using MongoDB.Driver;
using ShoppingModular.Infrastructure.Common.Mongo;
using ShoppingModular.ReadModels.Orders;

// seu read model

namespace ShoppingModular.Infrastructure.Orders;

public class OrderReadRepository(IMongoDatabase database) : BaseMongoReadRepository<OrderReadModel>(database, "orders");