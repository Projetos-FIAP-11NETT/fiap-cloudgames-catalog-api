using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Elasticsearch;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Queue.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Queue.Consumers.Sqs;

public class GameIndexConsumer(
    ILogger<GameIndexConsumer> logger,
    IGameRepository gameRepository,
    IGameSearchRepository gameSearchRepository
) : IConsumer<IGameUpserted>, IConsumer<IGameDeleted>
{
    public async Task Consume(ConsumeContext<IGameUpserted> context)
    {
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        try
        {
            logger.LogInformation("[catalog-service] CorrelationId {correlationId} - GameIndexConsumer - Indexing GameId {gameId}",
                correlationId, context.Message.Id);

            var game = await gameRepository.GetByIdWithCategoriesAsync(context.Message.Id);
            if (game == null)
            {
                logger.LogError("[catalog-service] CorrelationId {correlationId} - GameIndexConsumer - Game {gameId} not found.",
                    correlationId, context.Message.Id);
                return;
            }

            await gameSearchRepository.IndexAsync(game, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[catalog-service] CorrelationId {correlationId} - GameIndexConsumer - Error indexing GameId {gameId}",
                correlationId, context.Message.Id);
        }
    }

    public async Task Consume(ConsumeContext<IGameDeleted> context)
    {
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        try
        {
            logger.LogInformation("[catalog-service] CorrelationId {correlationId} - GameIndexConsumer - Removing GameId {gameId}",
                correlationId, context.Message.GameId);

            await gameSearchRepository.DeleteAsync(context.Message.GameId, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[catalog-service] CorrelationId {correlationId} - GameIndexConsumer - Error removing GameId {gameId}",
                correlationId, context.Message.GameId);
        }
    }
}
