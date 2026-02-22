using Logger.Core.Domain.Entities;
using Logger.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.Dependencies;
using Shared.Abstractions.UnitOfWork;

namespace Logger.Infrastructure.Repositories;

public class RequestLogRepository(LoggerDbContext dbContext, IUnitOfWork unitOfWork) : IRequestLogRepository, IScopedDependency
{
    public async Task<List<RequestLog>> ReadLogs(Guid? tenantId = null, string? serviceName = null, string? httpMethod = null, int? statusCode = null, Guid? userId = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        var query = dbContext.RequestLogs.AsQueryable();

        if (tenantId.HasValue && tenantId != Guid.Empty)
            query = query.Where(x => x.Tenants.Contains(tenantId.Value));
        
        if (!string.IsNullOrWhiteSpace(serviceName))
            query = query.Where(x => x.ServiceName == serviceName);

        if (!string.IsNullOrWhiteSpace(httpMethod))
            query = query.Where(x => x.HttpMethod == httpMethod.ToUpper());

        if (statusCode.HasValue)
            query = query.Where(x => x.StatusCode == statusCode.Value);

        if (userId.HasValue && userId != Guid.Empty)
            query = query.Where(x => x.UserId == userId);

        if (startDate.HasValue && endDate.HasValue && startDate.Value < endDate.Value)
            query = query.Where(x => x.Timestamp >= startDate.Value && x.Timestamp <= endDate.Value);

        return await query.ToListAsync();
    }

    public async Task<RequestLog> InsertLog(RequestLog log)
    {
        dbContext.RequestLogs.Add(log);
        await unitOfWork.SaveChangesAsync();

        return log;
    }
}
