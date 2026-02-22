using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Abstractions.Context;
using Shared.Abstractions.Identity;
using Shared.Abstractions.MultiTenancy;
using Shared.Kernel.Abstractions.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Persistence.Interceptors;

public sealed class DbOperationInterceptor(
    ICurrentUser currentUser,
    ITenantResolver tenantResolver,
    TimeProvider timeProvider,
    IRequestExecutionContext executionContext
) : SaveChangesInterceptor
{// We store "pending operations" per DbContext instance.
    // Because one request can use multiple DbContexts.
    private readonly ConcurrentDictionary<DbContext, List<PendingDbOp>> pendingOps = new();
    private readonly ConcurrentDictionary<DbContext, long> startedAtTicks = new();

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context is null) return result;

        ApplyAuditAndTenant(context);

        var startedAt = Stopwatch.GetTimestamp();
        var ops = CapturePendingOperations(context, startedAt);
        pendingOps[context] = ops;

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return ValueTask.FromResult(result);

        ApplyAuditAndTenant(context);

        var startedAt = Stopwatch.GetTimestamp();
        var ops = CapturePendingOperations(context, startedAt);
        pendingOps[context] = ops;

        return ValueTask.FromResult(result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        var context = eventData.Context;
        if (context is null) return result;

        TrackSuccess(context, eventData, rowsAffected: result);
        Cleanup(context);

        return result;
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return ValueTask.FromResult(result);

        TrackSuccess(context, eventData, rowsAffected: result);
        Cleanup(context);

        return ValueTask.FromResult(result);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        var context = eventData.Context;
        if (context is null) return;

        TrackFailure(context, eventData);
        Cleanup(context);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return Task.CompletedTask;

        TrackFailure(context, eventData);
        Cleanup(context);

        return Task.CompletedTask;
    }

    // -----------------------------
    // Core logic
    // -----------------------------

    private void ApplyAuditAndTenant(DbContext context)
    {
        var utcNow = timeProvider.GetUtcNow();
        var tenantId = tenantResolver.ResolveTenant();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditable auditable)
                continue;

            // Added/Modified OR owned entity changed
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedAnyOwnedEntities())
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.InsertedAt = utcNow;
                    auditable.InsertedBy = currentUser.UserId?.ToString() ?? auditable.InsertedBy;
                    auditable.InsertedByDisplayName = currentUser.UserId?.ToString()
                        ?? auditable.InsertedByDisplayName
                        ?? auditable.InsertedBy
                        ?? "SYSTEM";
                }

                auditable.LastModifiedAt = utcNow;
                auditable.LastModifiedBy = currentUser.UserId?.ToString() ?? auditable.LastModifiedBy;
                auditable.LastModifiedByDisplayName = currentUser.UserId?.ToString()
                    ?? auditable.LastModifiedByDisplayName
                    ?? auditable.LastModifiedBy
                    ?? "SYSTEM";
            }

            if (entry.State == EntityState.Added && entry.Entity is IMustHaveTenant tenantEntity)
            {
                if (tenantId.HasValue)
                    tenantEntity.Tenants ??= [tenantId.Value];
            }
        }
    }

    private List<PendingDbOp> CapturePendingOperations(DbContext context, long startedAtTicks)
    {
        var utcNow = timeProvider.GetUtcNow();
        var list = new List<PendingDbOp>(capacity: 8);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            // Skip EF internal stuff
            if (entry.Metadata.IsOwned())
                continue;

            var entityName = entry.Metadata.ClrType.Name;
            var action = entry.State switch
            {
                EntityState.Added => "Insert",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => "Unknown"
            };

            var entityId = TryGetPrimaryKey(entry) ?? "UNKNOWN";

            Dictionary<string, object>? oldSnapshot = null;
            Dictionary<string, object>? newSnapshot = null;

            // Snapshots:
            // - Insert: only new
            // - Update: old + new (only modified props)
            // - Delete: only old
            if (entry.State == EntityState.Added)
            {
                newSnapshot = SnapshotCurrent(entry);
            }
            else if (entry.State == EntityState.Modified)
            {
                oldSnapshot = SnapshotOriginal(entry, onlyModified: true);
                newSnapshot = SnapshotCurrent(entry, onlyModified: true);
            }
            else if (entry.State == EntityState.Deleted)
            {
                oldSnapshot = SnapshotOriginal(entry, onlyModified: false);
            }

            list.Add(new PendingDbOp(
                Operation: action,
                EntityName: entityName,
                EntityId: entityId,
                OldSnapshot: oldSnapshot,
                NewSnapshot: newSnapshot,
                Timestamp: utcNow,
                StartedAtTicks: startedAtTicks
            ));
        }

        return list;
    }

    private void TrackSuccess(DbContext context, SaveChangesCompletedEventData eventData, int rowsAffected)
    {
        if (!pendingOps.TryGetValue(context, out var ops) || ops.Count == 0)
            return;

        var nowTicks = Stopwatch.GetTimestamp();

        // If multiple entities changed, each gets the same SaveChanges runtime.
        // This is a normal and accepted tradeoff.
        foreach (var op in ops)
        {
            var runtimeMs = ((nowTicks - op.StartedAtTicks) / (double)Stopwatch.Frequency) * 1000.0;

            var finalOp = new DatabaseOperation(
                Operation: op.Operation,
                EntityName: op.EntityName,
                EntityId: op.EntityId,
                SqlQuery: null, // Capturing the actual SQL would require deeper interception (e.g. DbCommandInterceptor)
                OldSnapshot: op.OldSnapshot,
                NewSnapshot: op.NewSnapshot,
                Timestamp: op.Timestamp,
                RuntimeMs: runtimeMs,
                ErrorMessage: null,
                RowsAffected: rowsAffected,
                IsError: false
            );

            executionContext.AddDatabaseOperation(finalOp);
        }
    }

    private void TrackFailure(DbContext context, DbContextErrorEventData eventData)
    {
        if (!pendingOps.TryGetValue(context, out var ops) || ops.Count == 0)
            return;

        var nowTicks = Stopwatch.GetTimestamp();
        var error = eventData.Exception?.Message ?? "Unknown EF Core SaveChanges failure";

        foreach (var op in ops)
        {
            var runtimeMs = ((nowTicks - op.StartedAtTicks) / (double)Stopwatch.Frequency) * 1000.0;

            var finalOp = new DatabaseOperation(
                Operation: op.Operation,
                EntityName: op.EntityName,
                EntityId: op.EntityId,
                SqlQuery: null,
                OldSnapshot: op.OldSnapshot,
                NewSnapshot: op.NewSnapshot,
                Timestamp: op.Timestamp,
                RuntimeMs: runtimeMs,
                ErrorMessage: error,
                RowsAffected: null,
                IsError: true
            );

            executionContext.AddDatabaseOperation(finalOp);
        }
    }

    private void Cleanup(DbContext context)
    {
        pendingOps.TryRemove(context, out _);
    }

    // -----------------------------
    // Helpers
    // -----------------------------

    private static string? TryGetPrimaryKey(EntityEntry entry)
    {
        try
        {
            var pk = entry.Metadata.FindPrimaryKey();
            if (pk is null) return null;

            var values = pk.Properties
                .Select(p => entry.Property(p.Name).CurrentValue ?? entry.Property(p.Name).OriginalValue)
                .Where(v => v is not null)
                .Select(v => v!.ToString());

            var joined = string.Join("|", values);
            return string.IsNullOrWhiteSpace(joined) ? null : joined;
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, object>? SnapshotCurrent(EntityEntry entry, bool onlyModified = false)
    {
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.IsShadowProperty())
                continue;

            if (prop.Metadata.IsPrimaryKey())
                continue;

            if (onlyModified && !prop.IsModified)
                continue;

            dict[prop.Metadata.Name] = prop.CurrentValue ?? new { };
        }

        return dict.Count == 0 ? null : dict;
    }

    private static Dictionary<string, object>? SnapshotOriginal(EntityEntry entry, bool onlyModified)
    {
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.IsShadowProperty())
                continue;

            if (prop.Metadata.IsPrimaryKey())
                continue;

            if (onlyModified && !prop.IsModified)
                continue;

            dict[prop.Metadata.Name] = prop.OriginalValue ?? new { };
        }

        return dict.Count == 0 ? [] : dict;
    }

    // -----------------------------
    // Pending operation type
    // -----------------------------
    private sealed record PendingDbOp(
        string Operation,
        string EntityName,
        string EntityId,
        Dictionary<string, object>? OldSnapshot,
        Dictionary<string, object>? NewSnapshot,
        DateTimeOffset Timestamp,
        long StartedAtTicks
    );

    private double? GetRuntimeMs(DbContext context)
    {
        if (!startedAtTicks.TryGetValue(context, out var start))
            return null;

        var end = Stopwatch.GetTimestamp();
        var elapsedSeconds = (end - start) / (double)Stopwatch.Frequency;
        return elapsedSeconds * 1000.0;
    }
}

// -----------------------------
// Extension reused from your code
// -----------------------------
public static class EfEntryExtensions
{
    public static bool HasChangedAnyOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
