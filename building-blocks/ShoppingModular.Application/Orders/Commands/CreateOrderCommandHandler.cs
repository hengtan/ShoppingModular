using KafkaProducerService;
using KafkaProducerService.Api;
using MediatR;
using MongoDB.Driver;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces;

namespace ShoppingModular.Application.Orders.Commands;

/// <summary>
///     Handler responsável por executar o comando de criação de pedido.
/// </summary>
public class CreateOrderCommandHandler(
    IWriteRepository<Order> writeRepo,
    IMongoDatabase mongoDb,
    ICacheService<OrderReadModel> cache,
    IKafkaProducerService kafka) // ✅ Novo parâmetro
    : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            CreatedAt = DateTime.UtcNow,
            TotalAmount = request.TotalAmount
        };

        await writeRepo.AddAsync(order, cancellationToken);

        var readModel = new OrderReadModel
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.TotalAmount
        };

        await mongoDb.GetCollection<OrderReadModel>("orders")
            .InsertOneAsync(readModel, cancellationToken: cancellationToken);

        await cache.SetAsync($"order:{order.Id}", readModel, TimeSpan.FromMinutes(10), cancellationToken);

        // ✅ Publica o evento no Kafka
        await kafka.PublishAsync("orders.created", readModel, cancellationToken);

        return order.Id;
    }
}