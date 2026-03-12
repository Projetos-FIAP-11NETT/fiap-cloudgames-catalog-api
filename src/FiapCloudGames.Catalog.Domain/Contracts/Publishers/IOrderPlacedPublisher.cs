namespace FiapCloudGames.Catalog.Domain.Contracts.Publishers;

public interface IOrderPlacedPublisher
{
    Task PublishAsync(
        int orderId,
        string userId,
        Guid gameId,
        decimal price,
        string email,
        string name,
        CancellationToken cancellationToken = default);
}