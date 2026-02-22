namespace Shared.Kernel.Domain;

public abstract class AuditableAggregateRoot<TId> : AuditableEntity<TId> where TId : IEquatable<TId>
{
    protected AuditableAggregateRoot() { }

    protected AuditableAggregateRoot(TId id) : base(id) { }
}