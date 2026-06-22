using FiapCloudGames.Catalog.Application.Features.GameFeature.Commands.UpdateGame;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.Games.Commands;

/// <summary>
/// Testes unitários do UpdateGameCommandHandler, responsável por atualizar um jogo
/// validando sua existência, duplicidade de título e desenvolvedor, obrigatoriedade
/// de categorias, persistência e invalidação do cache no Redis.
/// </summary>
public class UpdateGameCommandHandlerTest
{
    private readonly Mock<IGameRepository> _gameRepositoryMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<IRedisRepository> _redisRepositoryMock = new();
    private readonly UpdateGameCommandHandler _handler;

    public UpdateGameCommandHandlerTest()
    {
        _handler = new UpdateGameCommandHandler(
            _gameRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _redisRepositoryMock.Object);
    }

    /// <summary>
    /// Cria um Game válido para uso nos testes.
    /// </summary>
    private static Game CreateGame() =>
        new("Game Title", "Descrição válida do jogo para testes.", new DateTime(2020, 1, 1),
            "Developer Studio", 59.90m, [new Category("RPG")]);

    /// <summary>
    /// Cria um UpdateGameCommand com valores padrão.
    /// </summary>
    private static UpdateGameCommand BuildCommand(
        Guid? id = null,
        string title = "Game Title Updated",
        string developer = "Developer Studio",
        List<Guid>? categories = null) =>
        new(
            Id: id ?? Guid.NewGuid(),
            Title: title,
            Description: "Descrição atualizada do jogo.",
            ReleaseDate: new DateTime(2021, 6, 1),
            Developer: developer,
            Price: 79.90m,
            Categories: categories ?? [Guid.NewGuid()]
        );

    /// <summary>
    /// Cria uma lista de categorias válidas para uso nos testes.
    /// </summary>
    private static List<Category> CreateCategories(int count = 1) =>
        Enumerable.Range(1, count)
            .Select(_ => new Category("RPG"))
            .ToList();

