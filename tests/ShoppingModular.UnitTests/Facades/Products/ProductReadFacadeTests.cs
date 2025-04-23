using Bogus;
using Moq;
using NUnit.Framework;
using ShoppingModular.Domain.Products;
using ShoppingModular.Infrastructure.Interfaces;
using ShoppingModular.Infrastructure.Products;

namespace ShoppingModular.Tests.Facades.Products;

[TestFixture]
public class ProductReadFacadeTests
{
    #region Fields

    private Faker _faker = null!;
    private Mock<IReadRepository<ProductReadModel>> _mongoMock = null!;
    private Mock<ICacheService<ProductReadModel>> _cacheMock = null!;
    private ProductReadFacade _facade = null!;

    #endregion

    #region Setup

    [SetUp]
    public void Setup()
    {
        _faker = new Faker();
        _mongoMock = new Mock<IReadRepository<ProductReadModel>>();
        _cacheMock = new Mock<ICacheService<ProductReadModel>>();
        _facade = new ProductReadFacade(_mongoMock.Object, _cacheMock.Object);
    }

    #endregion

    #region Tests

    [Test]
    public async Task GetByIdAsync_Should_Return_From_Cache_If_Found()
    {
        var id = Guid.NewGuid();
        var cached = _faker.GenerateProductReadModel(id);

        _cacheMock.Setup(x => x.GetAsync($"product:{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await _facade.GetByIdAsync(id);

        Assert.That(result, Is.EqualTo(cached));
        _mongoMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_Should_Fallback_To_Mongo_If_Cache_Miss()
    {
        var id = Guid.NewGuid();
        var fromMongo = _faker.GenerateProductReadModel(id);

        _cacheMock.Setup(x => x.GetAsync($"product:{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductReadModel?)null);

        _mongoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromMongo);

        var result = await _facade.GetByIdAsync(id);

        Assert.That(result, Is.EqualTo(fromMongo));
        _cacheMock.Verify(x => x.SetAsync($"product:{id}", fromMongo, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_Should_Return_Null_If_Not_Found_In_Cache_Or_Mongo()
    {
        var id = Guid.NewGuid();

        _cacheMock.Setup(x => x.GetAsync($"product:{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductReadModel?)null);

        _mongoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductReadModel?)null);

        var result = await _facade.GetByIdAsync(id);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_Should_Cache_Result_From_Mongo()
    {
        var id = Guid.NewGuid();
        var model = _faker.GenerateProductReadModel(id);

        _cacheMock.Setup(x => x.GetAsync($"product:{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductReadModel?)null);

        _mongoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        await _facade.GetByIdAsync(id);

        _cacheMock.Verify(x => x.SetAsync(
            $"product:{id}", model, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}

#region Faker Extensions

public static class FakerProductExtensions
{
    public static ProductReadModel GenerateProductReadModel(this Faker faker, Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = faker.Commerce.ProductName(),
            Description = faker.Commerce.ProductDescription(),
            Price = faker.Random.Decimal(10, 1000),
            Stock = faker.Random.Int(1, 50),
            Category = faker.Commerce.Categories(1)[0],
            Tags = new() { "tag1", "tag2" },
            Images = new() { "img1.png", "img2.png" },
            CreatedAt = DateTime.UtcNow
        };
}

#endregion