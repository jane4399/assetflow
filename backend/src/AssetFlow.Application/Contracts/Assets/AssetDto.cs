namespace AssetFlow.Application.Contracts.Assets;

public record AssetDto(
    Guid Id,
    string Name,
    string Tag,
    string Status,
    Guid SiteId,
    string SiteName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
