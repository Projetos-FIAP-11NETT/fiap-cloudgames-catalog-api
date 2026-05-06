using FiapCloudGames.Catalog.Application.Features.LibraryItemFeature.Commands.CreateLibraryItem;
using FiapCloudGames.Catalog.Application.Features.OrderFeature.Commands.UpdateOrderStatus;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Enums;
using FiapCloudGames.Queue.Contracts;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Queue.Consumers.Sqs;

public class PaymentProcessedConsumer(ILogger<PaymentProcessedConsumer> logger, IMediator mediator, IOrderRepository orderRepository) : IConsumer<IPaymentProcessed>
{
    private Guid correlationId;
    public async Task Consume(ConsumeContext<IPaymentProcessed> context)
    {
        try
        {
            correlationId = context.CorrelationId ?? Guid.NewGuid();
            logger.LogInformation("[catalog-service] CorrelationId {correlationId} - PaymentProcessedConsumer - Received OrderId {orderId}",
                correlationId ,context.Message.OrderId);

            if (context.Message.PaymentStatus == PaymentStatus.Approved)
                await ProcessApprovedPayment(context.Message);
            else
                await ProcessRejectedPayment(context.Message);
        }
        catch (Exception ex)
        {
            logger.LogError("[catalog-service] CorrelationId {correlationId} - PaymentProcessedConsumer - Error processing OrderId {orderId}",
                correlationId ,context.Message.OrderId);
        }
    }

    private async Task ProcessApprovedPayment(IPaymentProcessed message)
    {
        logger.LogInformation("[catalog-service] CorrelationId {correlationId} - PaymentProcessedConsumer - Processing Approved OrderId {orderId}",
            correlationId ,message.OrderId);
        var order = await orderRepository.GetOrderByIdAsync(message.OrderId);
        if (order == null)
        {
            logger.LogError("[catalog-service] CorrelationId {correlationId} - PaymentProcessedConsumer - Order {orderId} not found.",
                correlationId, message.OrderId);
            return;
        }
        var updateOrderCommand = new UpdateOrderStatusCommand(order.Id, OrderStatus.Aprovado);

        if (await mediator.Send(updateOrderCommand))
        {
            var addGameCommand = new CreateLibraryItemCommand(order.UserId, order.GameId, order.Id);
            await mediator.Send(addGameCommand);
        }
        else
            logger.LogError("[catalog-service] CorrelationId {correlationId} - PaymentProcessedConsumer - Order {orderId} - failed to update status.",
                correlationId, message.OrderId);
    }
    private async Task ProcessRejectedPayment(IPaymentProcessed message)
    {
        logger.LogInformation("[catalog-service] CorrelationId {correlationId} - PaymentProcessedConsumer - Processing Reproved OrderId {orderId}",
            correlationId, message.OrderId);
        var order = await orderRepository.GetOrderByIdAsync(message.OrderId);
        if (order == null)
        {
            logger.LogError("[catalog-service] CorrelationId {correlationId} - PaymentProcessedConsumer - Order {orderId} not found.",
                correlationId, message.OrderId);
            return;
        }

        var updateOrderCommand = new UpdateOrderStatusCommand(order.Id, OrderStatus.Rejeitado);
        await mediator.Send(updateOrderCommand);
    }
}
