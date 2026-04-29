using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Infrastructure.Data.Relational;
using FiapCloudGames.Catalog.Infrastructure.Repositories.Relational.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.Relational;

public class GameRepository(AppDbContext dataContext) : Repository<Game>(dataContext), IGameRepository 
{
    public async Task<bool> GameAlreadyExistsByTitle(string title, string developer)
    {
        var normalizedTitle = title.ToLower();
        var normalizedDeveloper = developer.ToLower();
        return await dataContext.Games.AnyAsync(x=>x.Title.ToLower() == normalizedTitle.ToLower() && x.Developer == normalizedDeveloper);
    }

    public async Task<bool> GameAlreadyExistsByTitle(string title, string developer, Guid excludeId)
    {
        var normalizedTitle = title.ToLower();
        var normalizedDeveloper = developer.ToLower();
        return await dataContext.Games.AnyAsync(x => x.Id != excludeId && x.Title.ToLower() == normalizedTitle && x.Developer.ToLower() == normalizedDeveloper);
    }

    public async Task<List<Game>> GetGame(string filter)
    {
        filter = $"%{filter}%";

        var query = dataContext.Games
            .Where(x =>
                EF.Functions.ILike(x.Id.ToString(), filter) ||
                EF.Functions.ILike(x.Title, filter) ||
                EF.Functions.ILike(x.Developer, filter)
            )
            .Include(g => g.Categories)
            .AsQueryable();

        return await query.ToListAsync();
    }
}