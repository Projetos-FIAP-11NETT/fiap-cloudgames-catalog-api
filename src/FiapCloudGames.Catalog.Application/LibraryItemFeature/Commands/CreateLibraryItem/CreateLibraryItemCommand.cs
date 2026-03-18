using MediatR;

namespace FiapCloudGames.Catalog.Application.LibraryItemFeature.Commands.CreateLibraryItem;

public sealed record class CreateLibraryItemCommand
(
    Guid UserId, Guid GameId, int? OrderId = null
)
    : IRequest<bool>;