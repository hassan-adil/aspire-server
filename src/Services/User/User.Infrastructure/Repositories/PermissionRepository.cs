using Shared.Abstractions.Dependencies;
using Shared.Abstractions.MultiTenancy;
using Shared.Persistence.Repositories;
using User.Core.Domain.Entities;
using User.Core.Repositories;

namespace User.Infrastructure.Repositories;
public class PermissionRepository(UserDbContext dbContext, ITenantResolver tenantResolver) : RepositoryBase<Permission, Guid>(dbContext, tenantResolver), IPermissionRepository, IScopedDependency
{
}
