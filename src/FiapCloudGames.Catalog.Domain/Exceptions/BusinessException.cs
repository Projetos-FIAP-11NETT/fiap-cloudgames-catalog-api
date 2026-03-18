namespace FiapCloudGames.Catalog.Domain.Exceptions;

public class BusinessException : Exception
{
    public object Errors { get; }

    public BusinessException()
    {
    }

    public BusinessException(string message) : base(message)
    {
    }

    public BusinessException(string message, object errors) : base(message)
    {
        Errors = errors;
    }

    public BusinessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}