using Shared.Abstractions.MultiTenancy;
using Shared.Kernel.Domain;

namespace Logger.Core.Domain.Entities;

public class ExceptionLog : Entity<Guid>, IMustHaveTenant
{
    public ExceptionLog()
    {
    }

    public string ServiceName { get; init; } = string.Empty;
    public ICollection<Guid> Tenants { get; set; } = [];
    public bool IsGlobal { get; set; } = false;
    public string RequestPath { get; init; } = string.Empty;
    public string HttpMethod { get; init; } = string.Empty;
    public string? RequestJson { get; init; } = default;
    public string? TraceIdentifier { get; init; } = default;
    public int? StatusCode { get; init; } = default;
    public string? ErrorMessage { get; init; } = default;
    public string? StackTrace { get; init; } = default;
    public Guid? UserId { get; init; } = default;
    public DateTimeOffset Timestamp { get; init; } = default;
    public double? RuntimeMs { get; set; } = default;

    public ExceptionLog(
        string serviceName,
        Guid? tenantId,
        string requestPath,
        string httpMethod,
        string? requestJson,
        string? traceIdentifier,
        int? statusCode,
        string? errorMessage,
        string? stackTrace,
        Guid? userId,
        DateTimeOffset timestamp,
        double? runtimeMs)
    {
        ServiceName = serviceName;
        Tenants = tenantId.HasValue ? [tenantId.Value] : [];
        RequestPath = requestPath;
        HttpMethod = httpMethod;
        RequestJson = requestJson;
        TraceIdentifier = traceIdentifier;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        StackTrace = stackTrace;
        UserId = userId;
        Timestamp = timestamp;
        RuntimeMs = runtimeMs;
    }
}
