namespace Shared.Infrastructure.Security.Keycloak.Models.Users;

public record KeycloakUserUpdateRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool? EmailVerified { get; set; }
    public bool? Enabled { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Dictionary<string, object>? Attributes { get; set; }
}
