namespace Shared.Abstractions.MultiTenancy;

public interface ITenantResolver
{
    Guid? ResolveTenant();
}

