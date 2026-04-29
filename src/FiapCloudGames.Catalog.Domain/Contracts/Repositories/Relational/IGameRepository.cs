using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational.Generic;
using FiapCloudGames.Catalog.Domain.Entities;

namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;

public interface IGameRepository : IRepository<Game>
{
    Task<bool> GameAlreadyExistsByTitle(string name, string developer);
    Task<bool> GameAlreadyExistsByTitle(string name, string developer, Guid excludeId);
    Task<List<Game>> GetGame(string filter);
}