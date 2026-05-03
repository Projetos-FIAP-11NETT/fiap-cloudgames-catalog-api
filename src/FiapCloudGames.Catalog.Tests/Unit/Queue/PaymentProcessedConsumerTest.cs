using FiapCloudGames.Catalog.Application.LibraryItemFeature.Commands.CreateLibraryItem;
using FiapCloudGames.Catalog.Application.OrderFeature.Commands.UpdateOrderStatus;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Enums;
using FiapCloudGames.Queue.Consumers.Sqs;
using FiapCloudGames.Queue.Contracts;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Queue;

/// <summary>
/// Testes unitários do <see cref="PaymentProcessedConsumer"/>, responsável por consumir
/// mensagens <see cref="IPaymentProcessed"/> da fila SQS e orquestrar a atualização
/// do status do pedido e a adição do jogo à biblioteca do usuário.
/// </summary>
public class PaymentProcessedConsumerTest
{
    private readonly Mock<ILogger<PaymentProcessedConsumer>> _loggerMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly PaymentProcessedConsumer _consumer;

    public PaymentProcessedConsumerTest()
    {
        _consumer = new PaymentProcessedConsumer(
            _loggerMock.Object,
            _mediatorMock.Object,
            _orderRepositoryMock.Object);
    }

    /// <summary>
    /// Cria um <see cref="ConsumeContext{IPaymentProcessed}"/> mockado com os dados informados.
    /// </summary>
    private static Mock<ConsumeContext<IPaymentProcessed>> BuildConsumeContext(
        int orderId = 42,
        PaymentStatus status = PaymentStatus.Approved,
        decimal amount = 99.9m,
        string email = "test@test.com",
        string name = "Test",
        Guid? correlationId = null)
    {
        var messageMock = new Mock<IPaymentProcessed>();
        messageMock.Setup(m => m.OrderId).Returns(orderId);
        messageMock.Setup(m => m.PaymentStatus).Returns(status);
        messageMock.Setup(m => m.Amount).Returns(amount);
        messageMock.Setup(m => m.Email).Returns(email);
        messageMock.Setup(m => m.Name).Returns(name);
        messageMock.Setup(m => m.PaymentDate).Returns(DateTimeOffset.UtcNow);

        var contextMock = new Mock<ConsumeContext<IPaymentProcessed>>();
        contextMock.Setup(c => c.Message).Returns(messageMock.Object);
        contextMock.Setup(c => c.CorrelationId).Returns(correlationId ?? Guid.NewGuid());

        return contextMock;
    }

    /// <summary>
    /// Cria uma <see cref="Order"/> válida com o Id definido via reflection,
    /// simulando uma entidade persistida no banco de dados.
    /// </summary>
    private static Order CreateOrder(int orderId)
    {
        var game = new Game("Game Test", "Descrição válida do jogo", new DateTime(2020, 1, 1), "Dev Studio", 49.90m,
            new List<Category> { new("RPG") });
        var order = new Order(Guid.NewGuid(), game);

        typeof(Order)
            .GetProperty(nameof(Order.Id))!
            .SetValue(order, orderId);

        return order;
    }

