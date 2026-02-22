namespace Shared.Infrastructure.Security.Keycloak.Models.Groups;

public record KeycloakGroupRepresentation
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public List<KeycloakGroupRepresentation>? SubGroups { get; set; }
};
