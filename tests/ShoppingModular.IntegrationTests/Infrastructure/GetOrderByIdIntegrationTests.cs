using Bogus;
using Moq;
using ShoppingModular.Application.Orders.Queries;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces;

namespace ShoppingModular.IntegrationTests.Infrastructure;

[TestFixture]
public class GetOrderByIdUnitTests
{
    private Faker _faker = null!;
    private Mock<IOrderReadFacade> _facade = null!;
    private GetOrderByIdQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _faker = new Faker();
        _facade = new Mock<IOrderReadFacade>();
        _handler = new GetOrderByIdQueryHandler(_facade.Object);
    }

    [Test]
    public async Task Should_Return_Order_When_Found_In_Cache_Or_Mongo()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expected = new OrderReadModel
        {
            Id = id,
            CustomerName = _faker.Name.FullName(),
            TotalAmount = _faker.Random.Decimal(100, 500),
            CreatedAt = DateTime.UtcNow
        };

        _facade.Setup(f => f.GetByIdAsync(id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(expected);

        // Act
        var result = await _handler.Handle(new GetOrderByIdQuery(id), CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(id));
        Assert.That(result.CustomerName, Is.EqualTo(expected.CustomerName));
    }

    [Test]
    public async Task Should_Return_Null_When_Not_Found()
    {
        var id = Guid.NewGuid();

        _facade.Setup(f => f.GetByIdAsync(id, It.IsAny<CancellationToken>()))
               .ReturnsAsync((OrderReadModel?)null);

        var result = await _handler.Handle(new GetOrderByIdQuery(id), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Should_Contain_Valid_Fields()
    {
        var id = Guid.NewGuid();
        var expected = new OrderReadModel
        {
            Id = id,
            CustomerName = "Field Check",
            TotalAmount = 250,
            CreatedAt = DateTime.UtcNow
        };

        _facade.Setup(f => f.GetByIdAsync(id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(expected);

        var result = await _handler.Handle(new GetOrderByIdQuery(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.CustomerName, Is.EqualTo("Field Check"));
            Assert.That(result.TotalAmount, Is.EqualTo(250));
        });
    }

    [Test]
    public async Task Should_Fallback_To_Mongo_When_Cache_Is_Null()
    {
        var id = Guid.NewGuid();
        var expected = new OrderReadModel { Id = id, CustomerName = "Mongo Heng", TotalAmount = 150, CreatedAt = DateTime.UtcNow };

        _facade.Setup(f => f.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _handler.Handle(new GetOrderByIdQuery(id), CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CustomerName, Is.EqualTo("Mongo Heng"));
    }

    [Test]
    public async Task Should_Handle_Timeout_Cancellation()
    {
        var cts = new CancellationTokenSource(1); // cancela rapidamente
        var id = Guid.NewGuid();

        _facade.Setup(f => f.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await _handler.Handle(new GetOrderByIdQuery(id), cts.Token);
        });
    }

    [Test]
    public async Task Should_Log_If_Order_Not_Found()
    {
        var id = Guid.NewGuid();

        _facade.Setup(f => f.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderReadModel?)null);

        var result = await _handler.Handle(new GetOrderByIdQuery(id), CancellationToken.None);

        Assert.That(result, Is.Null);
        _facade.Verify(f => f.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Should_Return_Correct_Structure_When_Valid()
    {
        var id = Guid.NewGuid();
        var order = new OrderReadModel
        {
            Id = id,
            CustomerName = "Struct Heng",
            TotalAmount = 321,
            CreatedAt = DateTime.UtcNow
        };

        _facade.Setup(f => f.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new GetOrderByIdQuery(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.CustomerName, Is.EqualTo("Struct Heng"));
            Assert.That(result.TotalAmount, Is.EqualTo(321));
            Assert.That(result.CreatedAt, Is.Not.EqualTo(default));
        });
    }
}