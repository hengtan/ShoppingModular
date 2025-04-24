using Bogus;
using KafkaProducerService;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using ShoppingModular.Application.Orders.Commands;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces;
using ShoppingModular.Infrastructure.Interfaces.Order;

namespace ShoppingModular.Tests.Commands;

[TestFixture]
public class CreateOrderCommandHandlerTests
{
    [SetUp]
    public void Setup()
    {
        _faker = new Faker();

        _writeRepoMock = new Mock<IOrderWriteRepository>();
        _mongoDbMock = new Mock<IMongoDatabase>();
        _mongoCollectionMock = new Mock<IMongoCollection<OrderReadModel>>();
        _cacheMock = new Mock<ICacheService<OrderReadModel>>();
        _kafkaMock = new Mock<IKafkaProducerService>();

        // Simula inserção no MongoDB (projeção)
        _mongoCollectionMock.Setup(x =>
                x.InsertOneAsync(It.IsAny<OrderReadModel>(), null, default))
            .Returns(Task.CompletedTask);

        _mongoDbMock.Setup(db =>
                db.GetCollection<OrderReadModel>("orders", null))
            .Returns(_mongoCollectionMock.Object);
    }

    private Faker _faker = null!;
    private Mock<IOrderWriteRepository> _writeRepoMock = null!;
    private Mock<IMongoDatabase> _mongoDbMock = null!;
    private Mock<IMongoCollection<OrderReadModel>> _mongoCollectionMock = null!;
    private Mock<ICacheService<OrderReadModel>> _cacheMock = null!;
    private Mock<IKafkaProducerService> _kafkaMock = null!;

    private CreateOrderCommandHandler CreateHandler()
    {
        return new CreateOrderCommandHandler(_writeRepoMock.Object, _mongoDbMock.Object, _cacheMock.Object,
            _kafkaMock.Object);
    }

    [Test]
    public async Task Handle_Should_Persist_Order_And_Trigger_Projections_And_Cache_And_Kafka()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new CreateOrderCommand(
            _faker.Name.FullName(),
            _faker.Random.Decimal(10, 500));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.EqualTo(Guid.Empty), "Order ID should not be empty");

        _writeRepoMock.Verify(x =>
            x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);

        _mongoDbMock.Verify(db =>
            db.GetCollection<OrderReadModel>("orders", null), Times.Once);

        _mongoCollectionMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<OrderReadModel>(), null, default), Times.Once);

        _cacheMock.Verify(x =>
            x.SetAsync(
                $"order:{result}",
                It.Is<OrderReadModel>(o => o.Id == result),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _kafkaMock.Verify(x =>
            x.PublishAsync("orders.created", It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}