namespace Shared.Infrastructure.Security.Keycloak.Models.Sessions;

public record KeycloakUserSessionRepresentation
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public long Start { get; set; }
    public long LastAccess { get; set; }
    public string? IpAddress { get; set; }
    public List<string>? Clients { get; set; }
}
