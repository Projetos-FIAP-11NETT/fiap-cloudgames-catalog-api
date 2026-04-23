using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational.Generic;
using FiapCloudGames.Catalog.Domain.Entities;

namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;

public interface ICategoryRepository : IRepository<Category> { 
    Task<bool> AnyCategoryByNameAsync(string name);
    Task<List<Category>> GetCategory(Guid? id, string? name);
    Task<List<Category>> GetByIdsAsync(IEnumerable<Guid> ids);
}