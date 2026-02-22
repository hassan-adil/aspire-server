namespace Shared.Infrastructure.ServicesInterCommunication.Resolution;

public interface IServiceEndpointResolver
{
    ServiceEndpoint Resolve(string serviceName, string endpointName);
}
