using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Application.GameFeature.Queries.GetGame;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using FiapCloudGames.Catalog.Domain.Entities;
using MediatR;

namespace FiapCloudGames.Catalog.Application.GameFeature.Queries.GetGame;

public class GetGameQueryHandler
    (
        IGameRepository gameRepository
    )
    : IRequestHandler<GetGameQuery, IEnumerable<GetGameResponse>>
{

    public async Task<IEnumerable<GetGameResponse>> Handle(GetGameQuery query, CancellationToken cancellationToken)
    {
        var games = await gameRepository.GetGame(query.Id, query.Title);

        return games.Select(x => new GetGameResponse
        {
            Title = x.Title,
            Categories = x.Categories.Select(x => x.Id).ToList(),
            Price = x.Price,
            Id = x.Id,
            Description = x.Description,
            Developer = x.Developer,
            ReleaseDate = x.ReleaseDate
        });
    }
}
