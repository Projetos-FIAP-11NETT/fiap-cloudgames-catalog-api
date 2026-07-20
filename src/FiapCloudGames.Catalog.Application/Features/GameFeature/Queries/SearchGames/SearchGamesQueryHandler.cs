using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Elasticsearch;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Queries.SearchGames;

public class SearchGamesQueryHandler
    (
        IGameSearchRepository gameSearchRepository
    )
    : IRequestHandler<SearchGamesQuery, IEnumerable<GetGameResponse>>
{
    public async Task<IEnumerable<GetGameResponse>> Handle(SearchGamesQuery query, CancellationToken cancellationToken)
    {
        var results = await gameSearchRepository.SearchAsync(query.Term, query.Page, query.Size, cancellationToken);

        // Preserva a ordem de relevância (score) retornada pelo Elasticsearch.
        return results
            .Select(r => new GetGameResponse
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                ReleaseDate = r.ReleaseDate,
                Developer = r.Developer,
                Price = r.Price,
                Categories = [.. r.Categories]
            })
            .ToList();
    }
}
