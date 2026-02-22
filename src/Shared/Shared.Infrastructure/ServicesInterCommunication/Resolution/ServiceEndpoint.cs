namespace Shared.Infrastructure.ServicesInterCommunication.Resolution;

public sealed record ServiceEndpoint(
    Uri BaseUri,
    string Path,
    HttpMethod Method,
    bool RequiresAuth
);