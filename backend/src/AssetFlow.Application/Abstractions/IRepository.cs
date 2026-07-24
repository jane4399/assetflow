namespace AssetFlow.Application.Abstractions;

/// <summary>
/// Minimal write-side abstraction shared by every aggregate repository. Reads
/// that need filtering/sorting/paging live on the resource-specific interfaces
/// so that EF Core query composition stays inside the Infrastructure layer and
/// the Application layer never takes a dependency on Entity Framework.
/// </summary>
public interface IRepository<T>
    where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Remove(T entity);
}
