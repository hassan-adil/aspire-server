namespace Shared.Contracts.Logging;

public record LogException(
    string ServiceName,
    Guid? TenantId,
    string RequestPath,
    string HttpMethod,
    string? RequestJson,
    string? TraceIdentifier,
    int? StatusCode,
    string? ErrorMessage,
    string? StackTrace,
    Guid? UserId,
    DateTimeOffset Timestamp,
    double? RuntimeMs);