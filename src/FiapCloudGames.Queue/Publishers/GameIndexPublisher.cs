using FiapCloudGames.Catalog.Domain.Contracts.Publishers;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Shared.Abstractions;
using FiapCloudGames.Queue.Configurations.Sqs;
using FiapCloudGames.Queue.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Queue.Publishers;

public class GameIndexPublisher(
    ISqsPublish bus,
    ILogger<GameIndexPublisher> logger,
    ICorrelationIdAccessor correlation) : IGameIndexPublisher
{
    private readonly IPublishEndpoint _publishEndpoint = bus;

    public async Task PublishUpsertedAsync(Game game, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[catalog-service] Publishing IGameUpserted to SQS: GameId={GameId}, Title={Title}",
            game.Id, game.Title);

        try
        {
            await _publishEndpoint.Publish<IGameUpserted>(new
            {
                game.Id,
                game.Title,
                game.Description,
                game.ReleaseDate,
                game.Developer,
                game.Price,
                Categories = game.Categories.Select(c => c.Id).ToList()
            }, context =>
            {
                context.CorrelationId = correlation.CorrelationId;
            },
            cancellationToken);

            logger.LogInformation(
                "[catalog-service] Successfully published IGameUpserted to SQS: GameId={GameId}", game.Id);
        }
        catch (Exception)
        {
            logger.LogError("[catalog-service] Failed to publish IGameUpserted to SQS: GameId={GameId}", game.Id);
            throw;
        }
    }

    public async Task PublishDeletedAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[catalog-service] Publishing IGameDeleted to SQS: GameId={GameId}", gameId);

        try
        {
            await _publishEndpoint.Publish<IGameDeleted>(new
            {
                GameId = gameId
            }, context =>
            {
                context.CorrelationId = correlation.CorrelationId;
            },
            cancellationToken);

            logger.LogInformation(
                "[catalog-service] Successfully published IGameDeleted to SQS: GameId={GameId}", gameId);
        }
        catch (Exception)
        {
            logger.LogError("[catalog-service] Failed to publish IGameDeleted to SQS: GameId={GameId}", gameId);
            throw;
        }
    }
}
