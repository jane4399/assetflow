namespace AssetFlow.Infrastructure.Security;

/// <summary>
/// Strongly-typed binding of the "Jwt" configuration section. In production the
/// <see cref="Key"/> is supplied from a secret store (Azure Key Vault / user
/// secrets), never checked in.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>Symmetric signing key (HMAC-SHA256). At least 32 bytes.</summary>
    public string Key { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 60;
}
