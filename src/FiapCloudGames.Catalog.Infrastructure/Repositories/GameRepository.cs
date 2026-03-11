using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Infrastructure.Data;
using FiapCloudGames.Catalog.Infrastructure.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories;

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

    public async Task<List<Game>> GetGame(Guid? id, string? title)
    {
        var query = dataContext.Games
            .Include(g => g.Categories)
            .AsQueryable();

        if (id.HasValue)
        {
            query = query.Where(g => g.Id == id.Value);
        }

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(g => g.Title == title);
        }

        return await query.ToListAsync();
    }
}