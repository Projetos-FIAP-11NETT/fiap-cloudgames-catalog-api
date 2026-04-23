using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FiapCloudGames.Catalog.Infrastructure.Data.NoSql.Mongodb;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);

        //EnsureIndexes();
    }

    public IMongoCollection<T> GetCollection<T>(string name) =>
        _database.GetCollection<T>(name);

    //private void EnsureIndexes()
    //{
    //    var games = GetCollection<Documents.GameDocument>("games");

    //    var indexModel = new CreateIndexModel<Documents.GameDocument>(
    //        Builders<Documents.GameDocument>.IndexKeys
    //            .Ascending("Categories.Id"),
    //        new CreateIndexOptions { Name = "IX_Games_Categories_Id" });

    //    games.Indexes.CreateOne(indexModel);
    //}
}