using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.UnitOfWork;
using Shared.Kernel.Abstractions.Events;
using Shared.Kernel.Domain;

namespace Shared.Persistence;

public class UnitOfWork<TDbContext>(TDbContext dbContext, IDomainEventDispatcher dispatcher)
    : ITransactionalUnitOfWork, IDisposable
    where TDbContext : DbContext
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = dbContext.ChangeTracker.Entries<AggregateRoot<Guid>>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var entry in dbContext.ChangeTracker.Entries<AggregateRoot<Guid>>())
        {
            entry.Entity.ClearDomainEvents();
        }

        var result = await dbContext.SaveChangesAsync(cancellationToken);
        if (domainEvents is not null && domainEvents.Count > 0)
        {
            await dispatcher.DispatchAsync(domainEvents, cancellationToken);

        }

        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.CurrentTransaction != null)
            await dbContext.Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.CurrentTransaction != null)
            await dbContext.Database.RollbackTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
