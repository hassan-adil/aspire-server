using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Kernel.Abstractions.Domain;
using Shared.Kernel.Abstractions.Events;

namespace Shared.Persistence.Interceptors;

public class DispatchDomainEventsInterceptor(IDomainEventDispatcher dispatcher) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(eventData.Context, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public async Task DispatchDomainEvents(DbContext? context, CancellationToken cancellationToken = default)
    {

        if (context == null) return;

        var entities = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count >= 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        await dispatcher.DispatchAsync(domainEvents, cancellationToken);

    }
}
