using Logger.Core.Domain.Entities;

namespace Logger.Core.Repositories;

public interface IDatabaseOperationLogRepository
{
    Task<List<DatabaseOperationLog>> ReadLogs(Guid? tenantId = null,
        string? serviceName = null,
        string? httpMethod = null,
        string? action = null,
        string? entityName = null,
        string? entityId = null,
        Guid? userId = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null
        );

    Task<DatabaseOperationLog> InsertLog(DatabaseOperationLog log);
}
