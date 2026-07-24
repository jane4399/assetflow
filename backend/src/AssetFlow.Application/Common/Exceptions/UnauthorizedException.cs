namespace AssetFlow.Application.Common.Exceptions;

/// <summary>
/// Thrown when authentication fails (bad credentials). Mapped to HTTP 401 by the
/// global exception middleware. Distinct from authorization (403), which is
/// enforced declaratively by the framework's policy middleware.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
