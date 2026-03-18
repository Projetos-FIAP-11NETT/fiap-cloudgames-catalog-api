namespace FiapCloudGames.Catalog.Shared.Abstractions;

public interface ICorrelationIdAccessor
{
    Guid CorrelationId { get; }
}