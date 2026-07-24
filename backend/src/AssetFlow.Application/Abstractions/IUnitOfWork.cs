namespace AssetFlow.Application.Abstractions;

/// <summary>
/// Transaction boundary. Repositories stage changes against the shared
/// <c>DbContext</c>; a service persists them atomically with a single call to
/// <see cref="SaveChangesAsync"/>.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
