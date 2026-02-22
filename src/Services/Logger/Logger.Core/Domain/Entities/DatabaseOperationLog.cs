using Shared.Abstractions.MultiTenancy;
using Shared.Kernel.Domain;

namespace Logger.Core.Domain.Entities;

public class DatabaseOperationLog : Entity<Guid>, IMustHaveTenant
{
    public DatabaseOperationLog()
    {
    }

    // ----- REQUEST AND SERVICE METADATA -----
    public string ServiceName { get; set; } = string.Empty;
    public ICollection<Guid> Tenants { get; set; } = [];
    public bool IsGlobal { get; set; } = false;
    public string RequestPath { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string? TraceIdentifier { get; set; } = default;
    public Guid? UserId { get; set; } = default;
    // ----------

    // ----- DATABASE OPERATION DATA -----
    public string Operation { get; set; } = string.Empty;
    public string? EntityName { get; set; } = default;
    public string? EntityId { get; set; } = default;
    public string? SqlQuery { get; set; } = default;
    public Dictionary<string, object>? OldSnapshot { get; set; } = [];
    public Dictionary<string, object>? NewSnapshot { get; set; } = [];
    public DateTimeOffset Timestamp { get; set; } = default;
    public bool IsError { get; set; } = false;
    public string? ErrorMessage { get; set; } = null;
    // ----------

    // ----- DIAGNOSTICS DATA -----
    public double? RuntimeMs { get; set; } = default;
    public int? RowsAffected { get; set; } = default;
    // ----------

    public DatabaseOperationLog(string serviceName,
        Guid? tenantId,
        string requestPath,
        string httpMethod,
        string? traceIdentifier,
        Guid? userId,
        string operation,
        string? entityName,
        string? entityId,
        string? sqlQuery,
        Dictionary<string, object>? oldSnapshot,
        Dictionary<string, object>? newSnapshot,
        DateTimeOffset timestamp,
        bool isError,
        string? errorMessage,
        double? runtimeMs,
        int? rowsAffected)
    {
        ServiceName = serviceName;
        Tenants = tenantId.HasValue ? [tenantId.Value] : [];
        RequestPath = requestPath;
        HttpMethod = httpMethod;
        TraceIdentifier = traceIdentifier;
        UserId = userId;
        Operation = operation;
        EntityName = entityName;
        EntityId = entityId;
        SqlQuery = sqlQuery;
        OldSnapshot = oldSnapshot;
        NewSnapshot = newSnapshot;
        Timestamp = timestamp;
        IsError = isError;
        ErrorMessage = errorMessage;
        RuntimeMs = runtimeMs;
        RowsAffected = rowsAffected;
    }
}
