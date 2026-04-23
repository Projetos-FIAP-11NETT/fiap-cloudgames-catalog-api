using FiapCloudGames.Catalog.Infrastructure.Configurations.NoSql;
using FiapCloudGames.Catalog.Infrastructure.Configurations.Relational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapCloudGames.Catalog.Infrastructure.Configurations;

public static class Infrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgres(configuration);
        services.AddMongoDb(configuration);

        return services;
    }
}   