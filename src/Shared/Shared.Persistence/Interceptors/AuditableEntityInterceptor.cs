using Shared.Abstractions.Identity;
using Shared.Abstractions.MultiTenancy;
using Shared.Kernel.Abstractions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Shared.Persistence.Interceptors;

public class AuditableEntityInterceptor(
        ICurrentUser currentUser,
        ITenantResolver tenantResolver,
        TimeProvider dateTime) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var utcNow = dateTime.GetUtcNow();

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.InsertedBy = currentUser.UserId.HasValue ? currentUser.UserId.Value.ToString() : entry.Entity.InsertedBy;
                    entry.Entity.InsertedByDisplayName = currentUser.UserId.HasValue ? currentUser.UserId.Value.ToString() : entry.Entity.InsertedBy ?? "SYSTEM";
                    entry.Entity.InsertedAt = utcNow;
                }
                entry.Entity.LastModifiedBy = currentUser.UserId.HasValue ? currentUser.UserId.Value.ToString() : entry.Entity.LastModifiedBy;
                entry.Entity.LastModifiedByDisplayName = currentUser.UserId.HasValue ? currentUser.UserId.Value.ToString() : entry.Entity.LastModifiedBy ?? "SYSTEM";
                entry.Entity.LastModifiedAt = utcNow;
            }

            if (entry.State == EntityState.Added && entry.Entity is IMustHaveTenant tenantEntity)
            {
                var tenantId = tenantResolver.ResolveTenant();

                if (tenantId.HasValue)
                    tenantEntity.Tenants ??= [tenantId.Value];
            }
        }
    }
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}

