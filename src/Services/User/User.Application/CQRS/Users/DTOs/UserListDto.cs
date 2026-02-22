using Shared.Infrastructure.Security.Keycloak.Helpers;
using Shared.Infrastructure.Security.Keycloak.Models.Users;

namespace User.Application.CQRS.Users.DTOs;

public record UserListDto(string Id, 
    string UserName, 
    string Email, 
    bool? Enabled, 
    string FirstName, 
    string LastName, 
    string? FullName, 
    bool IsGlobal, 
    List<string>? Tenants)
{
    public static explicit operator UserListDto?(KeycloakUserRepresentation? user)
    {
        if (user == null) return null;

        return new UserListDto(
            Id: user.Id ?? string.Empty,
            UserName: user.Username ?? string.Empty,
            Email: user.Email ?? string.Empty,
            Enabled: user.Enabled,
            FirstName: user.FirstName ?? string.Empty,
            LastName: user.LastName ?? string.Empty,
            FullName: user.FirstName != null && user.LastName != null ? $"{user.FirstName} {user.LastName}" : null,
            IsGlobal: user.Attributes.ReadScalar<bool>("isGlobal"),
            Tenants: [.. user.Attributes.ReadList<string>("tenants")]
        );
    }

    public static List<UserListDto?> TransformAll(List<KeycloakUserRepresentation>? users)
    {
        var userDtos = new List<UserListDto?>();

        if (users == null || (users != null && users.Count == 0)) return userDtos;

        userDtos = [.. users!.Select(user =>
        {
            return (UserListDto?)user;
        })];

        return userDtos;
    }
}
