namespace AssetFlow.Application.Contracts.Auth;

/// <summary>Public projection of a user account (never carries the password hash).</summary>
public record UserDto(Guid Id, string Email, string FullName, string Role);
