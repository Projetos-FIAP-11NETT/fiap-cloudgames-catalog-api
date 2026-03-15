using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using MediatR;

namespace FiapCloudGames.Catalog.Application.OrderFeature.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler (IOrderRepository orderRepository) : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetOrderByIdAsync(request.OrderId);
        if (order is null)
            return false;
        
        order.UpdateStatus(request.Status);
        orderRepository.Update(order);
        await orderRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}