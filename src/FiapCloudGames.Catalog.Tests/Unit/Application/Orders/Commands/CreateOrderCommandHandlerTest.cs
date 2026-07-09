using FiapCloudGames.Catalog.Application.Features.OrderFeature.Commands.CreateOrder;
using FiapCloudGames.Catalog.Domain.Contracts.Publishers;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.Orders.Commands;

/// <summary>
/// Testes unitários do CreateOrderCommandHandler, responsável por criar
/// pedidos validando regras de negócio como jogo inexistente, jogo já na biblioteca
/// e pedido pendente duplicado.
/// </summary>
public class CreateOrderCommandHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IGameRepository> _gameRepositoryMock = new();
    private readonly Mock<IOrderPlacedPublisher> _orderPlacedPublisherMock = new();
    private readonly Mock<ILibraryItemRepository> _libraryItemRepositoryMock = new();
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTest()
    {
        _handler = new CreateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _gameRepositoryMock.Object,
            _orderPlacedPublisherMock.Object,
            _libraryItemRepositoryMock.Object);
    }

    /// <summary>
    /// Cria um Game válido para uso nos testes.
    /// </summary>
    private static Game CreateGame() =>
        new("Game Title", "Descrição válida do jogo para testes.", new DateTime(2020, 1, 1),
            "Developer Studio", 59.90m, [new Category("RPG")]);

    /// <summary>
    /// Cria um CreateOrderCommand com valores padrão.
    /// </summary>
    private static CreateOrderCommand BuildCommand(Guid? userId = null, Guid? gameId = null) =>
        new(
            userId ?? Guid.NewGuid(),
            "user@test.com",
            "User Test",
            gameId ?? Guid.NewGuid()
        );

    /// <summary>
    /// Garante que, quando todos os dados são válidos, o pedido é criado e publicado
    /// na fila, retornando true.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValid_ShouldCreateOrderAndPublishEvent()
    {
        // Arrange
        var game = CreateGame();
        var command = BuildCommand(gameId: game.Id);
        const int newOrderId = 10;

        _gameRepositoryMock.Setup(r => r.GetByIdAsync(command.GameId)).ReturnsAsync(game);
        _libraryItemRepositoryMock.Setup(r => r.GetByUserIdAsync(command.UserId)).ReturnsAsync([]);
        _orderRepositoryMock.Setup(r => r.GetOrdersByUserIdAsync(command.UserId)).ReturnsAsync([]);
        _orderRepositoryMock.Setup(r => r.AddOrderAsync(It.IsAny<Order>())).ReturnsAsync(newOrderId);
        _orderPlacedPublisherMock
            .Setup(p => p.PublishAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _orderRepositoryMock.Verify(r => r.AddOrderAsync(It.IsAny<Order>()), Times.Once);
        _orderPlacedPublisherMock.Verify(p => p.PublishAsync(
            newOrderId,
            command.UserId,
            game.Id,
            game.Price,
            command.Email,
            command.Name,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que uma BusinessException é lançada quando o jogo
    /// informado no comando não existe no repositório.
    /// </summary>
    [Fact]
    public async Task Handle_WhenGameNotFound_ShouldThrowBusinessException()
    {
        // Arrange
        var command = BuildCommand();

        _gameRepositoryMock.Setup(r => r.GetByIdAsync(command.GameId)).ReturnsAsync((Game?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage($"Jogo com Id '{command.GameId}' não encontrado.");
    }

    /// <summary>
    /// Garante que uma BusinessException é lançada quando o usuário
    /// já possui o jogo na sua biblioteca.
    /// </summary>
    [Fact]
    public async Task Handle_WhenGameAlreadyInLibrary_ShouldThrowBusinessException()
    {
        // Arrange
        var game = CreateGame();
        var command = BuildCommand(gameId: game.Id);
        var libraryItem = new LibraryItem(command.UserId, game);

        _gameRepositoryMock.Setup(r => r.GetByIdAsync(command.GameId)).ReturnsAsync(game);
        _libraryItemRepositoryMock.Setup(r => r.GetByUserIdAsync(command.UserId))
            .ReturnsAsync([libraryItem]);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("Usuário já possui esse jogo");
    }

    /// <summary>
    /// Garante que uma BusinessException é lançada quando o usuário
    /// já possui um pedido com status OrderStatus.Pendente para o mesmo jogo.
    /// </summary>
    [Fact]
    public async Task Handle_WhenPendingOrderAlreadyExists_ShouldThrowBusinessException()
    {
        // Arrange
        var game = CreateGame();
        var command = BuildCommand(gameId: game.Id);
        var existingOrder = new Order(command.UserId, game);

        _gameRepositoryMock.Setup(r => r.GetByIdAsync(command.GameId)).ReturnsAsync(game);
        _libraryItemRepositoryMock.Setup(r => r.GetByUserIdAsync(command.UserId)).ReturnsAsync([]);
        _orderRepositoryMock.Setup(r => r.GetOrdersByUserIdAsync(command.UserId))
            .ReturnsAsync([existingOrder]);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("Usuário já possui um pedido pendente para esse jogo");
    }

    /// <summary>
    /// Garante que o repositório de pedidos não é chamado para criar o pedido
    /// quando o jogo não é encontrado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenGameNotFound_ShouldNotCallAddOrderAsync()
    {
        // Arrange
        var command = BuildCommand();
        _gameRepositoryMock.Setup(r => r.GetByIdAsync(command.GameId)).ReturnsAsync((Game?)null);

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { /* ignored */ }

        // Assert
        _orderRepositoryMock.Verify(r => r.AddOrderAsync(It.IsAny<Order>()), Times.Never);
    }

    /// <summary>
    /// Garante que o publisher não é chamado quando o jogo já está na biblioteca do usuário.
    /// </summary>
    [Fact]
    public async Task Handle_WhenGameAlreadyInLibrary_ShouldNotPublishEvent()
    {
        // Arrange
        var game = CreateGame();
        var command = BuildCommand(gameId: game.Id);
        var libraryItem = new LibraryItem(command.UserId, game);

        _gameRepositoryMock.Setup(r => r.GetByIdAsync(command.GameId)).ReturnsAsync(game);
        _libraryItemRepositoryMock.Setup(r => r.GetByUserIdAsync(command.UserId))
            .ReturnsAsync([libraryItem]);

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { /* ignored */ }

        // Assert
        _orderPlacedPublisherMock.Verify(p => p.PublishAsync(
            It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}