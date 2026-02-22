using Shared.Abstractions.MultiTenancy;
using Shared.Kernel.Domain;

namespace User.Core.Domain.Entities;

public class Permission : AuditableAggregateRoot<Guid>, IMustHaveTenant
{
    public string Name { get; init; } = string.Empty;
    public ICollection<Guid> Tenants { get; set; } = [];
    public bool IsGlobal { get; set; } = false;
}
