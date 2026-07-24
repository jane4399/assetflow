namespace AssetFlow.Domain.Entities;

/// <summary>
/// Coarse-grained authorization role. Mapped to the <c>role</c> JWT claim and
/// enforced through ASP.NET Core authorization policies.
/// </summary>
public enum UserRole
{
    Admin = 1,
    Technician = 2
}
