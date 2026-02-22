using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.DI;
using System.Reflection;

namespace ServiceDefaults;

public static class ServiceRegistration
{
    public static IServiceCollection AddDefaultServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            ServiceRegistrationHelper.RegisterServices(services, assembly);
        }
        return services;
    }
}
