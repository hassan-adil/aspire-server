using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.ServicesInterCommunication.Configuration;
using Shared.Infrastructure.ServicesInterCommunication.Resolution;

namespace Shared.Infrastructure.ServicesInterCommunication.Extensions;

public static class ServiceRegistryServiceCollectionExtensions
{
    public static IServiceCollection AddServiceRegistry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ServiceRegistryOptions>(
            configuration.GetSection("ServiceRegistry"));

        services.AddSingleton<IServiceEndpointResolver, ServiceEndpointResolver>();

        return services;
    }
}
