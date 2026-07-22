using Elastic.Clients.Elasticsearch;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Elasticsearch;
using FiapCloudGames.Catalog.Infrastructure.Data.Elasticsearch;
using FiapCloudGames.Catalog.Infrastructure.Repositories.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapCloudGames.Catalog.Infrastructure.Configurations;

public static class ElasticsearchConfig
{
    public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ElasticsearchSettings>(configuration.GetSection("Elasticsearch"));

        services.AddSingleton(sp =>
        {
            var settings = configuration.GetSection("Elasticsearch").Get<ElasticsearchSettings>()
                ?? new ElasticsearchSettings();

            var clientSettings = new ElasticsearchClientSettings(new Uri(settings.Uri))
                .DefaultIndex(settings.IndexName);

            return new ElasticsearchClient(clientSettings);
        });

        services.AddScoped<IGameSearchRepository, GameSearchRepository>();

        return services;
    }
}
