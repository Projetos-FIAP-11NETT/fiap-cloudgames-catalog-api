using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;
using Microsoft.Extensions.Caching.Distributed;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.Redis;

public class RedisRepository(IDistributedCache cache) : IRedisRepository
{
    public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
        => cache.GetStringAsync(key, cancellationToken);

    public Task SetAsync(string key, string value, TimeSpan expiration, CancellationToken cancellationToken = default)
        => cache.SetStringAsync(
            key,
            value,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration },
            cancellationToken);

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => cache.RemoveAsync(key, cancellationToken);
}