namespace Shared.Infrastructure.Security.Keycloak.Models.Users;

public record KeycloakCredentialRepresentation
{
    public string Type { get; set; } = "password";
    public string Value { get; set; } = default!;
    public bool Temporary { get; set; } = false;
}
