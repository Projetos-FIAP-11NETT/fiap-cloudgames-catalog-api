using FiapCloudGames.Catalog.Domain.Contracts.Repositories.NoSql;
using FiapCloudGames.Catalog.Infrastructure.Data.NoSql.Mongodb;
using FiapCloudGames.Catalog.Infrastructure.Repositories.NoSql.MongoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapCloudGames.Catalog.Infrastructure.Configurations.NoSql;

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