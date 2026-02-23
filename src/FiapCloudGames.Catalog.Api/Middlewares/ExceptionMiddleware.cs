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

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            // Não é possível sobrescrever a resposta; apenas logar
            // O caller já terá  log feito no middleware acima
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
}