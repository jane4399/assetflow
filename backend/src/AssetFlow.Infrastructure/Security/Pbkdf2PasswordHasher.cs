using System.Security.Cryptography;
using AssetFlow.Application.Abstractions;

namespace AssetFlow.Infrastructure.Security;

/// <summary>
/// PBKDF2 (RFC 2898) password hashing with a per-password random salt. The
/// stored value is self-describing — <c>iterations.saltBase64.hashBase64</c> —
/// so the work factor can be raised over time without breaking existing hashes.
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;         // 128-bit salt
    private const int KeySize = 32;          // 256-bit derived key
    private const int Iterations = 100_000;
    private const char Delimiter = '.';
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return string.Join(
            Delimiter,
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split(Delimiter);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] key;
        try
        {
            salt = Convert.FromBase64String(parts[1]);
            key = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var candidate = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, key.Length);

        // Constant-time comparison to avoid leaking information via timing.
        return CryptographicOperations.FixedTimeEquals(candidate, key);
    }
}
