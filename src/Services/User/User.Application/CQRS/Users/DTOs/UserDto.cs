using Shared.Infrastructure.Security.Keycloak.Helpers;
using Shared.Infrastructure.Security.Keycloak.Models.Users;

namespace User.Application.CQRS.Users.DTOs;

public record UserDto(string Id,
    string UserName,
    string Email,
    bool Enabled,
    bool EmailVerified,
    string FirstName,
    string LastName,
    string? FullName,
    bool IsGlobal,
    List<string>? Tenants)
{
    public static explicit operator UserDto?(KeycloakUserRepresentation? user)
    {
        if (user == null) return null;

        return new UserDto(
            Id: user.Id ?? string.Empty,
            UserName: user.Username ?? string.Empty,
            Email: user.Email ?? string.Empty,
            Enabled: user.Enabled ?? false,
            EmailVerified: user.EmailVerified ?? false,
            FirstName: user.FirstName ?? string.Empty,
            LastName: user.LastName ?? string.Empty,
            FullName: user.FirstName != null && user.LastName != null ? $"{user.FirstName} {user.LastName}" : null,
            IsGlobal: user.Attributes.ReadScalar<bool>("isGlobal"),
            Tenants: [.. user.Attributes.ReadList<string>("tenants")]
        );
    }
}