using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Commands.DeleteGame;

public sealed record class DeleteGameCommand
(
    Guid Id
)
    : IRequest<bool>;
