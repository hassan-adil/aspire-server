using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.ServicesInterCommunication.Configuration;

public sealed class ServiceDefinition
{
    public string Name { get; init; } = default!;
    public string BaseUrl { get; init; } = default!;
    public int TimeoutSeconds { get; init; } = 5;

    public Dictionary<string, ServiceEndpointDefinition> Endpoints { get; init; }
        = new(StringComparer.OrdinalIgnoreCase);
}
