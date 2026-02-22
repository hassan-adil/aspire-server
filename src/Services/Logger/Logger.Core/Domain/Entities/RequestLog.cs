using Shared.Abstractions.MultiTenancy;
using Shared.Kernel.Domain;

namespace Logger.Core.Domain.Entities;

public class RequestLog : Entity<Guid>, IMustHaveTenant
{
    public RequestLog()
    {
    }

    public string ServiceName { get; init; } = string.Empty;
    public ICollection<Guid> Tenants { get; set; } = [];
    public bool IsGlobal { get; set; } = false;
    public string RequestPath { get; init; } = string.Empty;
    public string HttpMethod { get; init; } = string.Empty;
    public string? RequestJson { get; init; } = default;
    public string? ResponseJson { get; init; } = default;
    public int? StatusCode { get; init; } = default;
    public Guid? UserId { get; init; } = default;
    public DateTimeOffset Timestamp { get; init; } = default;
    public double? RuntimeMs { get; set; } = default;

    public RequestLog(
        string serviceName,
        Guid? tenantId,
        string requestPath,
        string httpMethod,
        string? requestJson,
        string? responseJson,
        int? statusCode,
        Guid? userId,
        DateTimeOffset timestamp,
        double? runtimeMs)
    {
        ServiceName = serviceName;
        Tenants = tenantId.HasValue ? [tenantId.Value] : [];
        RequestPath = requestPath;
        HttpMethod = httpMethod;
        RequestJson = requestJson;
        ResponseJson = responseJson;
        StatusCode = statusCode;
        UserId = userId;
        Timestamp = timestamp;
        RuntimeMs = runtimeMs;
    }
}
