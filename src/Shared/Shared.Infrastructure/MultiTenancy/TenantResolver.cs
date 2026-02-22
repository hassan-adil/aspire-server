using Microsoft.AspNetCore.Http;
using Shared.Abstractions.Dependencies;
using Shared.Abstractions.MultiTenancy;

namespace Shared.Infrastructure.MultiTenancy;

public class TenantResolver(IHttpContextAccessor httpContextAccessor) : ITenantResolver, IScopedDependency
{
    public Guid? ResolveTenant()
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return null;
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader) && Guid.TryParse(tenantIdHeader, out var tenantId))
        {
            return tenantId;
        }
        var claim = context.User?.FindFirst("tenant_id");
        if (claim != null && Guid.TryParse(claim.Value, out var claimTenantId))
        {
            return claimTenantId;
        }
        return null;
    }
}
