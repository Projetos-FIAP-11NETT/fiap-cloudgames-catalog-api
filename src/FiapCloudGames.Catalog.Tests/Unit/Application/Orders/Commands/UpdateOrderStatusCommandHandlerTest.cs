using FiapCloudGames.Catalog.Application.Features.OrderFeature.Commands.UpdateOrderStatus;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Enums;
using FiapCloudGames.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.Orders.Commands;

/// <summary>
/// Testes unitários do UpdateOrderStatusCommandHandler, responsável por
/// atualizar o status de um pedido para Aprovado, Rejeitado ou Cancelado.
/// </summary>
public class UpdateOrderStatusCommandHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly UpdateOrderStatusCommandHandler _handler;

    public UpdateOrderStatusCommandHandlerTest()
    {
        _handler = new UpdateOrderStatusCommandHandler(_orderRepositoryMock.Object);
    }

    /// <summary>
    /// Cria um <see cref="Game"/> válido para uso nos testes.
    /// </summary>
    private static Game CreateGame() =>
        new("Game Title", "Descrição válida do jogo para testes.", new DateTime(2020, 1, 1),
            "Developer Studio", 59.90m, [new Category("RPG")]);

    /// <summary>
    /// Cria um Order com Id definido via reflection, simulando entidade persistida.
    /// </summary>
    private static Order CreateOrder(int orderId)
    {
        var order = new Order(Guid.NewGuid(), CreateGame());

        typeof(Order)
            .GetProperty(nameof(Order.Id))!
            .SetValue(order, orderId);

        return order;
    }

    /// <summary>
    /// Garante que, quando o pedido não existe, o handler retorna false
    /// sem chamar Update nem SaveChangesAsync.
    /// </summary>
    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldReturnFalse()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(99, OrderStatus.Aprovado);
        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(command.OrderId)).ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que, quando um status inválido/não mapeado é informado, o handler retorna false
    /// sem chamar Update nem SaveChangesAsync.
    /// </summary>
    [Fact]
    public async Task Handle_WhenStatusIsNotMapped_ShouldReturnFalse()
    {
        // Arrange
        const int orderId = 1;
        var order = CreateOrder(orderId);
        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Pendente);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que, ao receber o status Aprovado, o handler aprova o pedido,
    /// chama Update e SaveChangesAsync e retorna true.
    /// </summary>
    [Fact]
    public async Task Handle_WhenStatusIsAprovado_ShouldAprovarOrderAndReturnTrue()
    {
        // Arrange
        const int orderId = 1;
        var order = CreateOrder(orderId);
        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Aprovado);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Aprovado);
        _orderRepositoryMock.Verify(r => r.Update(order), Times.Once);
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que, ao receber o status Rejeitado, o handler rejeita o pedido,
    /// chama Update e SaveChangesAsync e retorna true.
    /// </summary>
    [Fact]
    public async Task Handle_WhenStatusIsRejeitado_ShouldRejeitarOrderAndReturnTrue()
    {
        // Arrange
        const int orderId = 2;
        var order = CreateOrder(orderId);
        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Rejeitado);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Rejeitado);
        _orderRepositoryMock.Verify(r => r.Update(order), Times.Once);
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que, ao receber o status Cancelado, o handler cancela o pedido,
    /// chama Update e SaveChangesAsync e retorna true.
    /// </summary>
    [Fact]
    public async Task Handle_WhenStatusIsCancelado_ShouldCancelarOrderAndReturnTrue()
    {
        // Arrange
        const int orderId = 3;
        var order = CreateOrder(orderId);
        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Cancelado);

        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelado);
        _orderRepositoryMock.Verify(r => r.Update(order), Times.Once);
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que aprovar um pedido já aprovado lança DomainException,
    /// sem persistir a mudança.
    /// </summary>
    [Fact]
    public async Task Handle_WhenOrderAlreadyAprovado_ShouldThrowDomainException()
    {
        // Arrange
        const int orderId = 4;
        var order = CreateOrder(orderId);
        order.Aprovar();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Aprovado);
        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("O pedido já foi aprovado.");

        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que rejeitar um pedido já rejeitado lança DomainException,
    /// sem persistir a mudança.
    /// </summary>
    [Fact]
    public async Task Handle_WhenOrderAlreadyRejeitado_ShouldThrowDomainException()
    {
        // Arrange
        const int orderId = 5;
        var order = CreateOrder(orderId);
        order.Rejeitar();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Rejeitado);
        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("O pedido já foi rejeitado.");

        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que cancelar um pedido já aprovado lança DomainException,
    /// sem persistir a mudança.
    /// </summary>
    [Fact]
    public async Task Handle_WhenOrderIsAprovadoAndCancelRequested_ShouldThrowDomainException()
    {
        // Arrange
        const int orderId = 6;
        var order = CreateOrder(orderId);
        order.Aprovar();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Cancelado);
        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Não é possível cancelar pedidos já processados.");

        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
