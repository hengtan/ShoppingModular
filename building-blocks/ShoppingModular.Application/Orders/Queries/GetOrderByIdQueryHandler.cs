using MediatR;
using ShoppingModular.Domain.Orders;
using ShoppingModular.Infrastructure.Interfaces.Order;

namespace ShoppingModular.Application.Orders.Queries;

/// <summary>
///     Handler responsável por buscar o pedido com cache-aside (Redis -> Mongo fallback).
/// </summary>
public class GetOrderByIdQueryHandler(IOrderReadFacade facade)
    : IRequestHandler<GetOrderByIdQuery, OrderReadModel?>
{
    public async Task<OrderReadModel?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return await facade.GetByIdAsync(request.Id, cancellationToken);
    }
}