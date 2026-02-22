namespace Shared.Infrastructure.Security.Keycloak.Models.Events;

public record KeycloakEventRepresentation
{
    public long Time { get; set; }
    public string? Type { get; set; }
    public string? RealmId { get; set; }
    public string? ClientId { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, string>? Details { get; set; }
}
