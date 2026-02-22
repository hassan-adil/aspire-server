namespace Shared.Persistence.Options;

public sealed class DbOperationLoggingOptions
{
    public int SlowQueryThresholdMs { get; init; } = 200;
    public bool AlwaysLogWrites { get; init; } = true;
    public bool AlwaysLogErrors { get; init; } = true;
    public bool IgnoreMigrations { get; init; } = true;
    public bool IgnoreHealthChecks { get; init; } = true;
}