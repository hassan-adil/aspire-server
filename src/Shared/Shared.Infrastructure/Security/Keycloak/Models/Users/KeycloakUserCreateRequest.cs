namespace Shared.Infrastructure.Security.Keycloak.Models.Users;

public record KeycloakUserCreateRequest
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public bool Enabled { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Dictionary<string, object>? Attributes { get; set; }
}
