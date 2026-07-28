namespace FiapCloudGames.Catalog.Domain.ReadModels;

public class GameCatalogReadModel
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime ReleaseDate { get; init; }
    public string Developer { get; init; } = string.Empty;
    public decimal Price { get; init; }

    public List<GameCatalogCategoryReadModel> Categories { get; init; } = [];
    public GameCatalogMetadataReadModel Metadata { get; init; } = new();
    public GameCatalogRatingReadModel Rating { get; init; } = new();
}

public class GameCatalogCategoryReadModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public class GameCatalogMetadataReadModel
{
    public List<string> Platforms { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public string AgeRating { get; init; } = string.Empty;
    public List<string> Languages { get; init; } = [];
    public List<string> Features { get; init; } = [];
}

public class GameCatalogRatingReadModel
{
    public double Average { get; init; }
    public int Count { get; init; }
}