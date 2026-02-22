using MediatR;
using Shared.Abstractions.Dependencies;
using Shared.Kernel.Abstractions.Events;

namespace Shared.Infrastructure.Events;

public class DomainEventDispatcher(IMediator mediator) : IDomainEventDispatcher, IScopedDependency
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
