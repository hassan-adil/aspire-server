using Logger.Core.Domain.Entities;
using Logger.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.Dependencies;
using Shared.Abstractions.UnitOfWork;

namespace Logger.Infrastructure.Repositories;

public class DatabaseOperationLogRepository(LoggerDbContext dbContext, IUnitOfWork unitOfWork) : IDatabaseOperationLogRepository, IScopedDependency
{
    public async Task<List<DatabaseOperationLog>> ReadLogs(Guid? tenantId = null, string? serviceName = null, string? httpMethod = null, string? operation = null, string? entityName = null, string? entityId = null, Guid? userId = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        var query = dbContext.DatabaseOperationLogs.AsQueryable();

        if (tenantId.HasValue && tenantId != Guid.Empty)
            query = query.Where(x => x.Tenants.Contains(tenantId.Value));

        if (!string.IsNullOrWhiteSpace(serviceName))
            query = query.Where(x => x.ServiceName == serviceName);

        if (!string.IsNullOrWhiteSpace(httpMethod))
            query = query.Where(x => x.HttpMethod == httpMethod);
        
        if (!string.IsNullOrWhiteSpace(operation))
            query = query.Where(x => x.Operation == operation);
        
        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(x => x.EntityName == entityName);
        
        if (!string.IsNullOrWhiteSpace(entityId))
            query = query.Where(x => x.EntityId == entityId);

        if (userId.HasValue && userId != Guid.Empty)
            query = query.Where(x => x.UserId == userId);

        if (startDate.HasValue && endDate.HasValue && startDate.Value < endDate.Value)
            query = query.Where(x => x.Timestamp >= startDate.Value && x.Timestamp <= endDate.Value);

        return await query.ToListAsync();
    }

    public async Task<DatabaseOperationLog> InsertLog(DatabaseOperationLog log)
    {
        dbContext.DatabaseOperationLogs.Add(log);
        await unitOfWork.SaveChangesAsync();

        return log;
    }
}
