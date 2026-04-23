using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Infrastructure.Data.NoSql.Mongodb;
using FiapCloudGames.Catalog.Infrastructure.Data.NoSql.Mongodb.Documents;
using MongoDB.Driver;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.NoSql.MongoDb;

public class MongoLibraryItemRepository(MongoDbContext context)
{
    private readonly IMongoCollection<LibraryItemDocument> _collection =
        context.GetCollection<LibraryItemDocument>("library_items");

    public async Task InsertAsync(LibraryItem item, string gameTitle, decimal gamePrice)
    {
        var doc = new LibraryItemDocument
        {
            Id = item.Id,
            UserId = item.UserId,
            GameId = item.GameId,
            GameTitle = gameTitle,
            GamePrice = gamePrice,
            OrderId = item.OrderId,
            AddedAt = item.AddedAt
        };

        await _collection.InsertOneAsync(doc);
    }

    public async Task<List<LibraryItemDocument>> GetByUserIdAsync(Guid userId) =>
        await _collection.Find(i => i.UserId == userId).ToListAsync();

    public async Task<bool> ExistsAsync(Guid userId, Guid gameId) =>
        await _collection.Find(i => i.UserId == userId && i.GameId == gameId)
            .AnyAsync();

    public async Task DeleteAsync(Guid id) =>
        await _collection.DeleteOneAsync(i => i.Id == id);
}