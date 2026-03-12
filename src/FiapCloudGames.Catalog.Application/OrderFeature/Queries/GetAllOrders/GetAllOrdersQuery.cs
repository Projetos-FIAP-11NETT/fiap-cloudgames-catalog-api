using FiapCloudGames.Catalog.Application.DTOs;
using MediatR;

namespace FiapCloudGames.Catalog.Application.OrderFeature.Queries.GetAllOrders;

public sealed record class GetAllOrdersQuery : IRequest<IEnumerable<GetOrderResponse>>;