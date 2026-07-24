namespace AssetFlow.Application.Contracts.Auth;

public record RegisterRequest(string Email, string FullName, string Password);
