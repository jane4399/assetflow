namespace AssetFlow.Application.Common.Exceptions;

/// <summary>
/// Thrown when a request violates a uniqueness or business invariant (e.g. a
/// duplicate site code or e-mail). Mapped to HTTP 409 by the global exception
/// middleware.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
