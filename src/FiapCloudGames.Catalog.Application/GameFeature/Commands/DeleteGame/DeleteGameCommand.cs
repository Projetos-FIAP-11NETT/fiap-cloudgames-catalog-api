using MediatR;

namespace FiapCloudGames.Catalog.Application.GameFeature.Commands.DeleteGame;

public sealed record class DeleteGameCommand
(
    Guid Id
)
    : IRequest<bool>;
