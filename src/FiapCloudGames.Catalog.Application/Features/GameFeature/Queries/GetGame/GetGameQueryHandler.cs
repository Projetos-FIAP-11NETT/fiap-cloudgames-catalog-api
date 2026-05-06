using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;
using MediatR;
using System.Text.Json;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Queries.GetGame;

public class GetGameQueryHandler
    (
        IRedisRepository redisRepository
    )
    : IRequestHandler<GetGameQuery, IEnumerable<GetGameResponse>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<IEnumerable<GetGameResponse>> Handle(GetGameQuery query, CancellationToken cancellationToken)
    {
        var key = $"games:";        
        if (!string.IsNullOrWhiteSpace(query.Filter))
            key = $"games:{query.Filter.ToLower()}";

        var cached = await redisRepository.GetGameAsync(key, query.Filter, cancellationToken);
        return JsonSerializer.Deserialize<IEnumerable<GetGameResponse>>(cached)!;
    }
}