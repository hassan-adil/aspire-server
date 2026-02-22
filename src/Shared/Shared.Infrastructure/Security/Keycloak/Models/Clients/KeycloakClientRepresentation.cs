namespace Shared.Infrastructure.Security.Keycloak.Models.Clients;

public record KeycloakClientRepresentation
{
    public string? Id { get; set; }
    public string? ClientId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? Enabled { get; set; }
    public List<string>? RedirectUris { get; set; }
    public List<string>? WebOrigins { get; set; }
}