    /// <summary>
    /// Garante que, ao receber um pagamento aprovado, o consumer atualiza o status do pedido
    /// para Aprovado e adiciona o jogo à biblioteca do usuário.
    /// </summary>
    [Fact]
    public async Task Consume_WhenPaymentApproved_ShouldUpdateOrderAndAddToLibrary()
    {
        // Arrange
        const int orderId = 1;
        var order = CreateOrder(orderId);
        var context = BuildConsumeContext(orderId: orderId, status: PaymentStatus.Approved);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), default)).ReturnsAsync(true);
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateLibraryItemCommand>(), default)).ReturnsAsync(true);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateOrderStatusCommand>(c => c.OrderId == orderId && c.Status == OrderStatus.Aprovado),
            default), Times.Once);

        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateLibraryItemCommand>(c => c.UserId == order.UserId && c.GameId == order.GameId && c.OrderId == orderId),
            default), Times.Once);
    }

    /// <summary>
    /// Garante que, ao receber um pagamento rejeitado, o consumer atualiza o status do pedido
    /// para Rejeitado e não adiciona o jogo à biblioteca.
    /// </summary>
    [Fact]
    public async Task Consume_WhenPaymentRejected_ShouldUpdateOrderStatusToRejected()
    {
        // Arrange
        const int orderId = 1;
        var order = CreateOrder(orderId);
        var context = BuildConsumeContext(orderId: orderId, status: PaymentStatus.Rejected);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), default)).ReturnsAsync(true);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateOrderStatusCommand>(c => c.OrderId == orderId && c.Status == OrderStatus.Rejeitado),
            default), Times.Once);

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<CreateLibraryItemCommand>(), default), Times.Never);
    }

    /// <summary>
    /// Confirma que o CorrelationId do contexto é capturado pelo consumer,
    /// garantindo rastreabilidade distribuída nos logs do serviço.
    /// </summary>
    [Fact]
    public async Task Consume_ShouldLogCorrelationIdFromContext()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        const int orderId = 10;
        var order = CreateOrder(orderId);
        var context = BuildConsumeContext(orderId: orderId, correlationId: correlationId);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), default)).ReturnsAsync(true);
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateLibraryItemCommand>(), default)).ReturnsAsync(true);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.Contains(correlationId.ToString()) &&
                    v.ToString()!.Contains(orderId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Verifica que, ao receber um pagamento aprovado para um pedido inexistente,
    /// nenhum command é enviado ao MediatR e um log de erro é registrado.
    /// </summary>
    [Fact]
    public async Task Consume_WhenApprovedAndOrderNotFound_ShouldNotSendCommandsAndLogError()
    {
        // Arrange
        const int orderId = 999;
        var context = BuildConsumeContext(orderId: orderId, status: PaymentStatus.Approved);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync((Order?)null);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), default), Times.Never);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateLibraryItemCommand>(), default), Times.Never);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(orderId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifica que, ao receber um pagamento rejeitado para um pedido inexistente,
    /// nenhum command é enviado ao MediatR e um log de erro é registrado.
    /// </summary>
    [Fact]
    public async Task Consume_WhenRejectedAndOrderNotFound_ShouldNotSendCommandsAndLogError()
    {
        // Arrange
        const int orderId = 999;
        var context = BuildConsumeContext(orderId: orderId, status: PaymentStatus.Rejected);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync((Order?)null);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), default), Times.Never);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(orderId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifica que, quando a atualização do status do pedido falha,
    /// o jogo não é adicionado à biblioteca do usuário e um log de erro é registrado.
    /// </summary>
    [Fact]
    public async Task Consume_WhenUpdateFails_ShouldNotAddToLibraryAndLogError()
    {
        // Arrange
        const int orderId = 1;
        var order = CreateOrder(orderId);
        var context = BuildConsumeContext(orderId: orderId, status: PaymentStatus.Approved);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), default)).ReturnsAsync(false);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.IsAny<CreateLibraryItemCommand>(), default), Times.Never);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(orderId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Cobre o tratamento de erros: quando o repositório lança uma exceção,
    /// o consumer não deve propagar o erro, garantindo resiliência na fila SQS.
    /// </summary>
    [Fact]
    public async Task Consume_WhenRepositoryThrows_ShouldLogErrorAndNotThrow()
    {
        // Arrange
        const int orderId = 1;
        var context = BuildConsumeContext(orderId: orderId);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId))
            .ThrowsAsync(new InvalidOperationException("DB unavailable"));

        // Act
        var act = async () => await _consumer.Consume(context.Object);

        // Assert
        await act.Should().NotThrowAsync();

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(orderId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Valida que uma falha no mediator também é capturada pelo bloco catch,
    /// registrando o erro sem propagar a exceção para o MassTransit.
    /// </summary>
    [Fact]
    public async Task Consume_WhenMediatorThrows_ShouldLogErrorAndNotThrow()
    {
        // Arrange
        const int orderId = 77;
        var order = CreateOrder(orderId);
        var context = BuildConsumeContext(orderId: orderId, status: PaymentStatus.Approved);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), default))
            .ThrowsAsync(new Exception("Mediator pipeline error"));

        // Act
        var act = async () => await _consumer.Consume(context.Object);

        // Assert
        await act.Should().NotThrowAsync();

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(orderId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifica se o UpdateOrderStatusCommand enviado ao mediator contém
    /// exatamente o OrderId e Status corretos para pagamento aprovado.
    /// </summary>
    [Fact]
    public async Task Consume_WhenApproved_ShouldSendUpdateCommandWithCorrectData()
    {
        // Arrange
        const int orderId = 42;
        var order = CreateOrder(orderId);
        var context = BuildConsumeContext(orderId: orderId, status: PaymentStatus.Approved);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), default)).ReturnsAsync(true);
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateLibraryItemCommand>(), default)).ReturnsAsync(true);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateOrderStatusCommand>(cmd =>
                cmd.OrderId == orderId &&
                cmd.Status == OrderStatus.Aprovado),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Confirma que os campos UserId, GameId e OrderId são repassados ao
    /// CreateLibraryItemCommand exatamente como vindos da entidade Order.
    /// </summary>
    [Fact]
    public async Task Consume_WhenApproved_ShouldSendCreateLibraryItemWithCorrectData()
    {
        // Arrange
        const int orderId = 55;
        var order = CreateOrder(orderId);
        var context = BuildConsumeContext(orderId: orderId, status: PaymentStatus.Approved);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), default)).ReturnsAsync(true);
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateLibraryItemCommand>(), default)).ReturnsAsync(true);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateLibraryItemCommand>(cmd =>
                cmd.UserId == order.UserId &&
                cmd.GameId == order.GameId &&
                cmd.OrderId == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
