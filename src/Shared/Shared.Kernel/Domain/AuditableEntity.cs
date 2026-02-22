using Shared.Kernel.Abstractions.Domain;

namespace Shared.Kernel.Domain;

public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
where TId : IEquatable<TId>
{
    protected AuditableEntity(TId id) : base(id) { }
    protected AuditableEntity()
    {
        Id = default!;
    }

    public DateTimeOffset InsertedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? InsertedBy { get; set; }
    public string? InsertedByDisplayName { get; set; } = "SYSTEM";

    public DateTimeOffset LastModifiedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? LastModifiedBy { get; set; }
    public string? LastModifiedByDisplayName { get; set; } = "SYSTEM";
}
