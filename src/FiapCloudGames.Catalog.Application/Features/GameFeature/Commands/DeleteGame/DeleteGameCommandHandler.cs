using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Commands.DeleteGame;

public class DeleteGameCommandHandler
    (
        IGameRepository gameRepository
    )
    : IRequestHandler<DeleteGameCommand, bool>
{

    public async Task<bool> Handle(DeleteGameCommand command, CancellationToken cancellationToken)
    {
        var existingGame = await gameRepository.GetByIdAsync(command.Id);

        if (existingGame == null)
            throw new NotFoundException("Jogo não encontrado.");

        gameRepository.Remove(existingGame);

        return await gameRepository.SaveChangesAsync(cancellationToken);
    }

}
