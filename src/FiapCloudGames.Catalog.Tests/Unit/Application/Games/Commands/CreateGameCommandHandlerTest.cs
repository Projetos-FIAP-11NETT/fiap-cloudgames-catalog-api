using FiapCloudGames.Catalog.Application.Features.GameFeature.Commands.CreateGame;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.Games.Commands;

/// <summary>
/// Testes unitários do CreateGameCommandHandler, responsável por criar jogos
/// validando duplicidade por título e desenvolvedor, obrigatoriedade de categorias,
/// persistência e retorno correto do resultado.
/// </summary>
public class CreateGameCommandHandlerTest
{
    private readonly Mock<IGameRepository> _gameRepositoryMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly CreateGameCommandHandler _handler;

    public CreateGameCommandHandlerTest()
    {
        _handler = new CreateGameCommandHandler(
            _gameRepositoryMock.Object,
            _categoryRepositoryMock.Object);
    }

    /// <summary>
    /// Cria um CreateGameCommand com valores padrão.
    /// </summary>
    private static CreateGameCommand BuildCommand(
        string title = "Game Title",
        string developer = "Developer Studio",
        List<Guid>? categories = null) =>
        new(
            Title: title,
            Description: "Descrição válida do jogo para testes.",
            ReleaseDate: new DateTime(2020, 1, 1),
            Developer: developer,
            Price: 59.90m,
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
    /// Garante que, quando todos os dados são válidos, o jogo é persistido
    /// e true é retornado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValid_ShouldCreateGameAndReturnTrue()
    {
        // Arrange
        var command = BuildCommand();
        var categories = CreateCategories();

        _gameRepositoryMock
            .Setup(r => r.GameAlreadyExistsByTitle(command.Title, command.Developer))
            .ReturnsAsync(false);
        _categoryRepositoryMock
            .Setup(r => r.GetByIdsAsync(command.Categories))
            .ReturnsAsync(categories);
        _gameRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);
        _gameRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _gameRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Game>()), Times.Once);
        _gameRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que uma BusinessException é lançada quando já existe um jogo
    /// com o mesmo título e desenvolvedor cadastrado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenGameAlreadyExists_ShouldThrowBusinessException()
    {
        // Arrange
        var command = BuildCommand();

        _gameRepositoryMock
            .Setup(r => r.GameAlreadyExistsByTitle(command.Title, command.Developer))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*título*desenvolvedor*");

        _categoryRepositoryMock.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
        _gameRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Game>()), Times.Never);
        _gameRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que uma BusinessException é lançada quando nenhuma categoria
    /// válida é encontrada para os IDs informados.
    /// </summary>
    [Fact]
    public async Task Handle_WhenNoCategoriesFound_ShouldThrowBusinessException()
    {
        // Arrange
        var command = BuildCommand();

        _gameRepositoryMock
            .Setup(r => r.GameAlreadyExistsByTitle(command.Title, command.Developer))
            .ReturnsAsync(false);
        _categoryRepositoryMock
            .Setup(r => r.GetByIdsAsync(command.Categories))
            .ReturnsAsync([]);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*categoria*");

        _gameRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Game>()), Times.Never);
        _gameRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que false é retornado quando SaveChanges falha após a criação do jogo.
    /// </summary>
    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldReturnFalse()
    {
        // Arrange
        var command = BuildCommand();
        var categories = CreateCategories();

        _gameRepositoryMock
            .Setup(r => r.GameAlreadyExistsByTitle(command.Title, command.Developer))
            .ReturnsAsync(false);
        _categoryRepositoryMock
            .Setup(r => r.GetByIdsAsync(command.Categories))
            .ReturnsAsync(categories);
        _gameRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);
        _gameRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Garante que o jogo é criado com múltiplas categorias quando todas
    /// são encontradas no repositório.
    /// </summary>
    [Fact]
    public async Task Handle_WhenMultipleCategoriesProvided_ShouldCreateGameWithAllCategories()
    {
        // Arrange
        var categoryIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var command = BuildCommand(categories: categoryIds);
        var categories = CreateCategories(count: 3);

        _gameRepositoryMock
            .Setup(r => r.GameAlreadyExistsByTitle(command.Title, command.Developer))
            .ReturnsAsync(false);
        _categoryRepositoryMock
            .Setup(r => r.GetByIdsAsync(command.Categories))
            .ReturnsAsync(categories);
        _gameRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);
        _gameRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _gameRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Game>(g => g.Categories.Count == 3)), Times.Once);
    }

    /// <summary>
    /// Garante que o título e desenvolvedor informados no comando são
    /// passados corretamente para a verificação de duplicidade.
    /// </summary>
    [Fact]
    public async Task Handle_Always_ShouldCheckDuplicityWithCorrectTitleAndDeveloper()
    {
        // Arrange
        var command = BuildCommand(title: "Elden Ring", developer: "FromSoftware");

        _gameRepositoryMock
            .Setup(r => r.GameAlreadyExistsByTitle(command.Title, command.Developer))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();

        _gameRepositoryMock.Verify(r => r.GameAlreadyExistsByTitle("Elden Ring", "FromSoftware"), Times.Once);
    }
}
