using FiapCloudGames.Catalog.Domain.Contracts.Publishers;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.LibraryItemFeature.Commands.CreateLibraryItem;

public class CreateLibraryItemCommandHandler(
    ILibraryItemRepository libraryItemRepository,
    IGameRepository gameRepository,
    Domain.Contracts.Repositories.MongoDb.ILibraryItemRepository mongoLibraryItemRepository,
    IEmailNotificationPublisher emailNotificationPublisher
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
        {
            await mongoLibraryItemRepository.InsertAsync(libraryItem, game.Title, game.Price);

            await emailNotificationPublisher.PublishAsync(
                to: command.Email,
                subject: "Novo item adicionado à sua biblioteca",
                body: $"Jogo {game.Title} adicionado à sua biblioteca.",
                cancellationToken: cancellationToken);
        }

        return result;
    }
}