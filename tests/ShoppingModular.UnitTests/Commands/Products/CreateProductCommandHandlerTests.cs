using Bogus;
using KafkaProducerService;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using ShoppingModular.Application.Products.Commands;
using ShoppingModular.Domain.Products;
using ShoppingModular.Infrastructure.Interfaces;
using ShoppingModular.Infrastructure.Interfaces.Products;

namespace ShoppingModular.Tests.Commands.Products;

[TestFixture]
public class CreateProductCommandHandlerTests
{
    [SetUp]
    public void Setup()
    {
        _faker = new Faker();
        _writeRepo = new Mock<IProductWriteRepository>();
        _mongoDb = new Mock<IMongoDatabase>();
        _mongoCollection = new Mock<IMongoCollection<ProductReadModel>>();
        _cache = new Mock<ICacheService<ProductReadModel>>();
        _kafka = new Mock<IKafkaProducerService>();

        _mongoDb.Setup(x =>
                x.GetCollection<ProductReadModel>("products", null))
            .Returns(_mongoCollection.Object);
    }

    private Faker _faker = null!;
    private Mock<IProductWriteRepository> _writeRepo = null!;
    private Mock<IMongoDatabase> _mongoDb = null!;
    private Mock<IMongoCollection<ProductReadModel>> _mongoCollection = null!;
    private Mock<ICacheService<ProductReadModel>> _cache = null!;
    private Mock<IKafkaProducerService> _kafka = null!;

    private CreateProductCommandHandler CreateHandler()
    {
        return new CreateProductCommandHandler(_writeRepo.Object, _mongoDb.Object, _cache.Object, _kafka.Object);
    }

    [Test]
    public async Task Should_Create_And_Return_Id()
    {
        var command = GenerateValidCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, default);

        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task Should_Persist_Product_In_Postgres()
    {
        var command = GenerateValidCommand();
        var handler = CreateHandler();

        await handler.Handle(command, default);

        _writeRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
    }

    [Test]
    public async Task Should_Project_Product_To_Mongo()
    {
        var command = GenerateValidCommand();
        var handler = CreateHandler();

        await handler.Handle(command, default);

        _mongoCollection.Verify(m => m.InsertOneAsync(
            It.IsAny<ProductReadModel>(), null, default), Times.Once);
    }

    [Test]
    public async Task Should_Cache_Product_In_Redis()
    {
        var command = GenerateValidCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, default);

        _cache.Verify(c => c.SetAsync(
            $"product:{result}",
            It.Is<ProductReadModel>(x => x.Id == result),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Should_Publish_Event_To_Kafka()
    {
        var command = GenerateValidCommand();
        var handler = CreateHandler();

        await handler.Handle(command, default);

        _kafka.Verify(k => k.PublishAsync(
            "products.created",
            It.IsAny<ProductReadModel>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Should_Throw_If_Write_Fails()
    {
        var command = GenerateValidCommand();
        _writeRepo.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var handler = CreateHandler();

        Assert.ThrowsAsync<Exception>(() => handler.Handle(command, default));
    }

    [Test]
    public async Task Should_Handle_Min_Valid_Stock_And_Price()
    {
        var command = new CreateProductCommand
        {
            Name = "Edge",
            Description = "Valid min test",
            Price = 0.01m,
            Stock = 1,
            Category = "Test",
            Tags = new List<string> { "a" },
            Images = new List<string> { "img.png" }
        };

        var handler = CreateHandler();
        var result = await handler.Handle(command, default);

        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task Should_Respect_Structured_Data()
    {
        var command = GenerateValidCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, default);

        _cache.Verify(x => x.SetAsync(
            $"product:{result}",
            It.Is<ProductReadModel>(p =>
                p.Tags.Contains("tag1") &&
                p.Images.Any(i => i.EndsWith(".jpg"))),
            It.IsAny<TimeSpan>(), default), Times.Once);
    }

    [Test]
    public async Task Should_Create_Full_ReadModel()
    {
        var command = GenerateValidCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, default);

        _mongoCollection.Verify(x => x.InsertOneAsync(
            It.Is<ProductReadModel>(p =>
                p.Id == result &&
                p.Name == command.Name &&
                p.Description == command.Description),
            null, default), Times.Once);
    }

    [Test]
    public async Task Should_Handle_High_Load()
    {
        var handler = CreateHandler();
        var commands = Enumerable.Range(0, 20).Select(_ => GenerateValidCommand());

        foreach (var cmd in commands)
        {
            var id = await handler.Handle(cmd, default);
            Assert.That(id, Is.Not.EqualTo(Guid.Empty));
        }
    }

    private CreateProductCommand GenerateValidCommand()
    {
        return new CreateProductCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Commerce.ProductDescription(),
            Price = _faker.Random.Decimal(1, 500),
            Stock = _faker.Random.Int(1, 100),
            Category = _faker.Commerce.Categories(1)[0],
            Tags = new List<string> { "tag1", "tag2" },
            Images = new List<string> { "http://image1.jpg", "http://image2.jpg" }
        };
    }
}