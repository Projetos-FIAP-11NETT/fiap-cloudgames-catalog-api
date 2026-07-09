using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Commands.DeleteGame;

public class DeleteGameCommandHandler
    (
        IGameRepository gameRepository,
        IRedisRepository redisRepository
    )
    : IRequestHandler<DeleteGameCommand, bool>
{
    public async Task<bool> Handle(DeleteGameCommand command, CancellationToken cancellationToken)
    {
        var existingGame = 
            await gameRepository.GetByIdAsync(command.Id)
                ?? throw new NotFoundException("Jogo não encontrado.");

        gameRepository.Remove(existingGame);

        var result = await gameRepository.SaveChangesAsync(cancellationToken);

        if (result)
            await redisRepository.RemoveKeysThatContainGameAsync(existingGame.Id, cancellationToken);

        return result;
    }
}