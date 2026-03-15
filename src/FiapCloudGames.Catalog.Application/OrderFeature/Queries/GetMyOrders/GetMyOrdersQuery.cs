using FiapCloudGames.Catalog.Application.DTOs;
using MediatR;

namespace FiapCloudGames.Catalog.Application.OrderFeature.Queries.GetMyOrders;

public sealed record class GetMyOrdersQuery(Guid UserId) : IRequest<IEnumerable<GetOrderResponse>>;