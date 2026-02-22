namespace Shared.Contracts.Logging;

public record LogRequest(
    string ServiceName,
    Guid? TenantId,
    string RequestPath,
    string HttpMethod,
    string? RequestJson,
    string? ResponseJson,
    int? StatusCode,
    Guid? UserId,
    DateTimeOffset Timestamp,
    double? RuntimeMs);
