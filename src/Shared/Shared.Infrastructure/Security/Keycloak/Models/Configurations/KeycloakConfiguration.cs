namespace Shared.Infrastructure.Security.Keycloak.Models.Configurations;

public class KeycloakConfiguration
{
    public const string SectionName = "Keycloak";

    public string ServerUrl { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string AdminRealm { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminClientId { get; set; } = string.Empty;
    public string RealmClientId { get; set; } = string.Empty;
    public string RealmClientSecret { get; set; } = string.Empty;
    public string Authority => $"{ServerUrl}/realms/{Realm}";
    public string AdminApiUrl => $"{ServerUrl}/admin/realms/{Realm}";
    public bool RequireHttpsMetadata { get; set; } = true;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
}
