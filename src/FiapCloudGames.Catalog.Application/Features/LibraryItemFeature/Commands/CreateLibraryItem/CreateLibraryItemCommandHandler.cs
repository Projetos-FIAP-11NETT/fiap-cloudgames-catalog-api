using FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.LibraryItemFeature.Commands.CreateLibraryItem;

public class CreateLibraryItemCommandHandler(
    Domain.Contracts.Repositories.Relational.ILibraryItemRepository libraryItemRepository,
    Domain.Contracts.Repositories.Relational.IGameRepository gameRepository,
    ILibraryItemRepository mongoLibraryItemRepository
)
    : IRequestHandler<CreateLibraryItemCommand, bool>
{
    public async Task<bool> Handle(CreateLibraryItemCommand command, CancellationToken cancellationToken)
    {
        var alreadyExists = await libraryItemRepository.ExistsAsync(command.UserId, command.GameId);
        if (alreadyExists)
            return false;

        var game = await gameRepository.GetByIdAsync(command.GameId);
        if (game is null)
            throw new BusinessException($"Jogo com Id '{command.GameId}' não encontrado.");

        var libraryItem = new LibraryItem(command.UserId, game, command.OrderId);

        await libraryItemRepository.AddAsync(libraryItem);
        var result = await libraryItemRepository.SaveChangesAsync(cancellationToken);

        if (result)
            await mongoLibraryItemRepository.InsertAsync(libraryItem, game.Title, game.Price);

        return result;
    }
}