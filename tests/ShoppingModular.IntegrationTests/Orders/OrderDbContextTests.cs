using Microsoft.EntityFrameworkCore;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Orders;

namespace ShoppingModular.IntegrationTests.Orders;

public class OrderDbContextTests
{
    private OrderDbContext? _dbContext;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=shoppingdb")
            .Options;

        _dbContext = new OrderDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    [Test]
    public async Task Should_Connect_To_Postgres()
    {
        var canConnect = await _dbContext!.Database.CanConnectAsync();
        Assert.That(canConnect, Is.True);
    }

    [Test]
    public async Task Should_Insert_And_Read_Order()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "Heng Test",
            CreatedAt = DateTime.UtcNow,
            TotalAmount = 99
        };

        _dbContext!.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        var result = await _dbContext.Orders.FindAsync(order.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CustomerName, Is.EqualTo(order.CustomerName));
    }

    [Test]
    public async Task Should_List_All_Orders()
    {
        var orders = await _dbContext!.Orders.ToListAsync();
        Assert.That(orders, Is.Not.Null);
    }
}