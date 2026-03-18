using FiapCloudGames.Catalog.Observability.Abstractions;
using FiapCloudGames.Catalog.Observability.Providers.NewRelic;
using Microsoft.Extensions.DependencyInjection;

namespace FiapCloudGames.Catalog.Observability.Configurations;

public static class ObservabilityConfig
{
    public static IServiceCollection AddObservabilityConfig(this IServiceCollection services)
    {
        services.AddScoped<IObservabilityService, NewRelicObservabilityService>();
        services.AddScoped(typeof(NewRelicConsumeFilter<>));

        return services;
    }
}