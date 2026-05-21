using FiapCloudGames.Catalog.Application.Features.OrderFeature.Queries.GetAllOrders;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Enums;
using FluentAssertions;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.Orders.Queries;

/// <summary>
/// Testes unitários do GetAllOrdersQueryHandler, responsável por retornar
/// todos os pedidos mapeados para o DTO GetOrderResponse.
/// </summary>
public class GetAllOrdersQueryHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly GetAllOrdersQueryHandler _handler;

    public GetAllOrdersQueryHandlerTest()
    {
        _handler = new GetAllOrdersQueryHandler(_orderRepositoryMock.Object);
    }

    /// <summary>
    /// Cria um Game válido para uso nos testes.
    /// </summary>
    private static Game CreateGame(string title = "Game Title") =>
        new(title, "Descrição válida do jogo para testes.", new DateTime(2020, 1, 1),
            "Developer Studio", 59.90m, [new Category("RPG")]);

    /// <summary>
    /// Garante que, quando há pedidos no repositório, todos são retornados
    /// corretamente mapeados para GetOrderResponse.
    /// </summary>
    [Fact]
    public async Task Handle_WhenOrdersExist_ShouldReturnMappedResponses()
    {
        // Arrange
        var game = CreateGame();
        var orders = new List<Order>
        {
            new(Guid.NewGuid(), game),
            new(Guid.NewGuid(), game)
        };

        _orderRepositoryMock
            .Setup(r => r.GetAllOrdersAsync())
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(new GetAllOrdersQuery(), CancellationToken.None);

        // Assert
        var responses = result.ToList();

        responses.Should().HaveCount(2);
        responses.Should().AllSatisfy(r =>
        {
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
    public async Task Handle_WhenOrderExists_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var game = CreateGame("Jogo Teste");
        var userId = Guid.NewGuid();
        var order = new Order(userId, game);

        _orderRepositoryMock
            .Setup(r => r.GetAllOrdersAsync())
            .ReturnsAsync([order]);

        // Act
        var result = await _handler.Handle(new GetAllOrdersQuery(), CancellationToken.None);

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
    /// Garante que uma lista vazia é retornada quando não existem pedidos no repositório.
    /// </summary>
    [Fact]
    public async Task Handle_WhenNoOrdersExist_ShouldReturnEmptyCollection()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(r => r.GetAllOrdersAsync())
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(new GetAllOrdersQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Garante que o repositório é chamado exatamente uma vez por execução do handler.
    /// </summary>
    [Fact]
    public async Task Handle_Always_ShouldCallRepositoryOnce()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(r => r.GetAllOrdersAsync())
            .ReturnsAsync([]);

        // Act
        await _handler.Handle(new GetAllOrdersQuery(), CancellationToken.None);

        // Assert
        _orderRepositoryMock.Verify(r => r.GetAllOrdersAsync(), Times.Once);
    }
}