using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;
using StackExchange.Redis;
using System.Text.Json;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.Redis;

public class RedisRepository
    (
        IGameRepository gameRepository,
        IConnectionMultiplexer connectionMultiplexer
    )
    : IRedisRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task RemoveKeysThatContainGameAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var db = connectionMultiplexer.GetDatabase();

        // Busca diretamente o índice do jogo
        var indexKey = $"games:index:{gameId}";
        var filterKeys = await db.SetMembersAsync(indexKey);

        if (filterKeys.Length == 0)
            return;

        var keysToDelete = filterKeys
            .Select(k => (RedisKey)k.ToString())
            .Append(indexKey)
            .ToArray();

        // Remove todas as keys + o índice em um único round-trip
        await db.KeyDeleteAsync(keysToDelete);
    }

    public async Task<string> GetGameAsync(string key, string filter, CancellationToken cancellationToken = default)
    {
        var db = connectionMultiplexer.GetDatabase();

        var cached = await db.StringGetAsync(key);
        if (cached.HasValue)
            return cached.ToString();

        var games = await gameRepository.GetGame(filter);

        var result = games.Select(x => new GetGameResponse
        {
            Id = x.Id,
            Title = x.Title,
            Categories = [.. x.Categories.Select(c => c.Id)],
            Price = x.Price,
            Description = x.Description,
            Developer = x.Developer,
            ReleaseDate = x.ReleaseDate
        }).ToList();

        var value = JsonSerializer.Serialize(result);

        var batch = db.CreateBatch();

        // Salva o cache do filtro
        var setTask = batch.StringSetAsync(key, value, CacheDuration);

        // Para cada jogo no resultado, registra que a key pertence a ele
        var indexTasks = result.Select(g =>
            batch.SetAddAsync($"games:index:{g.Id}", key)).ToList();

        batch.Execute();

        await setTask;
        await Task.WhenAll(indexTasks);

        return value;
    }
}