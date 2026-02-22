namespace Shared.Infrastructure.ServicesInterCommunication.Configuration;

public sealed class ServiceRegistryOptions
{
    public List<ServiceDefinition> Services { get; init; } = [];
}
