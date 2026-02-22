namespace Shared.Infrastructure.Security.Keycloak.Models.Realms;

public record KeycloakRealmRepresentation
{
    public string? Id { get; set; }
    public string? Realm { get; set; }
    public bool? Enabled { get; set; }
    public int? AccessTokenLifespan { get; set; }
    public int? RefreshTokenLifespan { get; set; }
}
