using Bogus;
using Moq;
using NUnit.Framework;
using ShoppingModular.Application.Orders.Commands;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces;
using KafkaProducerService;
using MongoDB.Driver;
using ShoppingModular.Infrastructure.Interfaces.Order;

namespace ShoppingModular.IntegrationTests.Infrastructure;

[TestFixture]
public class CreateOrderUnitTests
{
    #region Fields

    private Faker _faker = null!;
    private Mock<IOrderWriteRepository> _writeRepo = null!;
    private Mock<ICacheService<OrderReadModel>> _cache = null!;
    private Mock<IKafkaProducerService> _kafka = null!;
    private Mock<IMongoDatabase> _mongoDb = null!;
    private Mock<IMongoCollection<OrderReadModel>> _mongoCollection = null!;

    #endregion

    #region Setup

    [SetUp]
    public void Setup()
    {
        _faker = new Faker();
        _writeRepo = new Mock<IOrderWriteRepository>();
        _cache = new Mock<ICacheService<OrderReadModel>>();
        _kafka = new Mock<IKafkaProducerService>();
        _mongoDb = new Mock<IMongoDatabase>();
        _mongoCollection = new Mock<IMongoCollection<OrderReadModel>>();

        // MongoDB configurado para retornar uma collection mockada
        _mongoDb.Setup(db => db.GetCollection<OrderReadModel>(
                It.IsAny<string>(),
                It.IsAny<MongoCollectionSettings?>()))
            .Returns(_mongoCollection.Object);

        // Evita erro com argumentos opcionais (nullables)
        _mongoCollection.Setup(c => c.InsertOneAsync(
            It.IsAny<OrderReadModel>(),
            It.IsAny<InsertOneOptions?>(),
            It.IsAny<CancellationToken>()));
    }

    #endregion

    #region Helpers

    private CreateOrderCommandHandler CreateHandler() =>
        new(_writeRepo.Object, _mongoDb.Object, _cache.Object, _kafka.Object);

    #endregion

    #region Tests

    [Test]
    public async Task Should_Create_Order_And_Project_To_All_Systems()
    {
        var command = new CreateOrderCommand(
            _faker.Name.FullName(),
            _faker.Random.Decimal(10, 100));

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.EqualTo(Guid.Empty), "Order ID should not be empty");
            _writeRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
            _mongoDb.Verify(db => db.GetCollection<OrderReadModel>("orders", null), Times.Once);
            _mongoCollection.Verify(c => c.InsertOneAsync(It.IsAny<OrderReadModel>(), null, default), Times.Once);
            _cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<OrderReadModel>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            _kafka.Verify(k => k.PublishAsync(It.IsAny<string>(), It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()), Times.Once);
        });
    }

    [Test]
    public async Task Should_Handle_Min_Amount_Successfully()
    {
        var command = new CreateOrderCommand("Edge Heng", 0.01m);
        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.EqualTo(Guid.Empty), "Order should be created even with minimal amount");
    }

    [Test]
    public void Should_Not_Cache_If_Repository_Fails()
    {
        _writeRepo.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failed"));

        var command = new CreateOrderCommand(_faker.Name.FullName(), 199);
        var handler = CreateHandler();

        Assert.ThrowsAsync<Exception>(async () => await handler.Handle(command, CancellationToken.None));
        _cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<OrderReadModel>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Should_Create_Valid_ReadModel_Structure()
    {
        var command = new CreateOrderCommand("Model Heng", 150);
        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        _cache.Verify(c => c.SetAsync(
            It.Is<string>(key => key.StartsWith("order:")),
            It.Is<OrderReadModel>(model =>
                model.CustomerName == "Model Heng" &&
                model.TotalAmount == 150 &&
                model.Id == result),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Exactly(1));
    }

    [Test]
    public void Should_Throw_When_Kafka_Fails()
    {
        _kafka.Setup(k => k.PublishAsync(It.IsAny<string>(), It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Kafka failure"));

        var command = new CreateOrderCommand(_faker.Name.FullName(), 120);
        var handler = CreateHandler();

        Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }

    #endregion
}