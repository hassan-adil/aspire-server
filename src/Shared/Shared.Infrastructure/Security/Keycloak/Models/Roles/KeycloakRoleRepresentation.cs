namespace Shared.Infrastructure.Security.Keycloak.Models.Roles;

public record KeycloakRoleRepresentation
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool Composite { get; set; } = false;
    public Dictionary<string, List<string>>? Attributes { get; set; }
};
