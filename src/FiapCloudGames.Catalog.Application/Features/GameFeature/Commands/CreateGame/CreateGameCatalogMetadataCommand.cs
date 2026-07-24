using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Commands.CreateGame;

public class GameCatalogMetadataCommand
{
    public List<string> Platforms { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string AgeRating { get; set; } = string.Empty;
    public List<string> Languages { get; set; } = [];
    public List<string> Features { get; set; } = [];
}

public class GameCatalogRatingCommand
{
    public double Average { get; set; }
    public int Count { get; set; }
}