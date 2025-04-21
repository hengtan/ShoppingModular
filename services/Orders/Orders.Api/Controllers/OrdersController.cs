using KafkaProducerService;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShoppingModular.Application.Orders.Commands;
using ShoppingModular.Application.Orders.Queries;
using ShoppingModular.Domain.Orders;

namespace Orders.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderCommand command, CancellationToken ct)
    {
        var id = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetOrderById), new { id }, new { id });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderReadModel>> GetOrderById(Guid id, CancellationToken ct)
    {
        var order = await sender.Send(new GetOrderByIdQuery(id), ct);
        return order is null ? NotFound() : Ok(order);
    }
}