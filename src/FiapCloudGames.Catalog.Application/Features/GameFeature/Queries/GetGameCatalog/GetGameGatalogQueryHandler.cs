using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;
using MediatR;
using System.Text.Json;

namespace FiapCloudGames.Catalog.Application.Features.GameFeatureResponse.Queries.GetGame;

public class GetGameCatalogQueryHandler(
    IGameCatalogRepository gameCatalogRepository
)
    : IRequestHandler<GetGameCatalogQuery, List<GetGameCatalogResponse>>
{
    public async Task<List<GetGameCatalogResponse>> Handle(
    GetGameCatalogQuery query,
    CancellationToken cancellationToken)
    {
        var games = await gameCatalogRepository.GetCatalogAsync(
            query.Filter,
            query.Category,
            query.Developer,
            query.MinPrice,
            query.MaxPrice,
            cancellationToken);

        return games.Select(g => new GetGameCatalogResponse
        {
            Id = g.Id,
            Title = g.Title,
            Description = g.Description,
            ReleaseDate = g.ReleaseDate,
            Developer = g.Developer,
            Price = g.Price,
            Categories = g.Categories.Select(c => new GetGameCatalogCategoryResponse
            {
                Id = c.Id,
                Name = c.Name
            }).ToList(),
            Metadata = new GameCatalogMetadataResponse
            {
                Platforms = g.Metadata.Platforms,
                Tags = g.Metadata.Tags,
                AgeRating = g.Metadata.AgeRating,
                Languages = g.Metadata.Languages,
                Features = g.Metadata.Features
            },
            Rating = new GameCatalogRatingResponse
            {
                Average = g.Rating.Average,
                Count = g.Rating.Count
            }
        }).ToList();
    }
}