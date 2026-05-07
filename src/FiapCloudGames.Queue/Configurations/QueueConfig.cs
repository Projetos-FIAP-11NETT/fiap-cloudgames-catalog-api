using FiapCloudGames.Catalog.Domain.Contracts.Publishers;
using FiapCloudGames.Queue.Configurations.MassTransit;
using FiapCloudGames.Queue.Configurations.Sqs;
using FiapCloudGames.Queue.Publishers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapCloudGames.Queue.Configurations;

public static class QueueConfig
{
    public static IServiceCollection AddQueueConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MassTransitSettings>(configuration.GetSection(nameof(MassTransitSettings)));
        services.Configure<SqsSettings>(configuration.GetSection(nameof(SqsSettings)));

        services.AddScoped<IOrderPlacedPublisher, OrderPlacedPublisher>();
        
        services.RegisterSqsStartup();

        return services;
    }
}