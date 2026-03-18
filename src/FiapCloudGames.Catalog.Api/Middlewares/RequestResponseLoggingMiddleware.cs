using System.Diagnostics;
using FiapCloudGames.Catalog.Api.Constants;
using FiapCloudGames.Catalog.Observability.Abstractions;
using LogContext = Serilog.Context.LogContext;

namespace FiapCloudGames.Catalog.Api.Middlewares;

public sealed class RequestResponseLoggingMiddleware(RequestDelegate _next, ILogger<RequestResponseLoggingMiddleware> _logger)
{
    private const string MessageRequest = "[catalog-service] CorrelationId: {CorrelationId} | Inicio da Requisicao {Method} {Path}";
    private const string MessageResponse = "[catalog-service] CorrelationId: {CorrelationId} | Final da Requisicao {Method} {Path} | StatusCode: {StatusCode} {Elapsed}ms";


    public async Task InvokeAsync(HttpContext context, IObservabilityService observabilityService)
    {
        // CorrelationId
        var correlationId = GetOrCreateCorrelationId(context);

        context.Items[ContextItems.CorrelationId] = correlationId;
        context.Items["X-Correlation-Id"] = correlationId;

        context.Response.Headers[HeaderNames.CorrelationId] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path
        }))
        {
            var stopwatch = Stopwatch.StartNew();

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                observabilityService.AddCustomAttribute("CorrelationId", correlationId);
                _logger.LogInformation(MessageRequest, correlationId, context.Request.Method, context.Request.Path);

                await _next(context);
            }
            stopwatch.Stop();

            _logger.LogInformation(MessageResponse, correlationId, context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderNames.CorrelationId, out var correlationId)
           && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId!;
        }

        var newCorrelationId = Guid.NewGuid().ToString();
        context.Request.Headers[HeaderNames.CorrelationId] = newCorrelationId;

        return newCorrelationId;
    }
}