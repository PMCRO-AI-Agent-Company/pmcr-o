namespace ProjectName.Domain;

public abstract class Entity<TId> where TId : notnull
{
    protected Entity(TId id) => Id = id;
    public TId Id { get; protected init; }
}

public abstract class GuidEntity : Entity<Guid>
{
    protected GuidEntity() : base(Guid.NewGuid()) { }
    protected GuidEntity(Guid id) : base(id) { }
}
