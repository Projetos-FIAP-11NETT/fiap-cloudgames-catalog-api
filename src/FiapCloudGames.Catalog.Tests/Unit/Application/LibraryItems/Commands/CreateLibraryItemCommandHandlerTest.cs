using FiapCloudGames.Catalog.Application.Features.LibraryItemFeature.Commands.CreateLibraryItem;
using FiapCloudGames.Catalog.Domain.Contracts.Publishers;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;
using MongoDb = FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.LibraryItems.Commands;

/// <summary>
/// Testes unitários do CreateLibraryItemCommandHandler, responsável por adicionar
/// um jogo à biblioteca do usuário validando duplicidade, existência do jogo,
/// persistência e notificação por e-mail.
/// </summary>
public class CreateLibraryItemCommandHandlerTest
{
    private readonly Mock<ILibraryItemRepository> _libraryItemRepositoryMock = new();
    private readonly Mock<IGameRepository> _gameRepositoryMock = new();
    private readonly Mock<MongoDb.ILibraryItemRepository> _mongoLibraryItemRepositoryMock = new();
    private readonly Mock<IEmailNotificationPublisher> _emailNotificationPublisherMock = new();
    private readonly CreateLibraryItemCommandHandler _handler;

    public CreateLibraryItemCommandHandlerTest()
    {
        _handler = new CreateLibraryItemCommandHandler(
            _libraryItemRepositoryMock.Object,
            _gameRepositoryMock.Object,
            _mongoLibraryItemRepositoryMock.Object,
            _emailNotificationPublisherMock.Object);
    }

    /// <summary>
    /// Cria um Game válido para uso nos testes.
    /// </summary>
    private static Game CreateGame() =>
        new("Game Title", "Descrição válida do jogo para testes.", new DateTime(2020, 1, 1),
            "Developer Studio", 59.90m, [new Category("RPG")]);

    /// <summary>
    /// Cria um CreateLibraryItemCommand com valores padrão.
    /// </summary>
    private static CreateLibraryItemCommand BuildCommand(Guid? userId = null, Guid? gameId = null) =>
        new(
            userId ?? Guid.NewGuid(),
            gameId ?? Guid.NewGuid(),
            OrderId: 1,
            Email: "user@test.com"
        );

    /// <summary>
    /// Garante que, quando todos os dados são válidos e o item ainda não existe,
    /// o item é salvo no Postgres, inserido no MongoDB e o e-mail é publicado,
    /// retornando true.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValid_ShouldPersistInPostgresAndMongoAndPublishEmail()
    {
        // Arrange
        var game = CreateGame();
        var command = BuildCommand(gameId: game.Id);

        _libraryItemRepositoryMock.Setup(r => r.ExistsAsync(command.UserId, command.GameId)).ReturnsAsync(false);
        _gameRepositoryMock.Setup(r => r.GetByIdAsync(command.GameId)).ReturnsAsync(game);
        _libraryItemRepositoryMock.Setup(r => r.AddAsync(It.IsAny<LibraryItem>())).Returns(Task.CompletedTask);
        _libraryItemRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mongoLibraryItemRepositoryMock
            .Setup(r => r.InsertAsync(It.IsAny<LibraryItem>(), game.Title, game.Price))
            .Returns(Task.CompletedTask);
        _emailNotificationPublisherMock
            .Setup(p => p.PublishAsync(command.Email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _libraryItemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<LibraryItem>()), Times.Once);
        _libraryItemRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mongoLibraryItemRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<LibraryItem>(), game.Title, game.Price), Times.Once);
        _emailNotificationPublisherMock.Verify(p => p.PublishAsync(
            command.Email,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que false é retornado imediatamente quando o item já existe
    /// na biblioteca do usuário, sem persistir ou publicar nada.
    /// </summary>
    [Fact]
    public async Task Handle_WhenLibraryItemAlreadyExists_ShouldReturnFalseWithoutSideEffects()
    {
        // Arrange
        var command = BuildCommand();

        _libraryItemRepositoryMock.Setup(r => r.ExistsAsync(command.UserId, command.GameId)).ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        _gameRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _libraryItemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<LibraryItem>()), Times.Never);
        _libraryItemRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mongoLibraryItemRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<LibraryItem>(), It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        _emailNotificationPublisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
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

        _libraryItemRepositoryMock.Setup(r => r.ExistsAsync(command.UserId, command.GameId)).ReturnsAsync(false);
        _gameRepositoryMock.Setup(r => r.GetByIdAsync(command.GameId)).ReturnsAsync((Game?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage($"*{command.GameId}*");

        _libraryItemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<LibraryItem>()), Times.Never);
        _mongoLibraryItemRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<LibraryItem>(), It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        _emailNotificationPublisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que quando SaveChanges retorna false, o MongoDB e o e-mail
    /// não são acionados.
    /// </summary>
    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldNotInsertInMongoOrPublishEmail()
    {
        // Arrange
        var game = CreateGame();
        var command = BuildCommand(gameId: game.Id);

        _libraryItemRepositoryMock.Setup(r => r.ExistsAsync(command.UserId, command.GameId)).ReturnsAsync(false);
        _gameRepositoryMock.Setup(r => r.GetByIdAsync(command.GameId)).ReturnsAsync(game);
        _libraryItemRepositoryMock.Setup(r => r.AddAsync(It.IsAny<LibraryItem>())).Returns(Task.CompletedTask);
        _libraryItemRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        _mongoLibraryItemRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<LibraryItem>(), It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        _emailNotificationPublisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que o e-mail de notificação contém o título do jogo no corpo da mensagem.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValid_ShouldPublishEmailWithGameTitleInBody()
    {
        // Arrange
        var game = CreateGame();
        var command = BuildCommand(gameId: game.Id);

        _libraryItemRepositoryMock.Setup(r => r.ExistsAsync(command.UserId, command.GameId)).ReturnsAsync(false);
        _gameRepositoryMock.Setup(r => r.GetByIdAsync(command.GameId)).ReturnsAsync(game);
        _libraryItemRepositoryMock.Setup(r => r.AddAsync(It.IsAny<LibraryItem>())).Returns(Task.CompletedTask);
        _libraryItemRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mongoLibraryItemRepositoryMock.Setup(r => r.InsertAsync(It.IsAny<LibraryItem>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(Task.CompletedTask);
        _emailNotificationPublisherMock.Setup(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _emailNotificationPublisherMock.Verify(p => p.PublishAsync(
            command.Email,
            It.IsAny<string>(),
            It.Is<string>(body => body.Contains(game.Title)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
