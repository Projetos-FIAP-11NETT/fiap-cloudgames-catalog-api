namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;

public interface IRedisRepository
{
    Task<string> GetGameAsync(string key, string filter, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}