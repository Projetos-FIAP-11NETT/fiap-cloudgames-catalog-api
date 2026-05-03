using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.LibraryItemFeature.Queries.GetLibraryItemsByUser;

public class GetLibraryItemsByUserQueryHandler(
        ILibraryItemRepository mongoLibraryItemRepository
    )
    : IRequestHandler<GetLibraryItemsByUserQuery, IEnumerable<GetLibraryItemResponse>>
{
    public async Task<IEnumerable<GetLibraryItemResponse>> Handle(
        GetLibraryItemsByUserQuery query,
        CancellationToken cancellationToken)
    {
        var library = await mongoLibraryItemRepository.GetByUserIdAsync(query.UserId);

        if (library is null)
            return [];

        return library.Games.Select(g => new GetLibraryItemResponse
        {
            UserId = library.UserId,
            GameId = g.GameId,
            GameTitle = g.GameTitle,
            OrderId = g.OrderId,
            AddedAt = g.AddedAt
        });
    }
}