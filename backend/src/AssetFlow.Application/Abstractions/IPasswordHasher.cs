namespace AssetFlow.Application.Abstractions;

/// <summary>
/// Salted, iterated password hashing. The concrete implementation
/// (<c>Pbkdf2PasswordHasher</c>) lives in Infrastructure so the algorithm can be
/// swapped without touching application code.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Produces an opaque, self-describing hash string to persist.</summary>
    string Hash(string password);

    /// <summary>Constant-time verification of a candidate password against a stored hash.</summary>
    bool Verify(string password, string hash);
}
