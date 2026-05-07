using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Commands.UpdateGame;

public class UpdateGameCommandHandler
    (
        IGameRepository gameRepository,
        ICategoryRepository categoryRepository,
        IRedisRepository redisRepository
    )
    : IRequestHandler<UpdateGameCommand, bool>
{
    public async Task<bool> Handle(UpdateGameCommand command, CancellationToken cancellationToken)
    {
        var existingGame = await gameRepository.GetByIdWithCategoriesAsync(command.Id);

        if (existingGame == null)
            throw new BusinessException("Jogo não encontrado.");

        var duplicateExists = await gameRepository.GameAlreadyExistsByTitle(command.Title, command.Developer, command.Id);
        if (duplicateExists)
            throw new BusinessException("Já existe um jogo com esse título e desenvolvedor cadastrado.");

        var categories = await categoryRepository.GetByIdsAsync(command.Categories);
        if (!categories.Any())
            throw new BusinessException("É obrigatório definir pelo menos uma categoria para o jogo.");

        existingGame.Update(
            command.Title,
            command.Description,
            command.ReleaseDate,
            command.Developer,
            command.Price,
            categories);

        gameRepository.Update(existingGame);

        var result = await gameRepository.SaveChangesAsync(cancellationToken);

        if (result)
            await redisRepository.RemoveKeysThatContainGameAsync(existingGame.Id, cancellationToken);

        return result;
    }
}