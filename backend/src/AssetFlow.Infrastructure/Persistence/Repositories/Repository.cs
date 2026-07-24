using AssetFlow.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic write-side repository over the shared <see cref="AssetFlowDbContext"/>.
/// Persistence is deferred to <see cref="IUnitOfWork.SaveChangesAsync"/> so a
/// service can batch several repository operations into one transaction.
/// </summary>
public class Repository<T> : IRepository<T>
    where T : class
{
    protected AssetFlowDbContext Context { get; }

    protected DbSet<T> Set => Context.Set<T>();

    public Repository(AssetFlowDbContext context)
    {
        Context = context;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Set.FindAsync(new object?[] { id }, cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await Set.AddAsync(entity, cancellationToken);

    public virtual void Update(T entity) => Set.Update(entity);

    public virtual void Remove(T entity) => Set.Remove(entity);
}
