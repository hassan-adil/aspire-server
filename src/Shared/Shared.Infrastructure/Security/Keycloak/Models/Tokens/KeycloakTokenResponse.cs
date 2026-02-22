namespace Shared.Infrastructure.Security.Keycloak.Models.Tokens;

public record KeycloakTokenResponse
{
    public string Access_token { get; set; } = string.Empty;
    public string Token_type { get; set; } = string.Empty;
    public int Expires_in { get; set; }
    public int Refresh_expires_in { get; set; }
    public string Refresh_token { get; set; } = string.Empty;
    public string Session_state { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;

    // Computed Expiration
    public DateTime ExpiresAt => DateTime.UtcNow.AddSeconds(Expires_in);
    public DateTime RefreshExpiresAt => DateTime.UtcNow.AddSeconds(Refresh_expires_in);
};
