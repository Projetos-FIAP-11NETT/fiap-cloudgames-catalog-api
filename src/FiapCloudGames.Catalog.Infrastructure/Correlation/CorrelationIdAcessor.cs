using FiapCloudGames.Catalog.Shared.Abstractions;
using Microsoft.AspNetCore.Http;

namespace FiapCloudGames.Catalog.Infrastructure.Correlation;

public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid CorrelationId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.Items["X-Correlation-Id"]?.ToString(), out var correlationId)
            ? correlationId
            : Guid.NewGuid();
}