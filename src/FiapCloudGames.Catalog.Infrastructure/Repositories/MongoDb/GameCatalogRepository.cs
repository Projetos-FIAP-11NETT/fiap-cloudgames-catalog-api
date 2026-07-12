using MongoDB.Driver;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.MongoDb;

using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.ReadModels;
using FiapCloudGames.Catalog.Infrastructure.Data.Mongodb;
using FiapCloudGames.Catalog.Infrastructure.Data.Mongodb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

public class GameCatalogRepository(MongoDbContext context) : IGameCatalogRepository
{
    private readonly IMongoCollection<GameDocument> _collection =
        context.GetCollection<GameDocument>("game_catalog");

    public async Task UpsertAsync(
        Game game,
        GameCatalogMetadataReadModel metadata,
        GameCatalogRatingReadModel rating,
        CancellationToken cancellationToken)
    {
        var document = GameDocument.FromEntity(game, metadata, rating);

        await _collection.ReplaceOneAsync(
            g => g.Id == document.Id,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }

    public async Task DeleteAsync(Guid gameId, CancellationToken cancellationToken)
    {
        await _collection.DeleteOneAsync(
            g => g.Id == gameId,
            cancellationToken);
    }

    public async Task<List<GameCatalogReadModel>> GetCatalogAsync(
        string? filter,
        string? category,
        string? developer,
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken cancellationToken)
    {
        var builder = Builders<GameDocument>.Filter;
        var mongoFilter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var regex = new BsonRegularExpression(filter, "i");

            mongoFilter &= builder.Or(
                builder.Regex(g => g.Title, regex),
                builder.Regex(g => g.Description, regex),
                builder.Regex(g => g.Developer, regex));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            mongoFilter &= builder.ElemMatch(
                g => g.Categories,
                c => c.Name == category);
        }

        if (!string.IsNullOrWhiteSpace(developer))
        {
            mongoFilter &= builder.Regex(
                g => g.Developer,
                new BsonRegularExpression(developer, "i"));
        }

        if (minPrice.HasValue)
            mongoFilter &= builder.Gte(g => g.Price, minPrice.Value);

        if (maxPrice.HasValue)
            mongoFilter &= builder.Lte(g => g.Price, maxPrice.Value);

        var documents = await _collection
            .Find(mongoFilter)
            .SortBy(g => g.Title)
            .ToListAsync(cancellationToken);

        return documents.Select(d => d.ToReadModel()).ToList();
    }
}