using FiapCloudGames.Catalog.Domain.Contracts.Repositories.NoSql;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Infrastructure.Data.NoSql.Mongodb;
using FiapCloudGames.Catalog.Infrastructure.Data.NoSql.Mongodb.Documents;
using MongoDB.Driver;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.NoSql.MongoDb;

public class MongoGameRepository(
    MongoDbContext context
) 
    : IMongoGameRepository

{
    private readonly IMongoCollection<GameDocument> _collection =
        context.GetCollection<GameDocument>("games");

    public async Task UpsertAsync(Game game)
    {
        var doc = new GameDocument
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            ReleaseDate = game.ReleaseDate,
            Developer = game.Developer,
            Price = game.Price,
            Categories = [.. game.Categories.Select(c =>
                new CategoryDocument { Id = c.Id, Name = c.Name })]
        };

        await _collection.ReplaceOneAsync(
            filter: d => d.Id == game.Id,
            replacement: doc,
            options: new ReplaceOptions { IsUpsert = true });
    }

    public async Task<List<GameDocument>> GetAllAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<GameDocument?> GetByIdAsync(Guid id) =>
        await _collection.Find(g => g.Id == id).FirstOrDefaultAsync();

    public async Task<List<GameDocument>> SearchAsync(
        string? title = null,
        string? developer = null,
        Guid? categoryId = null)
    {
        var filters = new List<FilterDefinition<GameDocument>>();

        if (!string.IsNullOrWhiteSpace(title))
            filters.Add(Builders<GameDocument>.Filter
                .Regex(g => g.Title, new MongoDB.Bson.BsonRegularExpression(title, "i")));

        if (!string.IsNullOrWhiteSpace(developer))
            filters.Add(Builders<GameDocument>.Filter
                .Regex(g => g.Developer, new MongoDB.Bson.BsonRegularExpression(developer, "i")));

        if (categoryId.HasValue)
            filters.Add(Builders<GameDocument>.Filter
                .ElemMatch(g => g.Categories, c => c.Id == categoryId.Value));

        var combinedFilter = filters.Count > 0
            ? Builders<GameDocument>.Filter.And(filters)
            : Builders<GameDocument>.Filter.Empty;

        return await _collection.Find(combinedFilter).ToListAsync();
    }

    public async Task DeleteAsync(Guid id) =>
        await _collection.DeleteOneAsync(g => g.Id == id);
}