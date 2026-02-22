using Logger.Core.Domain.Entities;

namespace Logger.Core.Repositories;

public interface IExceptionLogRepository
{
    Task<List<ExceptionLog>> ReadLogs(Guid? TenantId = null,
        string? serviceName = null,
        string? httpMethod = null,
        int? statusCode = null,
        Guid? userId = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null
        );

    Task<ExceptionLog> InsertLog(ExceptionLog log);
}
