using MediatR;

namespace FiapCloudGames.Catalog.Application.OrderFeature.Commands.CreateOrder;

public sealed record class CreateOrderCommand
(
    string UserId, string Email, string Name, Guid GameId
)
    : IRequest<bool>;