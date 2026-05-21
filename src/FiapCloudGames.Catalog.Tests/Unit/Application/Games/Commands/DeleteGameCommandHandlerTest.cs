using FiapCloudGames.Catalog.Application.Features.GameFeature.Commands.DeleteGame;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.Games.Commands;

/// <summary>
/// Testes unitários do DeleteGameCommandHandler, responsável por remover um jogo
/// validando sua existência, persistindo a remoção e invalidando o cache no Redis.
/// </summary>
public class DeleteGameCommandHandlerTest
{
    private readonly Mock<IGameRepository> _gameRepositoryMock = new();
    private readonly Mock<IRedisRepository> _redisRepositoryMock = new();
    private readonly DeleteGameCommandHandler _handler;

    public DeleteGameCommandHandlerTest()
    {
        _handler = new DeleteGameCommandHandler(
            _gameRepositoryMock.Object,
            _redisRepositoryMock.Object);
    }

    /// <summary>
    /// Cria um Game válido para uso nos testes.
    /// </summary>
    private static Game CreateGame() =>
        new("Game Title", "Descrição válida do jogo para testes.", new DateTime(2020, 1, 1),
            "Developer Studio", 59.90m, [new Category("RPG")]);

    /// <summary>
    /// Garante que, quando o jogo existe e SaveChanges é bem-sucedido, o jogo é removido,
    /// o cache do Redis é invalidado e true é retornado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValid_ShouldRemoveGameAndInvalidateCacheAndReturnTrue()
    {
        // Arrange
        var game = CreateGame();
        var command = new DeleteGameCommand(game.Id);

        _gameRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync(game);
        _gameRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _redisRepositoryMock
            .Setup(r => r.RemoveKeysThatContainGameAsync(game.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _gameRepositoryMock.Verify(r => r.Remove(game), Times.Once);
        _gameRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _redisRepositoryMock.Verify(r => r.RemoveKeysThatContainGameAsync(game.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que uma NotFoundException é lançada quando o jogo
    /// informado no comando não existe no repositório.
    /// </summary>
    [Fact]
    public async Task Handle_WhenGameNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteGameCommand(Guid.NewGuid());

        _gameRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync((Game?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Jogo*");

        _gameRepositoryMock.Verify(r => r.Remove(It.IsAny<Game>()), Times.Never);
        _gameRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _redisRepositoryMock.Verify(r => r.RemoveKeysThatContainGameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que o cache do Redis não é invalidado quando SaveChanges falha,
    /// e false é retornado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldNotInvalidateCacheAndReturnFalse()
    {
        // Arrange
        var game = CreateGame();
        var command = new DeleteGameCommand(game.Id);

        _gameRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync(game);
        _gameRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        _redisRepositoryMock.Verify(r => r.RemoveKeysThatContainGameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que o Id do jogo removido é repassado corretamente ao Redis
    /// para invalidação das chaves de cache.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValid_ShouldInvalidateCacheWithCorrectGameId()
    {
        // Arrange
        var game = CreateGame();
        var command = new DeleteGameCommand(game.Id);

        _gameRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync(game);
        _gameRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _redisRepositoryMock
            .Setup(r => r.RemoveKeysThatContainGameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _redisRepositoryMock.Verify(r => r.RemoveKeysThatContainGameAsync(
            game.Id,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que o Id informado no comando é repassado corretamente ao repositório
    /// para busca do jogo a ser removido.
    /// </summary>
    [Fact]
    public async Task Handle_Always_ShouldQueryRepositoryWithCorrectGameId()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var command = new DeleteGameCommand(gameId);

        _gameRepositoryMock
            .Setup(r => r.GetByIdAsync(gameId))
            .ReturnsAsync((Game?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _gameRepositoryMock.Verify(r => r.GetByIdAsync(gameId), Times.Once);
    }
}