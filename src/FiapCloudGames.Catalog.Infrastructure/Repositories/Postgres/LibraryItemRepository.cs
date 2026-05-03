using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Infrastructure.Data.Relational;
using FiapCloudGames.Catalog.Infrastructure.Repositories.Relational.Generic;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.Relational;

public class LibraryItemRepository(AppDbContext dataContext)
    : Repository<LibraryItem>(dataContext), ILibraryItemRepository
{
    public async Task<List<LibraryItem>> GetByUserIdAsync(Guid userId)
    {
        return await dataContext.LibraryItems
            .Include(l => l.Game)
            .Where(l => l.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid gameId)
    {
        return await dataContext.LibraryItems
            .AnyAsync(l => l.UserId == userId && l.GameId == gameId);
    }
}