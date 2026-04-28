using FiapCloudGames.Catalog.Domain.ReadModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FiapCloudGames.Catalog.Infrastructure.Data.NoSql.Mongodb.Documents;

public class LibraryItemDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    public List<GameInLibraryDocument> Games { get; set; } = [];

    public LibraryItemReadModel ToReadModel() => new()
    {
        UserId = UserId,
        Games = Games.Select(g => new GameInLibraryReadModel
        {
            GameId = g.GameId,
            GameTitle = g.GameTitle,
            GamePrice = g.GamePrice,
            OrderId = g.OrderId,
            AddedAt = g.AddedAt
        }).ToList()
    };
}

public class GameInLibraryDocument
{
    [BsonRepresentation(BsonType.String)]
    public Guid GameId { get; set; }

    public string GameTitle { get; set; } = string.Empty;
    public decimal GamePrice { get; set; }
    public int? OrderId { get; set; }
    public DateTime AddedAt { get; set; }
}