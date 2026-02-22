namespace Shared.Abstractions.Context;

public record DatabaseOperation(
    string Operation,          // Insert / Update / Delete / Query
    string? EntityName,             // User, Order, Notification
    string? EntityId,
    string? SqlQuery,
    Dictionary<string, object>? OldSnapshot,
    Dictionary<string, object>? NewSnapshot,
    DateTimeOffset Timestamp,
    double? RuntimeMs,
    int? RowsAffected,
    bool IsError,
    string? ErrorMessage = null
);
