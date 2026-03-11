using MediatR;

namespace FiapCloudGames.Catalog.Application.GameFeature.Commands.UpdateGame;

public sealed record class UpdateGameCommand
(
    Guid Id,
    string Title,
    string Description,
    DateTime ReleaseDate,
    string Developer,
    decimal Price,
    List<Guid> Categories
)
    : IRequest<bool>;
