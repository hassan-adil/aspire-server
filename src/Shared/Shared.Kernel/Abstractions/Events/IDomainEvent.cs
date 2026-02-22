namespace Shared.Kernel.Abstractions.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
