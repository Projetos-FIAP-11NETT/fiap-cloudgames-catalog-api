namespace FiapCloudGames.Catalog.Domain.Exceptions;

public class ExternalException(string serviceName, string message) : Exception(message)
{
    public string ServiceName { get; } = serviceName;
}