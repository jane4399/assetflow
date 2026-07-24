namespace AssetFlow.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested aggregate does not exist. Mapped to HTTP 404 by the
/// global exception middleware.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"{name} with key '{key}' was not found.")
    {
    }
}
