using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Abstractions;

/// <summary>A freshly minted access token and its absolute UTC expiry.</summary>
public readonly record struct JwtToken(string AccessToken, DateTime ExpiresAtUtc);

/// <summary>
/// Issues signed JWT access tokens carrying the user's identity and role claims.
/// </summary>
public interface IJwtTokenService
{
    JwtToken CreateToken(User user);
}
