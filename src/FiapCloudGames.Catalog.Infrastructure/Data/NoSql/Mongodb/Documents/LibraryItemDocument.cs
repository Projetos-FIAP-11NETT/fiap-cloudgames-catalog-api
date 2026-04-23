using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FiapCloudGames.Catalog.Infrastructure.Data.NoSql.Mongodb.Documents;

public class LibraryItemDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid GameId { get; set; }

    public string GameTitle { get; set; } = string.Empty;
    public decimal GamePrice { get; set; }
    public int? OrderId { get; set; }
    public DateTime AddedAt { get; set; }
}