using FiapCloudGames.Catalog.Api.Constants;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.MongoDb;
using FiapCloudGames.Catalog.Domain.Entities;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace FiapCloudGames.Catalog.Api.Middlewares;

public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger,
    IWebHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;
    private readonly IWebHostEnvironment _env = env;

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task InvokeAsync(HttpContext context, IRequestLogRepository requestLogRepository)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await HandleExceptionAsync(context, ex, requestLogRepository, stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        IRequestLogRepository requestLogRepository,
        long elapsedMilliseconds)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogError(exception, "Nao foi possivel sobrescrever a resposta porque ela ja foi iniciada.");
            return;
        }

        context.Response.Clear();
        context.Response.ContentType = "application/json";

        int statusCode;
        object? errors = null;
        string message;

        var baseResponse = new
        {
            timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case FluentValidation.ValidationException fve:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Erro de validação.";
                errors = new
                {
                    validationErrors = fve.Errors.Select(e => new
                    {
                        property = e.PropertyName,
                        message = e.ErrorMessage
                    })
                };
                break;

            case Domain.Exceptions.DomainException de:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = de.Message;
                break;

            case Domain.Exceptions.NotFoundException nf:
                statusCode = (int)HttpStatusCode.NotFound;
                message = nf.Message;
                break;

            case Domain.Exceptions.BusinessException be:
                statusCode = (int)HttpStatusCode.UnprocessableEntity;
                message = be.Message;
                break;

            case KeyNotFoundException _:
                statusCode = (int)HttpStatusCode.NotFound;
                message = "Recurso não encontrado.";
                break;

            case UnauthorizedAccessException ua:
                statusCode = StatusCodes.Status401Unauthorized;
                message = string.IsNullOrEmpty(ua.Message) ? "Acesso não autorizado." : ua.Message;
                break;

            case ExternalException _:
                statusCode = StatusCodes.Status502BadGateway;
                message = "Ocorreu um erro com serviço externo.";
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "Ocorreu um erro interno no servidor.";
                break;
        }

        context.Response.StatusCode = statusCode;

        var correlationId = GetOrCreateCorrelationId(context);

        _logger.LogError(
            exception,
            "[catalog-service] CorrelationId: {CorrelationId} | Erro na requisicao {Method} {Path} | StatusCode: {StatusCode} {Elapsed}ms",
            correlationId,
            context.Request.Method,
            context.Request.Path,
            statusCode,
            elapsedMilliseconds);

        var requestLog = new RequestLog
        {
            CorrelationId = correlationId,
            Method = context.Request.Method,
            Path = context.Request.Path,
            StatusCode = statusCode,
            ElapsedMilliseconds = elapsedMilliseconds,
            UserId = context.User?.Identity?.Name,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await requestLogRepository.InsertAsync(requestLog, context.RequestAborted);
        }
        catch (Exception logException)
        {
            _logger.LogError(logException, "Erro ao persistir log da excecao no MongoDB.");
        }

        object payload;

        if (_env.IsDevelopment())
        {
            payload = new
            {
                status = statusCode,
                message,
                errors,
                baseResponse,
                exceptionType = exception.GetType().Name,
                stackTrace = exception.StackTrace,
                innerException = exception.InnerException?.Message
            };
        }
        else
        {
            payload = new
            {
                status = statusCode,
                message,
                errors,
                baseResponse
            };
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, s_jsonOptions));
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
