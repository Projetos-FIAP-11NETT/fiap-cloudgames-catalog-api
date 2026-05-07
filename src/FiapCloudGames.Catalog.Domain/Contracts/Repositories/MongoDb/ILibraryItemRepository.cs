using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.ReadModels;

namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;

public interface ILibraryItemRepository
{
    Task InsertAsync(LibraryItem item, string gameTitle, decimal gamePrice);
    Task<LibraryItemReadModel?> GetByUserIdAsync(Guid userId);
    Task<bool> ExistsAsync(Guid userId, Guid gameId);
    Task DeleteAsync(Guid id);
}