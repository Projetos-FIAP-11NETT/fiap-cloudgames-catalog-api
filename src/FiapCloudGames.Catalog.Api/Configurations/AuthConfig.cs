using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FiapCloudGames.Catalog.Api.Configurations;

public static class AuthConfig
{
    public static void AddAuthConfig(this IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = "https://securetoken.google.com/fiapcloudgames-eaced";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "https://securetoken.google.com/fiapcloudgames-eaced",

                    ValidateAudience = true,
                    ValidAudience = "fiapcloudgames-eaced",

                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    //RoleClaimType = "roles"
                };
            });
    }
}