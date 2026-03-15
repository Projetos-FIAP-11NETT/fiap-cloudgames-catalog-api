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
            throw new BusinessException($"Jogo com Id '{command.GameId}' não encontrado.");

        var order = new Order(command.UserId, game);

        var orderId = await orderRepository.AddOrderAsync(order);

        await orderPlacedPublisher.PublishAsync(
            orderId,
            command.UserId,
            game.Id,
            game.Price,
            command.Email,
            command.Name,
            cancellationToken);
        

        return true;
    }
}