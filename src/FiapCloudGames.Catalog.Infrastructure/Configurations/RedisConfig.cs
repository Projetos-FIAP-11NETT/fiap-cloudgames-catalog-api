using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;
using FiapCloudGames.Catalog.Infrastructure.Repositories.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FiapCloudGames.Catalog.Infrastructure.Configurations;

public static class RedisConfig
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("Redis")["ConnectionString"];

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(connectionString!));

        services.AddScoped<IRedisRepository, RedisRepository>();

        return services;
    }
}