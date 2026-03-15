using MediatR;

namespace FiapCloudGames.Catalog.Application.OrderFeature.Commands.CreateOrder;

public sealed record class CreateOrderCommand
(
    Guid UserId, string Email, string Name, Guid GameId
)
    : IRequest<bool>;