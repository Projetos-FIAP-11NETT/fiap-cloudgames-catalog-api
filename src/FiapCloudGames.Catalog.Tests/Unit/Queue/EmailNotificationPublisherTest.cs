using Amazon.SQS;
using Amazon.SQS.Model;
using FiapCloudGames.Catalog.Shared.Abstractions;
using FiapCloudGames.Queue.Configurations.Sqs;
using FiapCloudGames.Queue.Publishers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;

namespace FiapCloudGames.Catalog.Tests.Unit.Queue;

/// <summary>
/// Testes unitários do EmailNotificationPublisher, responsável por publicar
/// notificações de e-mail na fila SQS.
/// </summary>
public class EmailNotificationPublisherTest
{
    private readonly Mock<IAmazonSQS> _sqsClientMock = new();
    private readonly Mock<ILogger<EmailNotificationPublisher>> _loggerMock = new();
    private readonly Mock<ICorrelationIdAccessor> _correlationMock = new();
    private readonly Mock<IOptions<SqsSettings>> _sqsSettingsMock = new();
    private readonly EmailNotificationPublisher _publisher;

    private const string EmailQueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789/email-queue";

    public EmailNotificationPublisherTest()
    {
        _sqsSettingsMock.Setup(s => s.Value).Returns(new SqsSettings
        {
            EmailQueueUrl = EmailQueueUrl
        });

        _correlationMock.Setup(c => c.CorrelationId).Returns(Guid.NewGuid());

        _publisher = new EmailNotificationPublisher(
            _sqsClientMock.Object,
            _sqsSettingsMock.Object,
            _loggerMock.Object,
            _correlationMock.Object);
    }

    /// <summary>
    /// Garante que, ao publicar uma notificação de e-mail, o cliente SQS é chamado
    /// com a URL da fila correta e o corpo da mensagem contendo todos os campos esperados.
    /// </summary>
    [Fact]
    public async Task PublishAsync_WhenCalled_ShouldSendMessageToCorrectQueueUrl()
    {
        // Arrange
        const string to = "user@test.com";
        const string subject = "Confirmação de pedido";
        const string body = "Seu pedido foi confirmado.";

        _sqsClientMock
            .Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendMessageResponse());

        // Act
        await _publisher.PublishAsync(to, subject, body);

        // Assert
        _sqsClientMock.Verify(s => s.SendMessageAsync(
            It.Is<SendMessageRequest>(r => r.QueueUrl == EmailQueueUrl),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que o corpo da mensagem enviada ao SQS contém os campos
    /// To, Subject, Body e CorrelationId corretamente serializados.
    /// </summary>
    [Fact]
    public async Task PublishAsync_WhenCalled_ShouldSerializeAllFieldsInMessageBody()
    {
        // Arrange
        const string to = "user@test.com";
        const string subject = "Confirmação de pedido";
        const string body = "Seu pedido foi confirmado.";
        var correlationId = Guid.NewGuid();

        _correlationMock.Setup(c => c.CorrelationId).Returns(correlationId);

        SendMessageRequest? capturedRequest = null;
        _sqsClientMock
            .Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SendMessageRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new SendMessageResponse());

        // Act
        await _publisher.PublishAsync(to, subject, body);

        // Assert
        capturedRequest.Should().NotBeNull();

        using var doc = JsonDocument.Parse(capturedRequest!.MessageBody);
        var root = doc.RootElement;

        root.GetProperty("To").GetString().Should().Be(to);
        root.GetProperty("Subject").GetString().Should().Be(subject);
        root.GetProperty("Body").GetString().Should().Be(body);
        root.GetProperty("CorrelationId").GetString().Should().Be(correlationId.ToString());
    }

    /// <summary>
    /// Garante que o CorrelationId presente na mensagem corresponde ao valor
    /// retornado pelo ICorrelationIdAccessor
    /// </summary>
    [Fact]
    public async Task PublishAsync_WhenCalled_ShouldUseCorrelationIdFromAccessor()
    {
        // Arrange
        var expectedCorrelationId = Guid.NewGuid();
        _correlationMock.Setup(c => c.CorrelationId).Returns(expectedCorrelationId);

        SendMessageRequest? capturedRequest = null;
        _sqsClientMock
            .Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SendMessageRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new SendMessageResponse());

        // Act
        await _publisher.PublishAsync("a@b.com", "Assunto", "Corpo");

        // Assert
        using var doc = JsonDocument.Parse(capturedRequest!.MessageBody);
        doc.RootElement.GetProperty("CorrelationId").GetString()
            .Should().Be(expectedCorrelationId.ToString());
    }

    /// <summary>
    /// Garante que exceções lançadas pelo cliente SQS se propagam corretamente ao chamador.
    /// </summary>
    [Fact]
    public async Task PublishAsync_WhenSqsThrows_ShouldPropagateException()
    {
        // Arrange
        _sqsClientMock
            .Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonSQSException("SQS unavailable"));

        // Act
        var act = async () => await _publisher.PublishAsync("a@b.com", "Assunto", "Corpo");

        // Assert
        await act.Should().ThrowAsync<AmazonSQSException>()
            .WithMessage("SQS unavailable");
    }

    /// <summary>
    /// Garante que o CancellationToken fornecido é repassado ao cliente SQS.
    /// </summary>
    [Fact]
    public async Task PublishAsync_WhenCancellationTokenProvided_ShouldPassItToSqsClient()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _sqsClientMock
            .Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), token))
            .ReturnsAsync(new SendMessageResponse());

        // Act
        await _publisher.PublishAsync("a@b.com", "Assunto", "Corpo", token);

        // Assert
        _sqsClientMock.Verify(s => s.SendMessageAsync(
            It.IsAny<SendMessageRequest>(),
            token), Times.Once);
    }
}