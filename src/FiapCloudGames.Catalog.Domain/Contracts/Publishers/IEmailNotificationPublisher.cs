namespace FiapCloudGames.Catalog.Domain.Contracts.Publishers;

public interface IEmailNotificationPublisher
{
    Task PublishAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}