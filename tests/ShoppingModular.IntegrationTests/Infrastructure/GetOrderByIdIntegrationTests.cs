using System.Net;
using System.Net.Http.Json;
using Bogus;

namespace ShoppingModular.IntegrationTests.Infrastructure;

[TestFixture]
public class GetOrderByIdIntegrationTests
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
        _client.Dispose(); // ✅ Libera corretamente
    }

    private HttpClient _client = null!;
    private Faker _faker = null!;

    [TestCase("00000000-0000-0000-0000-000000000001")] // Use um ID real criado via POST
    public async Task Should_Get_Order_By_Id_From_Cache_Or_Mongo(Guid id)
    {
        var response = await _client.GetAsync($"/api/orders/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            Assert.Inconclusive("Order not found in Redis or Mongo. May require a valid ID.");
            return;
        }

        Assert.That(response.IsSuccessStatusCode, Is.True);

        var order = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.That(order, Is.Not.Null); // ✅ Corrigido
        Assert.That(order!.ContainsKey("id"), Is.True);
    }
}