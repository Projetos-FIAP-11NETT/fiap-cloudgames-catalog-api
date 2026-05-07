using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Infrastructure.Data.Relational;
using FiapCloudGames.Catalog.Infrastructure.Repositories.Postgres.Generic;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.Postgres;

public class GameRepository(AppDbContext dataContext) : Repository<Game>(dataContext), IGameRepository 
{
    public async Task<bool> GameAlreadyExistsByTitle(string title, string developer)
    {
        var normalizedTitle = title.ToLower();
        var normalizedDeveloper = developer.ToLower();
        return await dataContext.Games.AnyAsync(x => x.Title.ToLower() == normalizedTitle && x.Developer == normalizedDeveloper);
    }

    public async Task<bool> GameAlreadyExistsByTitle(string title, string developer, Guid excludeId)
    {
        var normalizedTitle = title.ToLower();
        var normalizedDeveloper = developer.ToLower();
        return await dataContext.Games.AnyAsync(x => x.Id != excludeId && x.Title.ToLower() == normalizedTitle && x.Developer.ToLower() == normalizedDeveloper);
    }

    public async Task<Game?> GetByIdWithCategoriesAsync(Guid id)
        => await dataContext.Games
            .Include(g => g.Categories)
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task<List<Game>> GetGame(string filter)
    {
        var pattern = $"%{filter}%";

        return await dataContext.Games
            .Where(x =>
                EF.Functions.ILike(x.Id.ToString(), pattern) ||
                EF.Functions.ILike(x.Title, pattern) ||
                EF.Functions.ILike(x.Developer, pattern)
            )
            .Include(g => g.Categories)
            .ToListAsync();
    }
}