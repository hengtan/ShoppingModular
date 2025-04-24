using KafkaProducerService;
using MediatR;
using MongoDB.Driver;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces;
using ShoppingModular.Infrastructure.Interfaces.Order;

namespace ShoppingModular.Application.Orders.Commands;

/// <summary>
///     Handler responsável por criar pedidos, salvar projeção, cachear e publicar evento no Kafka.
/// </summary>
public class CreateOrderCommandHandler(
    IOrderWriteRepository writeRepo,
    IMongoDatabase mongoDb,
    ICacheService<OrderReadModel> cache,
    IKafkaProducerService kafka
) : IRequestHandler<CreateOrderCommand, Guid>
{
    #region Handle

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        #region 1. Criação do domínio

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            CreatedAt = DateTime.UtcNow,
            TotalAmount = request.TotalAmount
        };

        #endregion

        #region 2. Persiste no PostgreSQL

        await writeRepo.AddAsync(order, cancellationToken);

        #endregion

        #region 3. Cria o ReadModel para projeção

        var readModel = new OrderReadModel
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.TotalAmount
        };

        #endregion

        #region 4. Persiste no MongoDB (projeção de leitura)

        await mongoDb
            .GetCollection<OrderReadModel>("orders")
            .InsertOneAsync(readModel, cancellationToken: cancellationToken);

        #endregion

        #region 5. Cacheia no Redis (cache-aside pattern)

        await cache.SetAsync(
            $"order:{order.Id}",
            readModel,
            TimeSpan.FromMinutes(10),
            cancellationToken
        );

        #endregion

        #region 6. Publica no Kafka

        await kafka.PublishAsync("orders.created", readModel, cancellationToken);

        #endregion

        return order.Id;
    }

    #endregion
}