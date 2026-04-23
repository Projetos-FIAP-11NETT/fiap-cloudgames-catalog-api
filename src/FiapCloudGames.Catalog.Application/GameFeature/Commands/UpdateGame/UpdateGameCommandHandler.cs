using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.GameFeature.Commands.UpdateGame;

public class UpdateGameCommandHandler
    (
        IGameRepository gameRepository,
        ICategoryRepository categoryRepository
    )
    : IRequestHandler<UpdateGameCommand, bool>
{

    public async Task<bool> Handle(UpdateGameCommand command, CancellationToken cancellationToken)
    {
        var existingGame = await gameRepository.GetByIdAsync(command.Id);

        if (existingGame == null)
            throw new BusinessException("Jogo não encontrado.");

        var duplicateExists = await gameRepository.GameAlreadyExistsByTitle(command.Title, command.Developer, command.Id);
        if (duplicateExists)
            throw new BusinessException("Já existe um jogo com esse título e desenvolvedor cadastrado.");

        var categories = await categoryRepository.GetByIdsAsync(command.Categories);
        if (!categories.Any())
            throw new BusinessException("É obrigatório definir pelo menos uma categoria para o jogo.");

        existingGame.GetType()
            .GetProperty("Title")?
            .SetValue(existingGame, command.Title);

        existingGame.GetType()
            .GetProperty("Description")?
            .SetValue(existingGame, command.Description);

        existingGame.GetType()
            .GetProperty("ReleaseDate")?
            .SetValue(existingGame, command.ReleaseDate);

        existingGame.GetType()
            .GetProperty("Developer")?
            .SetValue(existingGame, command.Developer);

        existingGame.GetType()
            .GetProperty("Price")?
            .SetValue(existingGame, command.Price);

        gameRepository.Update(existingGame);

        return await gameRepository.SaveChangesAsync(cancellationToken);
    }


}
