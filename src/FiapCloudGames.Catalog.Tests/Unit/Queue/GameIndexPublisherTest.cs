using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Shared.Abstractions;
using FiapCloudGames.Queue.Configurations.Sqs;
using FiapCloudGames.Queue.Contracts;
using FiapCloudGames.Queue.Publishers;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Queue;

/// <summary>
/// Testes unitários do <see cref="GameIndexPublisher"/>, responsável por publicar
/// os eventos de indexação (<see cref="IGameUpserted"/> e <see cref="IGameDeleted"/>)
/// que mantêm o índice do Elasticsearch sincronizado com o banco relacional.
/// </summary>
public class GameIndexPublisherTest
{
    private readonly Mock<ISqsPublish> _busMock = new();
    private readonly Mock<ILogger<GameIndexPublisher>> _loggerMock = new();
    private readonly Mock<ICorrelationIdAccessor> _correlationMock = new();
    private readonly GameIndexPublisher _publisher;
    private readonly Guid _correlationId = Guid.NewGuid();

    public GameIndexPublisherTest()
    {
        _correlationMock.Setup(c => c.CorrelationId).Returns(_correlationId);
        _publisher = new GameIndexPublisher(_busMock.Object, _loggerMock.Object, _correlationMock.Object);
    }

    /// <summary>
    /// Cria um <see cref="Game"/> válido para os testes.
    /// </summary>
    private static Game BuildGame() =>
        new(
            "Projeto final agora vai, ai sim tá funcionando.zip",
            "ahsuahsuahsuah",
            new DateTime(2015, 5, 19),
            "Pedro",
            99.90m,
            new List<Category> { new("RPG"), new("Aventura") });

    /// <summary>
    /// Configura o mock do bus para a publicação de um evento de tipo T.
    /// </summary>
    private void SetupBusPublish<T>(Action<object>? onPublish = null) where T : class
    {
        _busMock
            .Setup(b => b.Publish<T>(
                It.IsAny<object>(),
                It.IsAny<IPipe<PublishContext<T>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<object, IPipe<PublishContext<T>>, CancellationToken>(
                (msg, _, _) => onPublish?.Invoke(msg))
            .Returns(Task.CompletedTask);
    }

    /// <summary>Lê uma propriedade de um objeto anônimo via reflexão.</summary>
    private static T GetProp<T>(object obj, string name) =>
        (T)obj.GetType().GetProperty(name)!.GetValue(obj)!;

    /// <summary>
    /// Garante que PublishUpsertedAsync publica IGameUpserted com todos os campos
    /// do jogo, incluindo os ids das categorias.
    /// </summary>
    [Fact]
    public async Task PublishUpsertedAsync_ShouldPublishIGameUpsertedWithCorrectData()
    {
        // Arrange
        var game = BuildGame();
        var expectedCategoryIds = game.Categories.Select(c => c.Id).ToList();

        object? capturedMsg = null;
        SetupBusPublish<IGameUpserted>(msg => capturedMsg = msg);

        // Act
        await _publisher.PublishUpsertedAsync(game);

        // Assert
        _busMock.Verify(b => b.Publish<IGameUpserted>(
            It.IsAny<object>(),
            It.IsAny<IPipe<PublishContext<IGameUpserted>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        capturedMsg.Should().NotBeNull();
        GetProp<Guid>(capturedMsg!, "Id").Should().Be(game.Id);
        GetProp<string>(capturedMsg!, "Title").Should().Be(game.Title);
        GetProp<string>(capturedMsg!, "Description").Should().Be(game.Description);
        GetProp<DateTime>(capturedMsg!, "ReleaseDate").Should().Be(game.ReleaseDate);
        GetProp<string>(capturedMsg!, "Developer").Should().Be(game.Developer);
        GetProp<decimal>(capturedMsg!, "Price").Should().Be(game.Price);
        GetProp<IEnumerable<Guid>>(capturedMsg!, "Categories").Should().BeEquivalentTo(expectedCategoryIds);
    }

    /// <summary>
    /// Garante que PublishDeletedAsync publica IGameDeleted com o id do jogo.
    /// </summary>
    [Fact]
    public async Task PublishDeletedAsync_ShouldPublishIGameDeletedWithGameId()
    {
        // Arrange
        var gameId = Guid.NewGuid();

        object? capturedMsg = null;
        SetupBusPublish<IGameDeleted>(msg => capturedMsg = msg);

        // Act
        await _publisher.PublishDeletedAsync(gameId);

        // Assert
        _busMock.Verify(b => b.Publish<IGameDeleted>(
            It.IsAny<object>(),
            It.IsAny<IPipe<PublishContext<IGameDeleted>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        capturedMsg.Should().NotBeNull();
        GetProp<Guid>(capturedMsg!, "GameId").Should().Be(gameId);
    }

    /// <summary>
    /// Confirma que o CancellationToken é repassado ao endpoint de publicação.
    /// </summary>
    [Fact]
    public async Task PublishUpsertedAsync_ShouldForwardCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        CancellationToken capturedToken = default;

        _busMock
            .Setup(b => b.Publish<IGameUpserted>(
                It.IsAny<object>(),
                It.IsAny<IPipe<PublishContext<IGameUpserted>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<object, IPipe<PublishContext<IGameUpserted>>, CancellationToken>(
                (_, _, ct) => capturedToken = ct)
            .Returns(Task.CompletedTask);

        // Act
        await _publisher.PublishUpsertedAsync(BuildGame(), token);

        // Assert
        capturedToken.Should().Be(token);
    }

    /// <summary>
    /// Garante que uma exceção lançada pelo bus é propagada ao chamador.
    /// </summary>
    [Fact]
    public async Task PublishUpsertedAsync_WhenBusThrows_ShouldPropagateException()
    {
        // Arrange
        _busMock
            .Setup(b => b.Publish<IGameUpserted>(
                It.IsAny<object>(),
                It.IsAny<IPipe<PublishContext<IGameUpserted>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SQS unavailable"));

        // Act
        var act = async () => await _publisher.PublishUpsertedAsync(BuildGame());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("SQS unavailable");
    }

    /// <summary>
    /// Verifica que o CorrelationId do accessor é atribuído ao contexto de publicação.
    /// </summary>
    [Fact]
    public async Task PublishUpsertedAsync_ShouldSetCorrelationIdOnContext()
    {
        // Arrange
        IPipe<PublishContext<IGameUpserted>>? capturedPipe = null;

        _busMock
            .Setup(b => b.Publish<IGameUpserted>(
                It.IsAny<object>(),
                It.IsAny<IPipe<PublishContext<IGameUpserted>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<object, IPipe<PublishContext<IGameUpserted>>, CancellationToken>(
                (_, pipe, _) => capturedPipe = pipe)
            .Returns(Task.CompletedTask);

        // Act
        await _publisher.PublishUpsertedAsync(BuildGame());

        // Assert
        capturedPipe.Should().NotBeNull();

        var contextMock = new Mock<PublishContext<IGameUpserted>>();
        await capturedPipe!.Send(contextMock.Object);

        contextMock.VerifySet(c => c.CorrelationId = _correlationId, Times.Once);
    }
}
