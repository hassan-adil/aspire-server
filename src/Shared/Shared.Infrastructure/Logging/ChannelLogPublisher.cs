using Shared.Abstractions.Logging;
using System.Threading.Channels;

namespace Shared.Infrastructure.Logging;

public sealed class ChannelLogPublisher(Channel<object> channel) : ILogPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken ct = default)
        where T : class
        => channel.Writer.WriteAsync(message, ct).AsTask();
}