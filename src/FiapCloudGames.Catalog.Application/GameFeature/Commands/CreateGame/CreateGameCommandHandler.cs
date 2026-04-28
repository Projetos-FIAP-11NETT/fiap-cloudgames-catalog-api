using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.GameFeature.Commands.CreateGame;

public class CreateGameCommandHandler(
    IGameRepository gameRepository,
    ICategoryRepository categoryRepository
) 
    : IRequestHandler<CreateGameCommand, bool>

{
    public async Task<bool> Handle(CreateGameCommand command, CancellationToken cancellationToken)
    {
        var exists = await gameRepository.GameAlreadyExistsByTitle(command.Title, command.Developer);
        if (exists)
            throw new BusinessException("Já existe um jogo com esse título e desenvolvedor cadastrado.");

        var categories = await categoryRepository.GetByIdsAsync(command.Categories);
        if (!categories.Any())
            throw new BusinessException("É obrigatório definir pelo menos uma categoria para o jogo.");

        var game = new Game(command.Title, command.Description, command.ReleaseDate,
            command.Developer, command.Price, categories);

        await gameRepository.AddAsync(game);
        return await gameRepository.SaveChangesAsync(cancellationToken);
    }
}