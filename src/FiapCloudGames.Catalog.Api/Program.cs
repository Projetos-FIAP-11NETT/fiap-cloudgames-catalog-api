using FiapCloudGames.Catalog.Api.Configurations;
using FiapCloudGames.Catalog.Api.Middlewares;
using FiapCloudGames.Catalog.Application.CategoryFeature.Commands.CreateCategory;
using FiapCloudGames.Catalog.Infrastructure.Configurations;
using FiapCloudGames.Catalog.Infrastructure.Data;
using FiapCloudGames.Queue.Configurations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(CreateCategoryCommand).Assembly);
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient();

builder.Services.AddAuthorization();

builder.Services.AddAuthConfig();

builder.Services.AddHttpContextAccessor();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddQueueConfig(builder.Configuration);

builder.Services.AddSwaggerConfig();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwaggerConfig();

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

