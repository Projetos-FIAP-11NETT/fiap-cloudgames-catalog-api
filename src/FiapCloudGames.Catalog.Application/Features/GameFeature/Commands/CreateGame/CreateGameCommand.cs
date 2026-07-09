using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Commands.CreateGame;

public sealed record class CreateGameCommand
(
    string Title, string Description, DateTime ReleaseDate, string Developer, decimal Price, List<Guid> Categories
) 
    : IRequest<bool>;