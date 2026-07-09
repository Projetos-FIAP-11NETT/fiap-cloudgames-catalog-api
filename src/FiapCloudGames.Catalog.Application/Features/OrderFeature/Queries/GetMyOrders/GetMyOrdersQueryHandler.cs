using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.OrderFeature.Queries.GetMyOrders;

public class GetMyOrdersQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetMyOrdersQuery, IEnumerable<GetOrderResponse>>
{
    public async Task<IEnumerable<GetOrderResponse>> Handle(GetMyOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetOrdersByUserIdAsync(query.UserId);

        return orders.Select(o => new GetOrderResponse
        {
            Id = o.Id,
            UserId = o.UserId,
            GameId = o.GameId,
            GameTitle = o.Game.Title,
            Status = o.Status,
            TotalAmount = o.TotalAmount,
            CreatedAt = o.CreatedAt,
            PaidAt = o.PaidAt
        });
    }
}