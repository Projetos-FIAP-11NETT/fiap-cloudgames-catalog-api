using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using MediatR;

namespace FiapCloudGames.Catalog.Application.LibraryItemFeature.Queries.GetLibraryItemsByUser;

public class GetLibraryItemsByUserQueryHandler(ILibraryItemRepository libraryItemRepository)
    : IRequestHandler<GetLibraryItemsByUserQuery, IEnumerable<GetLibraryItemResponse>>
{
    public async Task<IEnumerable<GetLibraryItemResponse>> Handle(GetLibraryItemsByUserQuery query, CancellationToken cancellationToken)
    {
        var items = await libraryItemRepository.GetByUserIdAsync(query.UserId);

        return items.Select(l => new GetLibraryItemResponse
        {
            Id = l.Id,
            UserId = l.UserId,
            GameId = l.GameId,
            GameTitle = l.Game.Title,
            OrderId = l.OrderId,
            AddedAt = l.AddedAt
        });
    }
}