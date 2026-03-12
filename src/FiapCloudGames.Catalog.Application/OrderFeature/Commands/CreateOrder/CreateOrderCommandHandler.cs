using FiapCloudGames.Catalog.Domain.Contracts.Publishers;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.OrderFeature.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IGameRepository gameRepository,
    IOrderPlacedPublisher orderPlacedPublisher
)
    : IRequestHandler<CreateOrderCommand, bool>
{
    public async Task<bool> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsync(command.GameId);
        if (game is null)
            throw new NotFoundException($"Jogo com Id '{command.GameId}' não encontrado.");

        var order = new Order(command.UserId, game);

        await orderRepository.AddAsync(order);
        var saved = await orderRepository.SaveChangesAsync(cancellationToken);

        if (saved)
        {
            await orderPlacedPublisher.PublishAsync(
                order.Id.GetHashCode(),
                command.UserId,
                game.Id,
                game.Price,
                command.Email,
                command.Name,
                cancellationToken);
        }

        return saved;
    }
}