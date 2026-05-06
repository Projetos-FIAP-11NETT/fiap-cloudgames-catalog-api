using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.Redis;

public class RedisRepository
    (
        IDistributedCache cache,
        IGameRepository gameRepository
    )
    : IRedisRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => cache.RemoveAsync(key, cancellationToken);

    public async Task<string> GetGameAsync(string key, string filter, CancellationToken cancellationToken = default) 
    {
        var cached = await cache.GetStringAsync(key, cancellationToken);
        if (cached is not null)
            return cached;

        var games = await gameRepository.GetGame(filter);
        
        var result = games.Select(x => new GetGameResponse
        {
            Title = x.Title,
            Categories = [.. x.Categories.Select(x => x.Id)],
            Price = x.Price,
            Id = x.Id,
            Description = x.Description,
            Developer = x.Developer,
            ReleaseDate = x.ReleaseDate
        }).ToList();

        var value = JsonSerializer.Serialize(result);
        await SetAsync(key, value, CacheDuration, cancellationToken);
        return value;
    }

    private Task SetAsync(string key, string value, TimeSpan expiration, CancellationToken cancellationToken = default)
        => cache.SetStringAsync(
            key,
            value,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration },
            cancellationToken);
}