using MediatR;

namespace ShoppingModular.Application.Orders.Commands;

/// <summary>
/// Comando responsável por criar um novo pedido.
/// </summary>
public record CreateOrderCommand(string CustomerName, decimal TotalAmount) : IRequest<Guid>;