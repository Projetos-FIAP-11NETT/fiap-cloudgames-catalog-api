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
/// Testes unitários do <see cref="OrderPlacedPublisher"/>, responsável por publicar
/// mensagens <see cref="IOrderPlaced"/> no tópico SNS via MassTransit.
/// </summary>
public class OrderPlacedPublisherTest
{
    private readonly Mock<ISqsPublish> _busMock;
    private readonly Mock<ILogger<OrderPlacedPublisher>> _loggerMock;
    private readonly Mock<ICorrelationIdAccessor> _correlationMock;
    private readonly OrderPlacedPublisher _publisher;
    private readonly Guid _correlationId = Guid.NewGuid();

    public OrderPlacedPublisherTest()
    {
        _busMock = new Mock<ISqsPublish>();
        _loggerMock = new Mock<ILogger<OrderPlacedPublisher>>();
        _correlationMock = new Mock<ICorrelationIdAccessor>();
        _correlationMock.Setup(c => c.CorrelationId).Returns(_correlationId);

        _publisher = new OrderPlacedPublisher(_busMock.Object, _loggerMock.Object, _correlationMock.Object);
    }

    /// <summary>
    /// Configura o mock do bus para o método de interface real do MassTransit (IPipe).
    /// </summary>
    private void SetupBusPublish(Action<object>? onPublish = null)
    {
        _busMock
            .Setup(b => b.Publish<IOrderPlaced>(
                It.IsAny<object>(),
                It.IsAny<IPipe<PublishContext<IOrderPlaced>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<object, IPipe<PublishContext<IOrderPlaced>>, CancellationToken>(
                (msg, _, _) => onPublish?.Invoke(msg))
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Lê uma propriedade de um objeto anônimo via reflexão.
    /// Necessário pois o publisher utiliza new { } na chamada ao endpoint.
    /// </summary>
    private static T GetProp<T>(object obj, string name) =>
        (T)obj.GetType().GetProperty(name)!.GetValue(obj)!;

    /// <summary>
    /// Garante que PublishAsync delega a publicação ao IPublishEndpoint com os dados corretos,
    /// confirmando que nenhuma transformação indevida ocorre nos valores antes do envio.
    /// </summary>
    [Fact]
    public async Task PublishAsync_ShouldPublishIOrderPlacedWithCorrectData()
    {
        // Arrange
        const int orderId = 10;
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        const decimal price = 199.90m;
        const string email = "gamer@test.com";
        const string name = "Cloud Gamer";

        object? capturedMsg = null;
        SetupBusPublish(msg => capturedMsg = msg);

        // Act
        await _publisher.PublishAsync(orderId, userId, gameId, price, email, name);

        // Assert
        _busMock.Verify(b => b.Publish<IOrderPlaced>(
            It.IsAny<object>(),
            It.IsAny<IPipe<PublishContext<IOrderPlaced>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        capturedMsg.Should().NotBeNull();
        GetProp<int>(capturedMsg!, "OrderId").Should().Be(orderId);
        GetProp<Guid>(capturedMsg!, "UserId").Should().Be(userId);
        GetProp<Guid>(capturedMsg!, "GameId").Should().Be(gameId);
        GetProp<decimal>(capturedMsg!, "Price").Should().Be(price);
        GetProp<string>(capturedMsg!, "Email").Should().Be(email);
        GetProp<string>(capturedMsg!, "Name").Should().Be(name);
    }

    /// <summary>
    /// Confirma que o CancellationToken é repassado ao endpoint de publicação,
    /// permitindo o cancelamento correto da operação em cenários de timeout ou shutdown.
    /// </summary>
    [Fact]
    public async Task PublishAsync_ShouldForwardCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        CancellationToken capturedToken = default;

        _busMock
            .Setup(b => b.Publish<IOrderPlaced>(
                It.IsAny<object>(),
                It.IsAny<IPipe<PublishContext<IOrderPlaced>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<object, IPipe<PublishContext<IOrderPlaced>>, CancellationToken>(
                (_, _, ct) => capturedToken = ct)
            .Returns(Task.CompletedTask);

        // Act
        await _publisher.PublishAsync(1, Guid.NewGuid(), Guid.NewGuid(), 10m, "a@b.com", "Test", token);

        // Assert
        capturedToken.Should().Be(token);
    }

    /// <summary>
    /// Garante que uma exceção lançada pelo endpoint de publicação é propagada ao chamador,
    /// permitindo que a camada superior decida como tratar o erro.
    /// </summary>
    [Fact]
    public async Task PublishAsync_WhenBusThrows_ShouldPropagateException()
    {
        // Arrange
        _busMock
            .Setup(b => b.Publish<IOrderPlaced>(
                It.IsAny<object>(),
                It.IsAny<IPipe<PublishContext<IOrderPlaced>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SQS unavailable"));

        // Act
        var act = async () => await _publisher.PublishAsync(99, Guid.NewGuid(), Guid.NewGuid(), 10m, "x@y.com", "Name");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("SQS unavailable");
    }

    /// <summary>
    /// Verifica que o LogInformation é chamado duas vezes (antes e após o envio),
    /// garantindo rastreabilidade da operação nos logs do serviço.
    /// </summary>
    [Fact]
    public async Task PublishAsync_WhenSucceeds_ShouldLogInformationTwice()
    {
        // Arrange
        const int orderId = 5;
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        SetupBusPublish();

        // Act
        await _publisher.PublishAsync(orderId, userId, gameId, 59.90m, "log@test.com", "Logger Test");

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.Contains(orderId.ToString()) &&
                    v.ToString()!.Contains(userId.ToString()) &&
                    v.ToString()!.Contains(gameId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }

    /// <summary>
    /// Verifica que o LogError é chamado quando o bus lança exceção,
    /// garantindo que falhas no envio sejam devidamente registradas.
    /// </summary>
    [Fact]
    public async Task PublishAsync_WhenBusThrows_ShouldLogError()
    {
        // Arrange
        const int orderId = 7;
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        _busMock
            .Setup(b => b.Publish<IOrderPlaced>(
                It.IsAny<object>(),
                It.IsAny<IPipe<PublishContext<IOrderPlaced>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SQS unavailable"));

        // Act
        var act = async () => await _publisher.PublishAsync(orderId, userId, gameId, 10m, "x@y.com", "Name");
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.Contains(orderId.ToString()) &&
                    v.ToString()!.Contains(userId.ToString()) &&
                    v.ToString()!.Contains(gameId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifica que o CorrelationId do accessor é atribuído ao contexto de publicação,
    /// confirmando que a propriedade é corretamente repassada ao pipe do MassTransit.
    /// </summary>
    [Fact]
    public async Task PublishAsync_ShouldAccessCorrelationIdFromAccessor()
    {
        // Arrange
        IPipe<PublishContext<IOrderPlaced>>? capturedPipe = null;

        _busMock
            .Setup(b => b.Publish<IOrderPlaced>(
                It.IsAny<object>(),
                It.IsAny<IPipe<PublishContext<IOrderPlaced>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<object, IPipe<PublishContext<IOrderPlaced>>, CancellationToken>(
                (_, pipe, _) => capturedPipe = pipe)
            .Returns(Task.CompletedTask);

        // Act
        await _publisher.PublishAsync(1, Guid.NewGuid(), Guid.NewGuid(), 10m, "a@b.com", "Test");

        // Assert
        capturedPipe.Should().NotBeNull();

        var contextMock = new Mock<PublishContext<IOrderPlaced>>();
        await capturedPipe!.Send(contextMock.Object);

        contextMock.VerifySet(c => c.CorrelationId = _correlationId, Times.Once);
    }
}
