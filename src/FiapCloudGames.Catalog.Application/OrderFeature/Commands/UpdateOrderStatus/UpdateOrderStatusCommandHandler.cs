using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using FiapCloudGames.Catalog.Domain.Enums;
using MediatR;

namespace FiapCloudGames.Catalog.Application.OrderFeature.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetOrderByIdAsync(request.OrderId);
        if (order is null)
            return false;

        switch (request.Status)
        {
            case OrderStatus.Aprovado:
                order.Aprovar();
                break;
            case OrderStatus.Rejeitado:
                order.Rejeitar();
                break;
            case OrderStatus.Cancelado:
                order.Cancelar();
                break;
            default:
                return false;
        }

        orderRepository.Update(order);
        await orderRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}