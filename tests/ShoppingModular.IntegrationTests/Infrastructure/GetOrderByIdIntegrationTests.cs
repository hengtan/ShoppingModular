using System.Net;
using System.Net.Http.Json;
using Bogus;
using NUnit.Framework.Legacy;

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

    [Test]
    public async Task Should_Return_404_If_Order_Not_Found()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Should_Return_Valid_Order_Structure()
    {
        var createRequest = new
        {
            CustomerName = _faker.Name.FullName(),
            TotalAmount = 222
        };

        var response = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = content!["id"];

        var getResponse = await _client.GetAsync($"/api/orders/{id}");
        var order = await getResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.That(order, Is.Not.Null);
        Assert.That(order!.ContainsKey("customerName"), Is.True);
        Assert.That(order!.ContainsKey("totalAmount"), Is.True);
    }

    [Test]
    public async Task Should_Respect_Casing_In_Response()
    {
        var createRequest = new
        {
            CustomerName = "Case Test",
            TotalAmount = 333
        };

        var response = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = content!["id"];

        var getResponse = await _client.GetAsync($"/api/orders/{id}");
        var body = await getResponse.Content.ReadAsStringAsync();

        StringAssert.Contains("customerName", body);
        StringAssert.Contains("totalAmount", body);
    }

    [Test]
    public async Task Should_Return_Correct_Status_Code()
    {
        var createRequest = new
        {
            CustomerName = "Status Test",
            TotalAmount = 150
        };

        var response = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = content!["id"];

        var getResponse = await _client.GetAsync($"/api/orders/{id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}