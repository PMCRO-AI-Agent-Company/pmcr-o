namespace ProjectName.Application.Abstractions;

using ProjectName.Domain;

public interface IGenericRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}

public interface IUnitOfWork
{
    IGenericRepository<TEntity, TId> Repository<TEntity, TId>()
        where TEntity : Entity<TId>
        where TId : notnull;

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
