using MediatR;
using ShoppingModular.Domain.Orders;

namespace ShoppingModular.Application.Orders.Queries;

/// <summary>
///     Query para buscar um pedido pelo ID.
/// </summary>
public record GetOrderByIdQuery(Guid Id) : IRequest<OrderReadModel?>;