using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Dependencies;
using System.Reflection;

namespace Shared.Infrastructure.DI;

public static class ServiceRegistrationHelper
{
    public static void RegisterServices(IServiceCollection services, Assembly assembly)
    {
        RegisterByMarker<ITransientDependency>(services, assembly, ServiceLifetime.Transient);
        RegisterByMarker<IScopedDependency>(services, assembly, ServiceLifetime.Scoped);
        RegisterByMarker<ISingletonDependency>(services, assembly, ServiceLifetime.Singleton);

        // Register repositories with naming convention 
        RegisterRepositories(services, assembly);

        // Register services with naming convention
        RegisterServicesByConvention(services, assembly);
    }

    private static void RegisterByMarker<TMarker>(IServiceCollection services, Assembly assembly, ServiceLifetime lifetime)
    {
        var implementations = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(TMarker).IsAssignableFrom(t));

        foreach (var impl in implementations)
        {
            var interfaces = impl.GetInterfaces()
                .Where(i => i != typeof(TMarker) && !i.IsAssignableFrom(typeof(TMarker)))
                .ToList();

            if (interfaces.Count > 0)
            {
                // Register primary interface (usually the first business interface)
                var primaryInterface = interfaces.FirstOrDefault(i => !i.Name.Contains("Dependency")) ?? interfaces.First();
                services.Add(new ServiceDescriptor(primaryInterface, impl, lifetime));

                // Register additional interfaces if any
                foreach (var additionalInterface in interfaces.Where(i => i != primaryInterface))
                {
                    services.Add(new ServiceDescriptor(additionalInterface, provider => provider.GetRequiredService(primaryInterface), lifetime));
                }
            }
            else
            {
                services.Add(new ServiceDescriptor(impl, impl, lifetime));
            }
        }
    }

    private static void RegisterRepositories(IServiceCollection services, Assembly assembly)
    {
        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"))
            .ToList();

        foreach (var repoType in repositoryTypes)
        {
            var interfaces = repoType.GetInterfaces()
                .Where(i => i.Name is ['I', ..] && i.Name.EndsWith("Repository"))
                .ToList();

            foreach (var repoInterface in interfaces)
            {
                if (!services.Any(s => s.ServiceType == repoInterface))
                {
                    services.AddScoped(repoInterface, repoType);
                }
            }
        }
    }

    private static void RegisterServicesByConvention(IServiceCollection services, Assembly assembly)
    {
        var serviceTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract &&
                       (t.Name.EndsWith("Service") || t.Name.EndsWith("Manager")) &&
                       !t.Name.EndsWith("Repository"))
            .ToList();

        foreach (var serviceType in serviceTypes)
        {
            var interfaces = serviceType.GetInterfaces()
                .Where(i => i.Name is ['I', ..] &&
                           (i.Name.EndsWith("Service") || i.Name.EndsWith("Manager")))
                .ToList();

            foreach (var serviceInterface in interfaces)
            {
                if (!services.Any(s => s.ServiceType == serviceInterface))
                {
                    services.AddScoped(serviceInterface, serviceType);
                }
            }
        }
    }
}