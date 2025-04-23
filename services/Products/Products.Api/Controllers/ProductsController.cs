using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Products.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /*
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }
    */

    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        // ainda será implementado com Redis → Mongo fallback
        return Ok();
    }
}