using Bogus;
using Moq;
using NUnit.Framework;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces;
using ShoppingModular.Infrastructure.Orders;

namespace ShoppingModular.Tests.Facades;

[TestFixture]
public class OrderReadFacadeTests
{
    [SetUp]
    public void Setup()
    {
        _faker = new Faker();
        _mongoMock = new Mock<IReadRepository<OrderReadModel>>();
        _cacheMock = new Mock<ICacheService<OrderReadModel>>();
        _facade = new OrderReadFacade(_mongoMock.Object, _cacheMock.Object);
    }

    private Faker _faker = null!;
    private Mock<IReadRepository<OrderReadModel>> _mongoMock = null!;
    private Mock<ICacheService<OrderReadModel>> _cacheMock = null!;
    private OrderReadFacade _facade = null!;

    [Test]
    public async Task GetByIdAsync_Should_Return_From_Cache_If_Found()
    {
        var id = Guid.NewGuid();
        var fake = new Faker<OrderReadModel>().RuleFor(x => x.Id, id).Generate();

        _cacheMock.Setup(x => x.GetAsync($"order:{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(fake);

        var result = await _facade.GetByIdAsync(id);

        Assert.That(result, Is.EqualTo(fake));
        _mongoMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_Should_Fallback_To_Mongo_If_Cache_Miss()
    {
        var id = Guid.NewGuid();
        var fake = new Faker<OrderReadModel>().RuleFor(x => x.Id, id).Generate();

        _cacheMock.Setup(x => x.GetAsync($"order:{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderReadModel?)null);

        _mongoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fake);

        var result = await _facade.GetByIdAsync(id);

        Assert.That(result, Is.EqualTo(fake));
        _cacheMock.Verify(x => x.SetAsync($"order:{id}", fake, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}