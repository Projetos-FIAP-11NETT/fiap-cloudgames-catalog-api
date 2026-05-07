using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.OrderFeature.Commands.CreateOrder;

public sealed record class CreateOrderCommand
(
    Guid UserId, string Email, string Name, Guid GameId
)
    : IRequest<bool>;