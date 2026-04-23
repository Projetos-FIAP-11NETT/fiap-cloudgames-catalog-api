using FiapCloudGames.Catalog.Domain.Entities;

namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories.NoSql;

public interface IMongoGameRepository
{
    Task UpsertAsync(Game game);
    Task DeleteAsync(Guid id);
}