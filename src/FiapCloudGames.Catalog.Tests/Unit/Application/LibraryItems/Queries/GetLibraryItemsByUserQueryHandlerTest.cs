using FiapCloudGames.Catalog.Application.Features.LibraryItemFeature.Queries.GetLibraryItemsByUser;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;
using FiapCloudGames.Catalog.Domain.ReadModels;
using FluentAssertions;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.LibraryItems.Queries;

/// <summary>
/// Testes unitários do GetLibraryItemsByUserQueryHandler, responsável por retornar
/// os itens da biblioteca de um usuário a partir do repositório MongoDB.
/// </summary>
public class GetLibraryItemsByUserQueryHandlerTest
{
    private readonly Mock<ILibraryItemRepository> _mongoLibraryItemRepositoryMock = new();
    private readonly GetLibraryItemsByUserQueryHandler _handler;

    public GetLibraryItemsByUserQueryHandlerTest()
    {
        _handler = new GetLibraryItemsByUserQueryHandler(_mongoLibraryItemRepositoryMock.Object);
    }

    /// <summary>
    /// Cria um LibraryItemReadModel com jogos para uso nos testes.
    /// </summary>
    private static LibraryItemReadModel CreateLibraryReadModel(Guid userId, int gameCount = 2)
    {
        var games = Enumerable.Range(1, gameCount).Select(i => new GameInLibraryReadModel
        {
            GameId = Guid.NewGuid(),
            GameTitle = $"Game {i}",
            GamePrice = 59.90m * i,
            OrderId = i,
            AddedAt = DateTime.UtcNow
        }).ToList();

        return new LibraryItemReadModel
        {
            UserId = userId,
            Games = games
        };
    }

    /// <summary>
    /// Garante que o UserId da query é repassado corretamente ao repositório MongoDB.
    /// </summary>
    [Fact]
    public async Task Handle_Always_ShouldCallMongoRepositoryWithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetLibraryItemsByUserQuery(userId);

        _mongoLibraryItemRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((LibraryItemReadModel?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mongoLibraryItemRepositoryMock.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    /// <summary>
    /// Garante que uma coleção vazia é retornada quando o usuário não possui
    /// biblioteca registrada no MongoDB.
    /// </summary>
    [Fact]
    public async Task Handle_WhenLibraryNotFound_ShouldReturnEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mongoLibraryItemRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((LibraryItemReadModel?)null);

        // Act
        var result = await _handler.Handle(new GetLibraryItemsByUserQuery(userId), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Garante que todos os jogos da biblioteca são retornados mapeados
    /// para GetLibraryItemResponse quando a biblioteca existe.
    /// </summary>
    [Fact]
    public async Task Handle_WhenLibraryExists_ShouldReturnMappedResponses()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var library = CreateLibraryReadModel(userId, gameCount: 3);

        _mongoLibraryItemRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(library);

        // Act
        var result = await _handler.Handle(new GetLibraryItemsByUserQuery(userId), CancellationToken.None);

        // Assert
        var responses = result.ToList();

        responses.Should().HaveCount(3);
    }

    /// <summary>
    /// Garante que os campos do DTO são mapeados corretamente a partir do GameInLibraryReadModel.
    /// </summary>
    [Fact]
    public async Task Handle_WhenLibraryHasOneGame_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var game = new GameInLibraryReadModel
        {
            GameId = Guid.NewGuid(),
            GameTitle = "Jogo Teste",
            GamePrice = 99.90m,
            OrderId = 42,
            AddedAt = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        var library = new LibraryItemReadModel
        {
            UserId = userId,
            Games = [game]
        };

        _mongoLibraryItemRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(library);

        // Act
        var result = await _handler.Handle(new GetLibraryItemsByUserQuery(userId), CancellationToken.None);

        // Assert
        var response = result.Single();

        response.GameId.Should().Be(game.GameId);
        response.GameTitle.Should().Be(game.GameTitle);
        response.OrderId.Should().Be(game.OrderId);
        response.AddedAt.Should().Be(game.AddedAt);
    }

    /// <summary>
    /// Garante que uma coleção vazia é retornada quando a biblioteca
    /// do usuário existe mas não possui jogos.
    /// </summary>
    [Fact]
    public async Task Handle_WhenLibraryHasNoGames_ShouldReturnEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var library = new LibraryItemReadModel
        {
            UserId = userId,
            Games = []
        };

        _mongoLibraryItemRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(library);

        // Act
        var result = await _handler.Handle(new GetLibraryItemsByUserQuery(userId), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
