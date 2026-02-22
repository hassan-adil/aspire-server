namespace Shared.Abstractions.Identity;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> Permissions { get; }
    IReadOnlyCollection<string> Services { get; }

    bool HasPermission(string permission);
    bool HasServiceAccess(string serviceName);
}
