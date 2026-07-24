namespace AssetFlow.Application.Contracts.Sites;

public record SiteDto(
    Guid Id,
    string Name,
    string Code,
    string? Location,
    int AssetCount,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
