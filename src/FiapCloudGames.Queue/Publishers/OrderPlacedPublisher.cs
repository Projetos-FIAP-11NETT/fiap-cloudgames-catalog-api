using FiapCloudGames.Queue.Configurations.Rabbitmq;
using FiapCloudGames.Queue.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Queue.Publishers;

public class OrderPlacedPublisher(IRabbitmqPublish bus, ILogger<OrderPlacedPublisher> logger) : IOrderPlacedPublisher
{
    private readonly IPublishEndpoint _publishEndpoint = bus;
    private readonly ILogger<OrderPlacedPublisher> _logger = logger;
    
    public Task PublishAsync(int orderId, Guid userId, Guid gameId, decimal price, string email, string name,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Publishing IOrderPlaced to RabbitMQ: OrderId={OrderId}, User={UserId}, Game={GameId}",
            orderId, userId, gameId);

        return _publishEndpoint.Publish<IOrderPlaced>(new
        {
            OrderId = orderId,
            UserId =  userId,
            GameId = gameId,
            Price = price,
            Email = email,
            Name = name
        }, cancellationToken);
    }
}