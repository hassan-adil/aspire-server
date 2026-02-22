namespace Shared.Abstractions.Logging;

public interface ILogPublisher
{
    Task PublishAsync<T>(T message, CancellationToken ct = default)
        where T : class;
}
