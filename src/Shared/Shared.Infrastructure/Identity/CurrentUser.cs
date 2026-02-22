using Microsoft.AspNetCore.Http;
using Shared.Abstractions.Dependencies;
using Shared.Abstractions.Identity;
using System.Security.Claims;

namespace Shared.Infrastructure.Identity;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser, IScopedDependency
{
    private readonly ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;

    public Guid? UserId => TryParseGuid(user?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    public Guid? TenantId => TryParseGuid(user?.FindFirst("tenantId")?.Value);

    public IReadOnlyCollection<string> Roles =>
        user?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];

    public IReadOnlyCollection<string> Permissions =>
        user?.FindAll("permissions").Select(c => c.Value).ToArray() ?? [];

    public IReadOnlyCollection<string> Services =>
        user?.FindAll("services").Select(c => c.Value).ToArray() ?? [];

    public bool HasPermission(string permission) =>
        Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);

    public bool HasServiceAccess(string serviceName) =>
        Services.Contains(serviceName, StringComparer.OrdinalIgnoreCase);

    private static Guid? TryParseGuid(string? value) =>
        Guid.TryParse(value, out var guid) ? guid : null;

}
