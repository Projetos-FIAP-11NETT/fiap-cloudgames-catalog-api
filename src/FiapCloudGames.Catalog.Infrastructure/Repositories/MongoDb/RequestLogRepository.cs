using FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Infrastructure.Data.Mongodb;
using FiapCloudGames.Catalog.Infrastructure.Data.Mongodb.Documents;
using MongoDB.Driver;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.MongoDb;

public class RequestLogRepository(MongoDbContext context) : IRequestLogRepository
{
    private readonly IMongoCollection<RequestLogDocument> _collection =
        context.GetCollection<RequestLogDocument>("request_logs");

    public Task InsertAsync(RequestLog log, CancellationToken cancellationToken = default)
    {
        var document = new RequestLogDocument
        {
            CorrelationId = log.CorrelationId,
            Method = log.Method,
            Path = log.Path,
            StatusCode = log.StatusCode,
            ElapsedMilliseconds = log.ElapsedMilliseconds,
            UserId = log.UserId,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            CreatedAt = log.CreatedAt
        };

        return _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }
}