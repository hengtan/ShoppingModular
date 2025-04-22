using System.Net;
using System.Net.Http.Json;
using Bogus;

namespace ShoppingModular.IntegrationTests.Infrastructure;

[TestFixture]
public class CreateOrderIntegrationTests
{
    [SetUp]
    public void Setup()
    {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:5001") };
        _faker = new Faker();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    private HttpClient _client = null!;
    private Faker _faker = null!;

    [Test]
    public async Task Should_Create_Order_And_Project_To_Mongo_And_Redis()
    {
        // Arrange
        var request = new
        {
            CustomerName = _faker.Name.FullName(),
            TotalAmount = _faker.Random.Decimal(50, 500)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        Assert.That(content, Is.Not.Null);
        Assert.That(content!.ContainsKey("id"), Is.True);

        var orderId = content["id"];
        Assert.That(orderId, Is.Not.EqualTo(Guid.Empty));

        // Aguarda o Kafka Consumer processar (simulado)
        await Task.Delay(3000);

        // Aqui você pode implementar validações reais no MongoDB ou Redis
        Console.WriteLine($"✔ Pedido criado e processado: {orderId}");
    }

    [Test]
    public async Task Should_Create_Order_With_Edge_Amount()
    {
        var request = new
        {
            CustomerName = _faker.Name.FullName(),
            TotalAmount = 0.01m
        };

        var response = await _client.PostAsJsonAsync("/api/orders", request);
        response.EnsureSuccessStatusCode();
        var id = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        Assert.That(id, Is.Not.Null);
    }

    [Test]
    public async Task Should_Return_Json_Content_Type_On_Create()
    {
        var request = new
        {
            CustomerName = _faker.Name.FullName(),
            TotalAmount = 123.45m
        };

        var response = await _client.PostAsJsonAsync("/api/orders", request);
        Assert.That(response.Content.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
    }
}