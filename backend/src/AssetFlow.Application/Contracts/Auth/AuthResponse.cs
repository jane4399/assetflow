namespace AssetFlow.Application.Contracts.Auth;

/// <summary>Returned by register/login. The bearer token authenticates subsequent calls.</summary>
public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, UserDto User);
