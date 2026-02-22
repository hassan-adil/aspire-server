using Shared.Kernel.Abstractions.Domain;
using Shared.Kernel.Abstractions.Events;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Kernel.Domain;

public abstract class Entity<TId> : IEntity<TId>, IHasDomainEvents
where TId : IEquatable<TId>
{
    public TId Id { get; protected set; } = default!;

    protected Entity(TId id)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default))
            throw new ArgumentException("The ID cannot be the default value.", nameof(id));

        Id = id;
        IsDeleted = false;
    }

    protected Entity()
    {
        Id = default!;
        IsDeleted = false;
    }
    private readonly List<IDomainEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public bool IsDeleted { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetUnproxiedType(this) != GetUnproxiedType(other))
            return false;

        if (EqualityComparer<TId>.Default.Equals(Id, default) || EqualityComparer<TId>.Default.Equals(other.Id, default))
            return false;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return (GetUnproxiedType(this).ToString() + Id).GetHashCode();
    }

    private static Type GetUnproxiedType(object obj)
    {
        const string EFCoreProxyPrefix = "Castle.Proxies.";
        const string NHibernateProxyPrefix = "NHibernate.Proxy.";

        Type type = obj.GetType();
        string typeName = type.ToString();

        if (typeName.Contains(EFCoreProxyPrefix) || typeName.Contains(NHibernateProxyPrefix))
            return type.BaseType ?? type;

        return type;
    }
}