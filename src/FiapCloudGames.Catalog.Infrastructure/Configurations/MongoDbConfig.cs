using FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;
using FiapCloudGames.Catalog.Infrastructure.Data.Mongodb;
using FiapCloudGames.Catalog.Infrastructure.Repositories.MongoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapCloudGames.Catalog.Infrastructure.Configurations;

public static class MongoDbConfig
{
    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        MongoDbSerializerConfig.Register();
        
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDB"));
        
        services.AddSingleton<MongoDbContext>();
        
        services.AddScoped<ILibraryItemRepository, LibraryItemRepository>();

        return services;
    }
}