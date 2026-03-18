using FiapCloudGames.Catalog.Domain.Contracts.Publishers;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Enums;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.OrderFeature.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IGameRepository gameRepository,
    IOrderPlacedPublisher orderPlacedPublisher,
    ILibraryItemRepository libraryItemRepository
)
    : IRequestHandler<CreateOrderCommand, bool>
{
    public async Task<bool> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsync(command.GameId);
        if (game is null)
            throw new BusinessException($"Jogo com Id '{command.GameId}' não encontrado.");
        
        var allUserGames = await libraryItemRepository.GetByUserIdAsync(command.UserId);
        if (allUserGames.Any(x => x.GameId == game.Id))
            throw new BusinessException("Usuário já possui esse jogo");
        var allUserOrders = await orderRepository.GetOrdersByUserIdAsync(command.UserId);
        if (allUserOrders.Any(x => x.GameId == game.Id && x.Status == OrderStatus.Pendente))
            throw new BusinessException("Usuário já possui um pedido pendente para esse jogo");
        
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