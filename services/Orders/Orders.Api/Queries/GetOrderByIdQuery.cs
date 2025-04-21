using MediatR;
using ShoppingModular.ReadModels.Orders;

namespace Orders.API.Queries;

/// <summary>
/// Query para buscar um pedido pelo ID.
/// </summary>
public record GetOrderByIdQuery(Guid Id) : IRequest<OrderReadModel?>;