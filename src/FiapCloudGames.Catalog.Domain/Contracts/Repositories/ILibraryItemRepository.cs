using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Generic;
using FiapCloudGames.Catalog.Domain.Entities;

namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories;

public interface ILibraryItemRepository : IRepository<LibraryItem>
{
    Task<List<LibraryItem>> GetByUserIdAsync(Guid userId);
    Task<bool> ExistsAsync(Guid userId, Guid gameId);
}