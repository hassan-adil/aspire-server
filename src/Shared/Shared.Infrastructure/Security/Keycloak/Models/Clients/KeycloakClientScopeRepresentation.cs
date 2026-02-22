namespace Shared.Infrastructure.Security.Keycloak.Models.Clients;

public record KeycloakClientScopeRepresentation
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}