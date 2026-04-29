using FiapCloudGames.Catalog.Api.Configurations;
using FiapCloudGames.Catalog.Api.Middlewares;
using FiapCloudGames.Catalog.Application.Configurations;
using FiapCloudGames.Catalog.Infrastructure.Configurations;
using FiapCloudGames.Catalog.Infrastructure.Correlation;
using FiapCloudGames.Catalog.Infrastructure.Data.Relational;
using FiapCloudGames.Catalog.Observability.Configurations;
using FiapCloudGames.Catalog.Observability.Middleware;
using FiapCloudGames.Catalog.Shared.Abstractions;
using FiapCloudGames.Queue.Configurations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddApplication(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient();

builder.Services.AddAuthorization();

builder.Services.AddAuthConfig();

builder.Services.AddHttpContextAccessor();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddQueueConfig(builder.Configuration);

builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

builder.Services.AddObservabilityConfig();

builder.Services.AddSwaggerConfig();

var app = builder.Build();

app.MigrateDatabase();

// Configure the HTTP request pipeline.
app.UseSwaggerConfig();

app.UseMiddleware<ExceptionMiddleware>();

app.UseMiddleware<ObservabilityMiddleware>();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

