using System.Diagnostics;
using FiapCloudGames.Catalog.Observability.Abstractions;
using FiapCloudGames.Catalog.Shared.Abstractions;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using LogContext = Serilog.Context.LogContext;

namespace FiapCloudGames.Catalog.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>
    (
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICorrelationIdAccessor correlation,
        IObservabilityService observabilityService
    )
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = correlation.CorrelationId;
        var stopwatch = Stopwatch.StartNew();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            observabilityService.AddCustomAttribute("CorrelationId", correlationId);
            observabilityService.AddCustomAttribute("RequestName", typeof(TRequest).Name);
            observabilityService.AddCustomAttribute("request.payload", request);
        }
            // REQUEST
        logger.LogInformation(
            "[catalog-service] CorrelationId {CorrelationId} | Iniciando {RequestName} Parametros: {request.payload}",
            correlationId,
            requestName,
            request                
        );

        try
        {
            var response = await next();

            observabilityService.AddCustomAttribute("response.payload", response!);

            stopwatch.Stop();

            // RESPONSE
            logger.LogInformation(
                "[catalog-service] CorrelationId {CorrelationId} | Finalizando {RequestName} em {Elapsed}ms Response: {response.payload}",
                correlationId,
                requestName,
                stopwatch.ElapsedMilliseconds,
                response
            );

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "[catalog-service] CorrelationId {CorrelationId} | Erro em {RequestName} em {Elapsed}ms",
                correlationId,
                requestName,
                stopwatch.ElapsedMilliseconds
            );

            throw;
        }
    }
}