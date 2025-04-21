using KafkaProducerService;
using Microsoft.AspNetCore.Mvc;

namespace Orders.API.Controllers;

[ApiController]
[Route("api/test-kafka")]
public class KafkaTestController(IKafkaProducerService kafka) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendTestMessage()
    {
        var message = new
        {
            Id = Guid.NewGuid(),
            CustomerName = "Heng Test",
            CreatedAt = DateTime.UtcNow,
            TotalAmount = 123.45M
        };

        await kafka.PublishAsync("orders.created", message);
        return Ok("✔ Message published to Kafka");
    }
}