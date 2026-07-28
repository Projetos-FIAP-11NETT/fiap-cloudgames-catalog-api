namespace FiapCloudGames.Catalog.Application.DTOs;

public class GetGameCatalogResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string Developer { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<GetGameCatalogCategoryResponse> Categories { get; set; } = [];
    public GameCatalogMetadataResponse Metadata { get; set; } = new();
    public GameCatalogRatingResponse Rating { get; set; } = new();
}

public class GetGameCatalogCategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class GameCatalogMetadataResponse
{
    public List<string> Platforms { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string AgeRating { get; set; } = string.Empty;
    public List<string> Languages { get; set; } = [];
    public List<string> Features { get; set; } = [];
}

public class GameCatalogRatingResponse
{
    public double Average { get; set; }
    public int Count { get; set; }
}