using FiapCloudGames.Catalog.Application.Features.OrderFeature.Queries.GetMyOrders;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Enums;
using FluentAssertions;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.Orders.Queries;

/// <summary>
/// Testes unitários do GetMyOrdersQueryHandler, responsável por retornar
/// os pedidos de um usuário específico mapeados para o DTO GetOrderResponse.
/// </summary>
public class GetMyOrdersQueryHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly GetMyOrdersQueryHandler _handler;

    public GetMyOrdersQueryHandlerTest()
    {
        _handler = new GetMyOrdersQueryHandler(_orderRepositoryMock.Object);
    }

    /// <summary>
    /// Cria um Game válido para uso nos testes.
    /// </summary>
    private static Game CreateGame(string title = "Game Title") =>
        new(title, "Descrição válida do jogo para testes.", new DateTime(2020, 1, 1),
            "Developer Studio", 59.90m, [new Category("RPG")]);

    /// <summary>
    /// Garante que o UserId da query é repassado corretamente ao repositório.
    /// </summary>
    [Fact]
    public async Task Handle_Always_ShouldCallRepositoryWithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetMyOrdersQuery(userId);

        _orderRepositoryMock
            .Setup(r => r.GetOrdersByUserIdAsync(userId))
            .ReturnsAsync([]);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _orderRepositoryMock.Verify(r => r.GetOrdersByUserIdAsync(userId), Times.Once);
    }

    /// <summary>
    /// Garante que, quando o usuário possui pedidos, todos são retornados
    /// corretamente mapeados para GetOrderResponse.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserHasOrders_ShouldReturnMappedResponses()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var game = CreateGame();
        var orders = new List<Order>
        {
            new(userId, game),
            new(userId, game)
        };

        _orderRepositoryMock
            .Setup(r => r.GetOrdersByUserIdAsync(userId))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(new GetMyOrdersQuery(userId), CancellationToken.None);

        // Assert
        var responses = result.ToList();

        responses.Should().HaveCount(2);
        responses.Should().AllSatisfy(r =>
        {
            r.UserId.Should().Be(userId);
            r.GameTitle.Should().Be(game.Title);
            r.TotalAmount.Should().Be(game.Price);
            r.Status.Should().Be(OrderStatus.Pendente);
            r.PaidAt.Should().BeNull();
        });
    }

    /// <summary>
    /// Garante que os campos do DTO são mapeados corretamente a partir da entidade Order.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserHasOneOrder_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var game = CreateGame("Jogo Teste");
        var order = new Order(userId, game);

        _orderRepositoryMock
            .Setup(r => r.GetOrdersByUserIdAsync(userId))
            .ReturnsAsync([order]);

        // Act
        var result = await _handler.Handle(new GetMyOrdersQuery(userId), CancellationToken.None);

        // Assert
        var response = result.Single();

        response.UserId.Should().Be(order.UserId);
        response.GameId.Should().Be(order.GameId);
        response.GameTitle.Should().Be(game.Title);
        response.Status.Should().Be(order.Status);
        response.TotalAmount.Should().Be(order.TotalAmount);
        response.CreatedAt.Should().Be(order.CreatedAt);
        response.PaidAt.Should().Be(order.PaidAt);
    }

    /// <summary>
    /// Garante que uma coleção vazia é retornada quando o usuário não possui pedidos.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserHasNoOrders_ShouldReturnEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(r => r.GetOrdersByUserIdAsync(userId))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(new GetMyOrdersQuery(userId), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Garante que pedidos de outros usuários não são retornados para o usuário solicitante.
    /// </summary>
    [Fact]
    public async Task Handle_WhenOtherUsersHaveOrders_ShouldReturnOnlyRequestingUserOrders()
    {
        // Arrange
        var requestingUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var game = CreateGame();

        var requestingUserOrders = new List<Order> { new(requestingUserId, game) };

        _orderRepositoryMock
            .Setup(r => r.GetOrdersByUserIdAsync(requestingUserId))
            .ReturnsAsync(requestingUserOrders);

        _orderRepositoryMock
            .Setup(r => r.GetOrdersByUserIdAsync(otherUserId))
            .ReturnsAsync([new Order(otherUserId, game)]);

        // Act
        var result = await _handler.Handle(new GetMyOrdersQuery(requestingUserId), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.Single().UserId.Should().Be(requestingUserId);
    }
}
