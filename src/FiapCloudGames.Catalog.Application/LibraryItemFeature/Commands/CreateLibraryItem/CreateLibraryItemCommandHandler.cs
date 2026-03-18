using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.LibraryItemFeature.Commands.CreateLibraryItem;

public class CreateLibraryItemCommandHandler(
    ILibraryItemRepository libraryItemRepository,
    IGameRepository gameRepository
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
        await libraryItemRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}