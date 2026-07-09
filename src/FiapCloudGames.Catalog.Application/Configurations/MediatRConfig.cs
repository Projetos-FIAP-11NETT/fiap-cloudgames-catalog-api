using FiapCloudGames.Catalog.Application.Behaviors;
using FiapCloudGames.Catalog.Application.Features.CategoryFeature.Commands.CreateCategory;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FiapCloudGames.Catalog.Application.Configurations;

public static class MediatRConfig
{
    public static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateCategoryCommand).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        return services;
    }
}