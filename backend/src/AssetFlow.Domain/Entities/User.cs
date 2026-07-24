using AssetFlow.Domain.Common;

namespace AssetFlow.Domain.Entities;

/// <summary>
/// An authenticated account. Doubles as the "technician" that work orders are
/// assigned to. Passwords are never stored in the clear; <see cref="PasswordHash"/>
/// holds a PBKDF2 digest encoded as <c>iterations.salt.hash</c>.
/// </summary>
public class User : AuditableEntity
{
    public required string Email { get; set; }

    public required string FullName { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; } = UserRole.Technician;

    public ICollection<WorkOrder> AssignedWorkOrders { get; set; } = new List<WorkOrder>();
}
