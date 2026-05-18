using Amazon.SQS;
using Amazon.SQS.Model;
using FiapCloudGames.Catalog.Domain.Contracts.Publishers;
using FiapCloudGames.Catalog.Shared.Abstractions;
using FiapCloudGames.Queue.Configurations.Sqs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FiapCloudGames.Queue.Publishers;

public class EmailNotificationPublisher(
        IAmazonSQS sqsClient,
        IOptions<SqsSettings> sqsSettings,
        ILogger<EmailNotificationPublisher> logger,
        ICorrelationIdAccessor correlation
    )
    : IEmailNotificationPublisher
{
    public async Task PublishAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[catalog-service] Publishing email notification to SQS: To={To}, Subject={Subject}",
            to, subject);

        var messageBody = JsonSerializer.Serialize(new
        {
            To = to,
            Subject = subject,
            Body = body,
            CorrelationId = correlation.CorrelationId.ToString()
        });

        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = sqsSettings.Value.EmailQueueUrl,
            MessageBody = messageBody
        },
        cancellationToken);
    }
}