using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Entities;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.GameFeatureResponse.Queries.GetGame;

public class GetGameCatalogQuery : IRequest<List<GetGameCatalogResponse>>
{
    public string? Filter { get; set; }
    public string? Category { get; set; }
    public string? Developer { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}