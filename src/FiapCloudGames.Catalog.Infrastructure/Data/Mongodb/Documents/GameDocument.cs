using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.ReadModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FiapCloudGames.Catalog.Infrastructure.Data.Mongodb.Documents;

public class GameDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string Developer { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public List<CategoryDocument> Categories { get; set; } = [];

    public DateTime UpdatedAt { get; set; }
    public GameMetadataDocument Metadata { get; set; } = new();
    public GameRatingDocument Rating { get; set; } = new();

    public static GameDocument FromEntity(
        Game game,
        GameCatalogMetadataReadModel metadata,
        GameCatalogRatingReadModel rating)
    {
        return new GameDocument
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            ReleaseDate = game.ReleaseDate,
            Developer = game.Developer,
            Price = game.Price,
            Categories = game.Categories.Select(c => new CategoryDocument
            {
                Id = c.Id,
                Name = c.Name
            }).ToList(),
            Metadata = new GameMetadataDocument
            {
                Platforms = metadata.Platforms,
                Tags = metadata.Tags,
                AgeRating = metadata.AgeRating,
                Languages = metadata.Languages,
                Features = metadata.Features
            },
            Rating = new GameRatingDocument
            {
                Average = rating.Average,
                Count = rating.Count
            }
        };
    }

    public GameCatalogReadModel ToReadModel()
    {
        return new GameCatalogReadModel
        {
            Id = Id,
            Title = Title,
            Description = Description,
            ReleaseDate = ReleaseDate,
            Developer = Developer,
            Price = Price,
            Categories = Categories.Select(c => new GameCatalogCategoryReadModel
            {
                Id = c.Id,
                Name = c.Name
            }).ToList(),
            Metadata = new GameCatalogMetadataReadModel
            {
                Platforms = Metadata.Platforms,
                Tags = Metadata.Tags,
                AgeRating = Metadata.AgeRating,
                Languages = Metadata.Languages,
                Features = Metadata.Features
            },
            Rating = new GameCatalogRatingReadModel
            {
                Average = Rating.Average,
                Count = Rating.Count
            }
        };
    }
}

public class CategoryDocument
{
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
public class GameMetadataDocument
{
    public List<string> Platforms { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string AgeRating { get; set; } = string.Empty;
    public List<string> Languages { get; set; } = [];
    public List<string> Features { get; set; } = [];
}

public class GameRatingDocument
{
    public double Average { get; set; }
    public int Count { get; set; }
}