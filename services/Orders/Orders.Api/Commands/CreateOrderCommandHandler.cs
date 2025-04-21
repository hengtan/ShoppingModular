using MediatR;
using MongoDB.Driver;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces;

namespace Orders.API.Commands;

/// <summary>
/// Handler responsável por executar o comando de criação de pedido.
/// </summary>
public class CreateOrderCommandHandler(
    IWriteRepository<Order> writeRepo,
    IMongoDatabase mongoDb,
    ICacheService<OrderReadModel> cache)
    : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Cria entidade de domínio
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            CreatedAt = DateTime.UtcNow,
            TotalAmount = request.TotalAmount
        };

        // 2. Salva no PostgreSQL
        await writeRepo.AddAsync(order, cancellationToken);

        // 3. Projeção (read model)
        var readModel = new OrderReadModel
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.TotalAmount
        };

        // 4. Salva no MongoDB
        await mongoDb.GetCollection<OrderReadModel>("orders")
            .InsertOneAsync(readModel, cancellationToken: cancellationToken);

        // 5. Atualiza cache Redis
        await cache.SetAsync($"order:{order.Id}", readModel, TimeSpan.FromMinutes(10), cancellationToken);

        return order.Id;
    }
}