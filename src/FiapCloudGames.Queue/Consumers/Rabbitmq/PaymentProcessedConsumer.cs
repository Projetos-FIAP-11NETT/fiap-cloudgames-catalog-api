using System.Transactions;
using FiapCloudGames.Catalog.Application.LibraryItemFeature.Commands.CreateLibraryItem;
using FiapCloudGames.Catalog.Application.OrderFeature.Commands.UpdateOrderStatus;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using FiapCloudGames.Catalog.Domain.Enums;
using FiapCloudGames.Queue.Contracts;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Queue.Consumers.Rabbitmq;

public class PaymentProcessedConsumer(ILogger<PaymentProcessedConsumer> logger, IMediator mediator, IOrderRepository orderRepository) : IConsumer<IPaymentProcessed>
{
    public async Task Consume(ConsumeContext<IPaymentProcessed> context)
    {
        try
        {
            logger.LogInformation("[catalog-service] PaymentProcessedConsumer - Received OrderId {orderId}",
                context.Message.OrderId);

            if (context.Message.PaymentStatus == PaymentStatus.Approved)
                await ProcessApprovedPayment(context.Message);
            else
                await ProcessRejectedPayment(context.Message);
        }
        catch (Exception ex)
        {
            logger.LogError("[catalog-service] PaymentProcessedConsumer - Error processing OrderId {orderId}", 
                context.Message.OrderId);
        }
    }

    private async Task ProcessApprovedPayment(IPaymentProcessed message)
    {
        logger.LogInformation("[catalog-service] PaymentProcessedConsumer - Processing Approved OrderId {orderId}",
            message.OrderId);
        var order = await orderRepository.GetOrderByIdAsync(message.OrderId);
        if (order == null)
        {
            logger.LogError("[catalog-service] PaymentProcessedConsumer - Order {orderId} not found.",
                message.OrderId);
            return;
        }
        
        var addGameCommand = new CreateLibraryItemCommand(order.UserId, order.GameId, order.Id);
        await mediator.Send(addGameCommand);

        var updateOrderCommand = new UpdateOrderStatusCommand(order.Id, OrderStatus.Aprovado);
        await mediator.Send(updateOrderCommand);

    }
    private async Task ProcessRejectedPayment(IPaymentProcessed message)
    {
        logger.LogInformation("[catalog-service] PaymentProcessedConsumer - Processing Reproved OrderId {orderId}",
            message.OrderId);
        var order = await orderRepository.GetOrderByIdAsync(message.OrderId);
        if (order == null)
        {
            logger.LogError("[catalog-service] PaymentProcessedConsumer - Order {orderId} not found.",
                message.OrderId);
            return;
        }

        var updateOrderCommand = new UpdateOrderStatusCommand(order.Id, OrderStatus.Rejeitado);
        await mediator.Send(updateOrderCommand);
    }
}