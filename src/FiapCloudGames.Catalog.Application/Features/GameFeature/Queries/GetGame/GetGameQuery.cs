using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Entities;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Queries.GetGame;

public sealed record class GetGameQuery
(
    string Filter
)
    : IRequest<IEnumerable<GetGameResponse>>;


/*
 
using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Queries.GetGame;

public class GetGameQueryHandler
    (
        IGameRepository gameRepository,
        IDistributedCache cache
    )
    : IRequestHandler<GetGameQuery, IEnumerable<GetGameResponse>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<IEnumerable<GetGameResponse>> Handle(GetGameQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"games:{query.Id}:{query.Title.ToUpper()}";

        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
            return JsonSerializer.Deserialize<IEnumerable<GetGameResponse>>(cached)!;

        var games = await gameRepository.GetGame(query.Id, query.Title);

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

        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration },
            cancellationToken);

        return result;
    }
}

 */
