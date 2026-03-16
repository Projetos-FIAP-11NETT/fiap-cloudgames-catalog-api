using FiapCloudGames.Catalog.Observability.Abstractions;
using Microsoft.AspNetCore.Http;

namespace FiapCloudGames.Catalog.Observability.Middleware;

public class ObservabilityMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, IObservabilityService obs)
    {
        obs.AddCustomAttribute("TraceId", context.TraceIdentifier);

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            obs.NoticeError(ex);
            throw;
        }
    }
}