using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Shared.Abstractions.Context;
using Shared.Persistence.Options;
using System.Data.Common;

namespace Shared.Persistence.Interceptors;

public sealed class RequestDbCommandInterceptor(
    IRequestExecutionContext executionContext,
    IOptions<DbOperationLoggingOptions> _options
) : DbCommandInterceptor
{
    private readonly DbOperationLoggingOptions options = _options.Value;

    // ----------------------------
    // SUCCESS: SELECT (Reader)
    // ----------------------------
    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        TrackSuccess(command, eventData, rowsAffected: null);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        TrackSuccess(command, eventData, rowsAffected: null);
        return ValueTask.FromResult(result);
    }

    // ----------------------------
    // SUCCESS: Scalar (COUNT, Any, etc.)
    // ----------------------------
    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        TrackSuccess(command, eventData, rowsAffected: null);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        TrackSuccess(command, eventData, rowsAffected: null);
        return ValueTask.FromResult(result);
    }

    // ----------------------------
    // SUCCESS: NonQuery (INSERT/UPDATE/DELETE)
    // ----------------------------
    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        TrackSuccess(command, eventData, rowsAffected: result);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        TrackSuccess(command, eventData, rowsAffected: result);
        return ValueTask.FromResult(result);
    }

    // ----------------------------
    // FAILURES
    // ----------------------------
    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        TrackFailure(command, eventData);
    }

    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        TrackFailure(command, eventData);
        return Task.CompletedTask;
    }

    // ============================================================
    // INTERNAL
    // ============================================================

    private void TrackSuccess(DbCommand command, CommandExecutedEventData eventData, int? rowsAffected)
    {
        var sql = command.CommandText;

        if (string.IsNullOrWhiteSpace(sql))
            return;

        var durationMs = eventData.Duration.TotalMilliseconds;

        if (!ShouldLog(sql, durationMs, isError: false))
            return;

        executionContext.AddDatabaseOperation(new DatabaseOperation(
            Operation: MapOperation(sql),
            SqlQuery: sql,
            EntityName: string.Empty,
            EntityId: null,
            OldSnapshot: null,
            NewSnapshot: null,
            Timestamp: DateTimeOffset.UtcNow,
            RuntimeMs: durationMs,
            RowsAffected: rowsAffected,
            IsError: false,
            ErrorMessage: null
        ));
    }

    private void TrackFailure(DbCommand command, CommandErrorEventData eventData)
    {
        var sql = command.CommandText;

        if (string.IsNullOrWhiteSpace(sql))
            return;

        var durationMs = eventData.Duration.TotalMilliseconds;

        if (options.AlwaysLogErrors == false)
            return;

        if (!ShouldLog(sql, durationMs, isError: true))
            return;

        executionContext.AddDatabaseOperation(new DatabaseOperation(
            Operation: MapOperation(sql),
            SqlQuery: sql,
            EntityName: string.Empty,
            EntityId: null,
            OldSnapshot: null,
            NewSnapshot: null,
            Timestamp: DateTimeOffset.UtcNow,
            RuntimeMs: durationMs,
            RowsAffected: null,
            IsError: true,
            ErrorMessage: eventData.Exception?.Message
        ));
    }

    private bool ShouldLog(string sql, double durationMs, bool isError)
    {
        if (isError)
            return options.AlwaysLogErrors;

        if (ShouldIgnore(sql))
            return false;

        var action = MapOperation(sql);

        // Always log writes
        if (options.AlwaysLogWrites && action is "Insert" or "Update" or "Delete")
            return true;

        // For reads, log only slow ones
        return durationMs >= options.SlowQueryThresholdMs;
    }

    private bool ShouldIgnore(string sql)
    {
        if (options.IgnoreMigrations)
        {
            if (sql.Contains("__EFMigrationsHistory", StringComparison.OrdinalIgnoreCase))
                return true;

            if (sql.Contains("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
                return true;

            if (sql.Contains("ALTER TABLE", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        if (options.IgnoreHealthChecks)
        {
            if (sql.Trim().Equals("SELECT 1", StringComparison.OrdinalIgnoreCase))
                return true;

            if (sql.Contains("pg_catalog", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string MapOperation(string sql)
    {
        sql = sql.TrimStart();

        if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)) return "QUERY";
        if (sql.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase)) return "INSERT";
        if (sql.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase)) return "MODIFY";
        if (sql.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase)) return "REMOVE";

        return "Command";
    }
}