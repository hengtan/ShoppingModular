using MongoDB.Driver;
using Npgsql;
using StackExchange.Redis;

namespace ShoppingModular.IntegrationTests.Orders;

[TestFixture]
public class DatabaseConnectivityTests
{
    [Test]
    public void Should_Connect_To_PostgreSQL()
    {
        var connString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=shoppingdb";
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        Assert.That(conn.State == System.Data.ConnectionState.Open);
    }

    [Test]
    public void Should_Connect_To_MongoDB()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        var db = client.GetDatabase("shoppingdb");
        var collections = db.ListCollectionNames().ToList();
        Assert.That(collections, Is.Not.Null);
    }

    [Test]
    public void Should_Connect_To_Redis()
    {
        var redis = ConnectionMultiplexer.Connect("localhost:6379");
        var db = redis.GetDatabase();
        Assert.That(db.Ping().TotalMilliseconds >= 0);
    }
}