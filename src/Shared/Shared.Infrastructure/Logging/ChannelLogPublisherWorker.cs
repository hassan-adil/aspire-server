using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Shared.Infrastructure.Logging;

public sealed class ChannelLogPublisherWorker(
    Channel<object> channel,
    IBus bus,
    ILogger<ChannelLogPublisherWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var msg in channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await bus.Publish(msg, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish log message from Channel.");
            }
        }
    }
}