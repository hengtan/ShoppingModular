using Bogus;
using KafkaProducerService.Api;
using NUnit.Framework;
using MongoDB.Driver;
using Moq;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces;
using ShoppingModular.Application.Orders.Commands;

namespace ShoppingModular.Tests.Commands;

[TestFixture]
public class CreateOrderCommandHandlerTests
{
    private Faker _faker = null!;
    private Mock<IWriteRepository<Order>> _writeRepoMock = null!;
    private Mock<IMongoDatabase> _mongoDbMock = null!;
    private Mock<ICacheService<OrderReadModel>> _cacheMock = null!;
    private Mock<IKafkaProducerService> _kafkaMock = null!;

    [SetUp]
    public void Setup()
    {
        _faker = new Faker();
        _writeRepoMock = new Mock<IWriteRepository<Order>>();
        _mongoDbMock = new Mock<IMongoDatabase>();
        _cacheMock = new Mock<ICacheService<OrderReadModel>>();
        _kafkaMock = new Mock<IKafkaProducerService>();

        var collectionMock = new Mock<IMongoCollection<OrderReadModel>>();
        collectionMock
            .Setup(x => x.InsertOneAsync(It.IsAny<OrderReadModel>(), null, default))
            .Returns(Task.CompletedTask);

        _mongoDbMock
            .Setup(db => db.GetCollection<OrderReadModel>("orders", null))
            .Returns(collectionMock.Object);
    }

    [Test]
    public async Task Handle_Should_Create_Order_And_Project_Correctly()
    {
        var handler = new CreateOrderCommandHandler(
            _writeRepoMock.Object,
            _mongoDbMock.Object,
            _cacheMock.Object,
            _kafkaMock.Object);

        var command = new CreateOrderCommand(
            _faker.Name.FullName(),
            _faker.Random.Decimal(10, 500));

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
        _writeRepoMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.SetAsync(
            $"order:{result}",
            It.Is<OrderReadModel>(o => o.Id == result),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _kafkaMock.Verify(x => x.PublishAsync(
            "orders.created",
            It.IsAny<OrderReadModel>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}