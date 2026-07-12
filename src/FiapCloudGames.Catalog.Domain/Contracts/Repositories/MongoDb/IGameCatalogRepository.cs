using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.ReadModels;

namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;

public interface IGameCatalogRepository
{
    Task UpsertAsync(
        Game game,
        GameCatalogMetadataReadModel metadata,
        GameCatalogRatingReadModel rating,
        CancellationToken cancellationToken);

    Task DeleteAsync(Guid gameId, CancellationToken cancellationToken);

    Task<List<GameCatalogReadModel>> GetCatalogAsync(
        string? filter,
        string? category,
        string? developer,
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken cancellationToken);
}