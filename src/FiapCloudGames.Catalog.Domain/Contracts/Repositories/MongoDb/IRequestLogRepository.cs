using FiapCloudGames.Catalog.Domain.Entities;

namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;

public interface IRequestLogRepository
{
    Task InsertAsync(RequestLog log, CancellationToken cancellationToken = default);
}
