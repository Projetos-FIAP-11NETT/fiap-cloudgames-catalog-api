using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FiapCloudGames.Catalog.Infrastructure.Data.Mongodb.Documents;

public class RequestLogDocument
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
