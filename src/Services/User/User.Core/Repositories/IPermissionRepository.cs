using Shared.Abstractions.Repositories;
using User.Core.Domain.Entities;

namespace User.Core.Repositories;

public interface IPermissionRepository : IRepository<Permission, Guid>
{
}
