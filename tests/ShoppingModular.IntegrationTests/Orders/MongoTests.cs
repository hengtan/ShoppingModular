using MongoDB.Driver;
using ShoppingModular.Domain.Orders;

namespace ShoppingModular.IntegrationTests.Orders;

public class MongoTests
{
    private IMongoDatabase _db = null!;

    [SetUp]
    public void Setup()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        _db = client.GetDatabase("shoppingdb");
    }

    [Test]
    public async Task Should_Connect_To_Mongo()
    {
        var names = await _db.ListCollectionNames().ToListAsync();
        Assert.That(names, Is.Not.Null);
    }

    [Test]
    public async Task Should_Insert_And_Read_From_Mongo()
    {
        var collection = _db.GetCollection<OrderReadModel>("orders");

        var order = new OrderReadModel
        {
            Id = Guid.NewGuid(),
            CustomerName = "Mongo Heng",
            CreatedAt = DateTime.UtcNow,
            TotalAmount = 101
        };

        await collection.InsertOneAsync(order);
        var found = await collection.Find(o => o.Id == order.Id).FirstOrDefaultAsync();

        Assert.That(found, Is.Not.Null);
        Assert.That(found.CustomerName, Is.EqualTo(order.CustomerName));
    }

    [Test]
    public async Task Should_List_All_From_Mongo()
    {
        var collection = _db.GetCollection<OrderReadModel>("orders");
        var all = await collection.Find(FilterDefinition<OrderReadModel>.Empty).ToListAsync();
        Assert.That(all, Is.Not.Empty);
    }
}