    /// <summary>
    /// Garante que, quando todos os dados são válidos, o jogo é atualizado,
    /// o cache do Redis é invalidado e true é retornado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValid_ShouldUpdateGameAndInvalidateCacheAndReturnTrue()
    {
        // Arrange
        var game = CreateGame();
        var command = BuildCommand(id: game.Id);
        var categories = CreateCategories();

        _gameRepositoryMock
            .Setup(r => r.GetByIdWithCategoriesAsync(command.Id))
            .ReturnsAsync(game);
        _gameRepositoryMock
            .Setup(r => r.GameAlreadyExistsByTitle(command.Title, command.Developer, command.Id))
            .ReturnsAsync(false);
        _categoryRepositoryMock
            .Setup(r => r.GetByIdsAsync(command.Categories))
            .ReturnsAsync(categories);
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

        _gameRepositoryMock.Verify(r => r.Update(game), Times.Once);
        _gameRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _redisRepositoryMock.Verify(r => r.RemoveKeysThatContainGameAsync(game.Id, It.IsAny<CancellationToken>()), Times.Once);
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

        _gameRepositoryMock
            .Setup(r => r.GetByIdWithCategoriesAsync(command.Id))
            .ReturnsAsync((Game?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Jogo*");

        _gameRepositoryMock.Verify(r => r.GameAlreadyExistsByTitle(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        _categoryRepositoryMock.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
        _gameRepositoryMock.Verify(r => r.Update(It.IsAny<Game>()), Times.Never);
        _gameRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _redisRepositoryMock.Verify(r => r.RemoveKeysThatContainGameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que uma BusinessException é lançada quando já existe outro jogo
    /// com o mesmo título e desenvolvedor cadastrado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDuplicateGameExists_ShouldThrowBusinessException()
    {
        // Arrange
        var game = CreateGame();
        var command = BuildCommand(id: game.Id);

        _gameRepositoryMock
            .Setup(r => r.GetByIdWithCategoriesAsync(command.Id))
            .ReturnsAsync(game);
        _gameRepositoryMock
            .Setup(r => r.GameAlreadyExistsByTitle(command.Title, command.Developer, command.Id))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*título*desenvolvedor*");

        _categoryRepositoryMock.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
        _gameRepositoryMock.Verify(r => r.Update(It.IsAny<Game>()), Times.Never);
        _gameRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _redisRepositoryMock.Verify(r => r.RemoveKeysThatContainGameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que uma BusinessException é lançada quando nenhuma categoria
    /// válida é encontrada para os IDs informados.
    /// </summary>
    [Fact]
    public async Task Handle_WhenNoCategoriesFound_ShouldThrowBusinessException()
    {
        // Arrange
        var game = CreateGame();
        var command = BuildCommand(id: game.Id);

        _gameRepositoryMock
            .Setup(r => r.GetByIdWithCategoriesAsync(command.Id))
            .ReturnsAsync(game);
        _gameRepositoryMock
            .Setup(r => r.GameAlreadyExistsByTitle(command.Title, command.Developer, command.Id))
            .ReturnsAsync(false);
        _categoryRepositoryMock
            .Setup(r => r.GetByIdsAsync(command.Categories))
            .ReturnsAsync([]);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*categoria*");

        _gameRepositoryMock.Verify(r => r.Update(It.IsAny<Game>()), Times.Never);
        _gameRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _redisRepositoryMock.Verify(r => r.RemoveKeysThatContainGameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que o cache do Redis não é invalidado quando SaveChanges falha
    /// e false é retornado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldNotInvalidateCacheAndReturnFalse()
    {
        // Arrange
        var game = CreateGame();
        var command = BuildCommand(id: game.Id);
        var categories = CreateCategories();

        _gameRepositoryMock
            .Setup(r => r.GetByIdWithCategoriesAsync(command.Id))
            .ReturnsAsync(game);
        _gameRepositoryMock
            .Setup(r => r.GameAlreadyExistsByTitle(command.Title, command.Developer, command.Id))
            .ReturnsAsync(false);
        _categoryRepositoryMock
            .Setup(r => r.GetByIdsAsync(command.Categories))
            .ReturnsAsync(categories);
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
    /// Garante que a verificação de duplicidade usa o Id do jogo sendo atualizado
    /// para excluí-lo da comparação (overload com excludeId).
    /// </summary>
    [Fact]
    public async Task Handle_Always_ShouldCheckDuplicityExcludingCurrentGameId()
    {
        // Arrange
        var game = CreateGame();
        var command = BuildCommand(id: game.Id, title: "Novo Título", developer: "Novo Dev");

        _gameRepositoryMock
            .Setup(r => r.GetByIdWithCategoriesAsync(command.Id))
            .ReturnsAsync(game);
        _gameRepositoryMock
            .Setup(r => r.GameAlreadyExistsByTitle(command.Title, command.Developer, command.Id))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();

        _gameRepositoryMock.Verify(r =>
            r.GameAlreadyExistsByTitle("Novo Título", "Novo Dev", game.Id), Times.Once);
    }

    /// <summary>
    /// Garante que o jogo é atualizado com múltiplas categorias quando todas
    /// são encontradas no repositório.
    /// </summary>
    [Fact]
    public async Task Handle_WhenMultipleCategoriesProvided_ShouldUpdateGameWithAllCategories()
    {
        // Arrange
        var game = CreateGame();
        var categoryIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var command = BuildCommand(id: game.Id, categories: categoryIds);
        var categories = CreateCategories(count: 3);

        _gameRepositoryMock
            .Setup(r => r.GetByIdWithCategoriesAsync(command.Id))
            .ReturnsAsync(game);
        _gameRepositoryMock
            .Setup(r => r.GameAlreadyExistsByTitle(command.Title, command.Developer, command.Id))
            .ReturnsAsync(false);
        _categoryRepositoryMock
            .Setup(r => r.GetByIdsAsync(command.Categories))
            .ReturnsAsync(categories);
        _gameRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _redisRepositoryMock
            .Setup(r => r.RemoveKeysThatContainGameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _gameRepositoryMock.Verify(r => r.Update(
            It.Is<Game>(g => g.Categories.Count == 3)), Times.Once);
    }
}
