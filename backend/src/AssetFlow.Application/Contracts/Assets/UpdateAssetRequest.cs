using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Contracts.Assets;

public record UpdateAssetRequest(string Name, string Tag, AssetStatus Status, Guid SiteId);
