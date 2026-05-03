using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using MediatR;
using System.Text.Json;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Queries.GetGame;

public class GetGameQueryHandler
    (
        IGameRepository gameRepository,
        IRedisRepository redisRepository
    )
    : IRequestHandler<GetGameQuery, IEnumerable<GetGameResponse>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<IEnumerable<GetGameResponse>> Handle(GetGameQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = "games:all";        
        if (!string.IsNullOrWhiteSpace(query.Filter))
            cacheKey = $"games:{query.Filter.ToLower()}";



        var cached = await redisRepository.GetAsync(cacheKey, cancellationToken);
        if (cached is not null)
            return JsonSerializer.Deserialize<IEnumerable<GetGameResponse>>(cached)!;

        var games = await gameRepository.GetGame(query.Filter);

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

        await redisRepository.SetAsync(cacheKey, JsonSerializer.Serialize(result), CacheDuration, cancellationToken);

        return result;
    }
}