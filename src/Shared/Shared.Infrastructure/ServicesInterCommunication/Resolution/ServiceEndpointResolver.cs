using Microsoft.Extensions.Options;
using Shared.Infrastructure.ServicesInterCommunication.Configuration;

namespace Shared.Infrastructure.ServicesInterCommunication.Resolution;

public sealed class ServiceEndpointResolver(IOptions<ServiceRegistryOptions> options) : IServiceEndpointResolver
{
    private readonly Dictionary<string, ServiceDefinition> _services = options.Value.Services
            .ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

    public ServiceEndpoint Resolve(string serviceName, string endpointName)
    {
        if (!_services.TryGetValue(serviceName, out var service))
            throw new KeyNotFoundException($"Service '{serviceName}' not configured.");

        if (!service.Endpoints.TryGetValue(endpointName, out var endpoint))
            throw new KeyNotFoundException(
                $"Endpoint '{endpointName}' not configured for service '{serviceName}'.");

        return new ServiceEndpoint(
            new Uri(service.BaseUrl),
            endpoint.Path,
            endpoint.Method,
            endpoint.RequiresAuth
        );
    }
}
