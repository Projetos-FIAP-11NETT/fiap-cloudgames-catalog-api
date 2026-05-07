using FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.ReadModels;
using FiapCloudGames.Catalog.Infrastructure.Data.Mongodb;
using FiapCloudGames.Catalog.Infrastructure.Data.Mongodb.Documents;
using MongoDB.Driver;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.MongoDb;

public class LibraryItemRepository(MongoDbContext context) : ILibraryItemRepository
{
    private readonly IMongoCollection<LibraryItemDocument> _collection =
        context.GetCollection<LibraryItemDocument>("library_items");

    public async Task InsertAsync(LibraryItem item, string gameTitle, decimal gamePrice)
    {
        var game = new GameInLibraryDocument
        {
            GameId = item.GameId,
            GameTitle = gameTitle,
            GamePrice = gamePrice,
            OrderId = item.OrderId,
            AddedAt = item.AddedAt
        };

        var filter = Builders<LibraryItemDocument>.Filter
            .Eq(d => d.UserId, item.UserId);

        var update = Builders<LibraryItemDocument>.Update
            .Push(d => d.Games, game);

        await _collection.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = true });
    }

    public async Task<LibraryItemReadModel?> GetByUserIdAsync(Guid userId)
    {
        var doc = await _collection
            .Find(d => d.UserId == userId)
            .FirstOrDefaultAsync();

        return doc?.ToReadModel();
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid gameId) =>
        await _collection
            .Find(d => d.UserId == userId && d.Games.Any(g => g.GameId == gameId))
            .AnyAsync();

    public async Task DeleteAsync(Guid userId) =>
        await _collection.DeleteOneAsync(d => d.UserId == userId);
}