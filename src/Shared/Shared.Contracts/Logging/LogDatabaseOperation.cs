namespace Shared.Contracts.Logging;

public record LogDatabaseOperation(
    string ServiceName,
    Guid? TenantId,
    string? EntityName,
    string? EntityId,
    string Action,
    string RequestPath,
    string HttpMethod,
    string? TraceIdentifier,
    Dictionary<string, object>? OldSnapshot,
    Dictionary<string, object>? NewSnapshot,
    Guid? UserId,
    DateTimeOffset Timestamp,
    bool IsError,
    string? ErrorMessage,
    double? RuntimeMs,
    int? RowsAffected,
    string? SqlQuery);
