namespace Shared.Kernel.Domain;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : IEquatable<TId>
{
    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }

}
