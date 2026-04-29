using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.OrderFeature.Queries.GetAllOrders;

public class GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetAllOrdersQuery, IEnumerable<GetOrderResponse>>
{
    public async Task<IEnumerable<GetOrderResponse>> Handle(GetAllOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllOrdersAsync();

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