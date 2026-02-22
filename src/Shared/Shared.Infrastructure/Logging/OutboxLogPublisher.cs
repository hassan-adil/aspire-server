using MassTransit;
using Shared.Abstractions.Logging;

namespace Shared.Infrastructure.Logging;

public sealed class OutboxLogPublisher(IBus publish) : ILogPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken ct = default)
        where T : class
        => publish.Publish(message, ct);
}