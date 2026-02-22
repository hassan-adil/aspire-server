using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Logging;
using System.Threading.Channels;

namespace Shared.Infrastructure.Logging.Extensions;

public static class SharedLoggingExtensions
{
    public static IServiceCollection AddSharedLoggingWithOutbox(this IServiceCollection services)
    {
        services.AddScoped<ILogPublisher, OutboxLogPublisher>();
        return services;
    }

    public static IServiceCollection AddSharedLoggingWithoutDb(this IServiceCollection services, int bufferSize = 5000)
    {
        services.AddSingleton(Channel.CreateBounded<object>(new BoundedChannelOptions(bufferSize)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        }));

        services.AddSingleton<ILogPublisher, ChannelLogPublisher>();
        services.AddHostedService<ChannelLogPublisherWorker>();

        return services;
    }
}
