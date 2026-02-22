namespace Shared.Infrastructure.ServicesInterCommunication.Configuration;

public sealed class ServiceEndpointDefinition
{
    public string Path { get; init; } = default!;
    public HttpMethod Method { get; init; } = HttpMethod.Get;

    // future-proofing
    public bool RequiresAuth { get; init; } = true;
    public string? Description { get; init; }
}
