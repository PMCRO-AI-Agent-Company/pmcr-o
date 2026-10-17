namespace ProjectName.Infrastructure;

using System.Collections.Concurrent;
using ProjectName.Application.Abstractions;
using ProjectName.Domain;

public sealed class InMemoryGenericRepository<TEntity, TId>(Action markChanged)
    : IGenericRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    private readonly ConcurrentDictionary<TId, TEntity> _items = new();

    public Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default) =>
        Task.FromResult(_items.TryGetValue(id, out var entity) ? entity : null);

    public Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<TEntity>>(_items.Values.ToArray());

    public Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        if (!_items.TryAdd(entity.Id, entity))
            throw new InvalidOperationException($"Entity already exists: {entity.Id}");
        markChanged();
        return Task.CompletedTask;
    }

    public void Update(TEntity entity)
    {
        _items[entity.Id] = entity;
        markChanged();
    }

    public void Remove(TEntity entity)
    {
        if (_items.TryRemove(entity.Id, out _)) markChanged();
    }
}

public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    private readonly ConcurrentDictionary<(Type Entity, Type Id), object> _repositories = new();
    private int _pendingChanges;

    public IGenericRepository<TEntity, TId> Repository<TEntity, TId>()
        where TEntity : Entity<TId>
        where TId : notnull =>
        (IGenericRepository<TEntity, TId>)_repositories.GetOrAdd(
            (typeof(TEntity), typeof(TId)),
            _ => new InMemoryGenericRepository<TEntity, TId>(() => Interlocked.Increment(ref _pendingChanges)));

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Task.FromResult(Interlocked.Exchange(ref _pendingChanges, 0));
}
