using FiapCloudGames.Catalog.Domain.Contracts.Publishers;
using FiapCloudGames.Catalog.Shared.Abstractions;
using FiapCloudGames.Queue.Configurations.Sqs;
using FiapCloudGames.Queue.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Queue.Publishers;

public class OrderPlacedPublisher(ISqsPublish bus, ILogger<OrderPlacedPublisher> logger, ICorrelationIdAccessor correlation) : IOrderPlacedPublisher
{
    private readonly IPublishEndpoint _publishEndpoint = bus;

    public Task PublishAsync(int orderId, Guid userId, Guid gameId, decimal price, string email, string name,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[catalog-service] Publishing IOrderPlaced to SQS: OrderId={OrderId}, User={UserId}, Game={GameId}",
            orderId, userId, gameId);

        return _publishEndpoint.Publish<IOrderPlaced>(new
        {
            OrderId = orderId,
            UserId = userId,
            GameId = gameId,
            Price = price,
            Email = email,
            Name = name
        }, context =>
        {
            context.CorrelationId = correlation.CorrelationId;
        }, 
            cancellationToken);
    }
}