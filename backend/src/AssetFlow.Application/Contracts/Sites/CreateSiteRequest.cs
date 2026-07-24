namespace AssetFlow.Application.Contracts.Sites;

public record CreateSiteRequest(string Name, string Code, string? Location);
