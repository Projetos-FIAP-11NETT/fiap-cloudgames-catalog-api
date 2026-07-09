using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Infrastructure.Data.Relational;
using FiapCloudGames.Catalog.Infrastructure.Repositories.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapCloudGames.Catalog.Infrastructure.Configurations;

public static class PostgresConfig
{
    public static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ILibraryItemRepository, LibraryItemRepository>();

        return services;
    }
}