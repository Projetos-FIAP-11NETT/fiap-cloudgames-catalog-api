using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Infrastructure.Data.Relational;
using FiapCloudGames.Catalog.Infrastructure.Repositories.Relational.Generic;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.Relational;

public class CategoryRepository(AppDbContext dataContext) : Repository<Category>(dataContext), ICategoryRepository 
{
    
    public async Task<bool> AnyCategoryByNameAsync(string name)
    {
        return await dataContext.Categories
            .AnyAsync(r => r.Name == name);
    }

    public async Task<List<Category>> GetCategory(Guid? id, string name)
    {
        var query = dataContext.Categories.AsQueryable();

        if(id.HasValue)
        {
            query = query.Where(r => r.Id == id.Value);
        }

        if(!string.IsNullOrEmpty(name))
        {
            query = query.Where(r => r.Name == name);
        }
        return await query.ToListAsync();
    }

    public Task<List<Category>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return dataContext.Categories.Where(r => ids.Contains(r.Id)).ToListAsync();
    }

    public async Task<Category?> GetCategoryByNameAsync(string name)
    {
        return await dataContext.Categories
            .FirstOrDefaultAsync(r => r.Name == name);
    }
}