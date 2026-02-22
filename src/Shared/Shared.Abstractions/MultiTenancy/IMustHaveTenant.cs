namespace Shared.Abstractions.MultiTenancy;

public interface IMustHaveTenant
{
    ICollection<Guid> Tenants { get; set; }
    bool IsGlobal { get; set; }
}

