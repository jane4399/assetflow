using AssetFlow.Application.Contracts.Assets;
using AssetFlow.Application.Contracts.Auth;
using AssetFlow.Application.Contracts.Sites;
using AssetFlow.Application.Contracts.WorkOrders;
using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Mapping;

/// <summary>
/// Explicit entity-to-DTO projections. Hand-written mapping (rather than a
/// convention-based mapper) keeps the shape of every API response obvious and
/// greppable, and avoids leaking navigation properties or the password hash.
/// </summary>
public static class MappingExtensions
{
    public static UserDto ToDto(this User user) =>
        new(user.Id, user.Email, user.FullName, user.Role.ToString());

    public static SiteDto ToDto(this Site site, int assetCount) =>
        new(
            site.Id,
            site.Name,
            site.Code,
            site.Location,
            assetCount,
            site.CreatedAtUtc,
            site.UpdatedAtUtc);

    public static AssetDto ToDto(this Asset asset) =>
        new(
            asset.Id,
            asset.Name,
            asset.Tag,
            asset.Status.ToString(),
            asset.SiteId,
            asset.Site?.Name ?? string.Empty,
            asset.CreatedAtUtc,
            asset.UpdatedAtUtc);

    public static WorkOrderDto ToDto(this WorkOrder workOrder) =>
        new(
            workOrder.Id,
            workOrder.Title,
            workOrder.Description,
            workOrder.Priority.ToString(),
            workOrder.Status.ToString(),
            workOrder.AssetId,
            workOrder.Asset?.Name ?? string.Empty,
            workOrder.AssignedTechnicianId,
            workOrder.AssignedTechnician?.FullName,
            workOrder.DueDate,
            workOrder.CreatedAtUtc,
            workOrder.UpdatedAtUtc);
}
