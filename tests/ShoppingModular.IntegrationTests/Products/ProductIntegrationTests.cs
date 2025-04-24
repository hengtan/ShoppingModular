// Arquivo: CreateProductUnitTests.cs

using Bogus;
using KafkaProducerService;
using MongoDB.Driver;
using Moq;
using ShoppingModular.Application.Products.Commands;
using ShoppingModular.Domain.Products;
using ShoppingModular.Infrastructure.Interfaces;
using ShoppingModular.Infrastructure.Interfaces.Products;

namespace ShoppingModular.IntegrationTests.Products;

[TestFixture]
public class CreateProductUnitTests
{
    [SetUp]
    public void Setup()
    {
        _faker = new Faker();
        _writeRepo = new Mock<IProductWriteRepository>();
        _cache = new Mock<ICacheService<ProductReadModel>>();
        _kafka = new Mock<IKafkaProducerService>();
        _mongoDb = new Mock<IMongoDatabase>();
        _mongoCollection = new Mock<IMongoCollection<ProductReadModel>>();

        _mongoDb.Setup(db => db.GetCollection<ProductReadModel>(
                It.IsAny<string>(),
                It.IsAny<MongoCollectionSettings?>()))
            .Returns(_mongoCollection.Object);

        _mongoCollection.Setup(c => c.InsertOneAsync(
            It.IsAny<ProductReadModel>(),
            It.IsAny<InsertOneOptions?>(),
            It.IsAny<CancellationToken>()));
    }

    private Faker _faker = null!;
    private Mock<IProductWriteRepository> _writeRepo = null!;
    private Mock<ICacheService<ProductReadModel>> _cache = null!;
    private Mock<IKafkaProducerService> _kafka = null!;
    private Mock<IMongoDatabase> _mongoDb = null!;
    private Mock<IMongoCollection<ProductReadModel>> _mongoCollection = null!;

    private CreateProductCommandHandler CreateHandler()
    {
        return new CreateProductCommandHandler(_writeRepo.Object, _mongoDb.Object, _cache.Object, _kafka.Object);
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
            Tags = new[] { "tag1", "tag2" }.ToList(),
            Images = new[] { "http://img1.jpg", "http://img2.jpg" }.ToList()
        };
    }

    [Test]
    public async Task Should_Create_Product_And_Project_To_All_Systems()
    {
        var command = GenerateValidCommand();
        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.EqualTo(Guid.Empty));
            _writeRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
            _mongoDb.Verify(db => db.GetCollection<ProductReadModel>("products", null), Times.Once);
            _mongoCollection.Verify(c => c.InsertOneAsync(It.IsAny<ProductReadModel>(), null, default), Times.Once);
            _cache.Verify(
                c => c.SetAsync(It.IsAny<string>(), It.IsAny<ProductReadModel>(), It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()), Times.Once);
            _kafka.Verify(
                k => k.PublishAsync("products.created", It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()),
                Times.Once);
        });
    }

    [Test]
    public async Task Should_Handle_Min_Amount_Successfully()
    {
        var command = new CreateProductCommand
        {
            Name = "Edge Heng",
            Description = "Edge Test",
            Price = 0.01m,
            Stock = 1,
            Category = "Test",
            Tags = new[] { "tag" }.ToList(),
            Images = new[] { "img.jpg" }.ToList()
        };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void Should_Not_Cache_If_Repository_Fails()
    {
        _writeRepo.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failed"));

        var command = GenerateValidCommand();
        var handler = CreateHandler();

        Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        _cache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<ProductReadModel>(), It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Should_Create_Valid_ReadModel_Structure()
    {
        var command = new CreateProductCommand
        {
            Name = "Product X",
            Description = "Test Product",
            Price = 150,
            Stock = 20,
            Category = "CategoryX",
            Tags = new[] { "one" }.ToList(),
            Images = new[] { "img.jpg" }.ToList()
        };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        _cache.Verify(c => c.SetAsync(
            It.Is<string>(key => key.StartsWith("product:")),
            It.Is<ProductReadModel>(model =>
                model.Name == command.Name &&
                model.Description == command.Description &&
                model.Price == command.Price &&
                model.Stock == command.Stock),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Should_Throw_When_Kafka_Fails()
    {
        _kafka.Setup(k =>
                k.PublishAsync(It.IsAny<string>(), It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Kafka failure"));

        var command = GenerateValidCommand();
        var handler = CreateHandler();

        Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }
}