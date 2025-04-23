using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShoppingModular.Application.Products.Commands;
using ShoppingModular.Infrastructure.Interfaces.Products;

namespace Products.Api.Controllers;

/// <summary>
/// Controller responsável por operações de produtos.
/// </summary>
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IProductReadFacade _readFacade;

    public ProductsController(IMediator mediator, IProductReadFacade readFacade)
    {
        _mediator = mediator;
        _readFacade = readFacade;
    }

    #region POST /api/products

    /// <summary>
    /// Cria um novo produto.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    #endregion

    #region GET /api/products/{id}

    /// <summary>
    /// Busca um produto por ID com fallback de Redis para MongoDB.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _readFacade.GetByIdAsync(id);
        if (product is null)
            return NotFound();

        return Ok(product);
    }

    #endregion
}