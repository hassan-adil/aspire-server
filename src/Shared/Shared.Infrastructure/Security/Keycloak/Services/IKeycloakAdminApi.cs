using Refit;
using Shared.Infrastructure.Security.Keycloak.Models.Clients;
using Shared.Infrastructure.Security.Keycloak.Models.Events;
using Shared.Infrastructure.Security.Keycloak.Models.Groups;
using Shared.Infrastructure.Security.Keycloak.Models.Realms;
using Shared.Infrastructure.Security.Keycloak.Models.Roles;
using Shared.Infrastructure.Security.Keycloak.Models.Sessions;
using Shared.Infrastructure.Security.Keycloak.Models.Tokens;
using Shared.Infrastructure.Security.Keycloak.Models.Users;

namespace Shared.Infrastructure.Security.Keycloak.Services;

[Headers("Content-Type: application/json")]
public interface IKeycloakAdminApi
{
    // ===========================
    // AUTHENTICATION
    // ===========================

    [Post("/realms/master/protocol/openid-connect/token")]
    [Headers("Content-Type: application/x-www-form-urlencoded")]
    Task<KeycloakTokenResponse> GenerateAdminTokenAsync(
        [Body(BodySerializationMethod.UrlEncoded)]
        Dictionary<string, object> tokenRequest);

    [Post("/realms/{realm}/protocol/openid-connect/token")]
    [Headers("Content-Type: application/x-www-form-urlencoded")]
    Task<KeycloakTokenResponse> GenerateClientTokenAsync(
        string realm,
        [Body(BodySerializationMethod.UrlEncoded)]
        Dictionary<string, object> tokenRequest);


    // ===========================
    // REALMS
    // ===========================

    [Get("/admin/realms")]
    Task<List<KeycloakRealmRepresentation>> ReadRealmsAsync(
        [Header("Authorization")] string authorization);

    [Get("/admin/realms/{realm}")]
    Task<KeycloakRealmRepresentation> ReadRealmAsync(
        string realm,
        [Header("Authorization")] string authorization);


    // ===========================
    // USERS
    // ===========================

    [Get("/admin/realms/{realm}/users")]
    Task<List<KeycloakUserRepresentation>> ReadUsersAsync(
        string realm,
        [Header("Authorization")] string authorization,
        [Query] string? search = null,
        [Query] string? email = null,
        [Query] string? username = null,
        [Query] bool enabled = true,
        [Query] bool exact = false,
        [Query] int first = 0,
        [Query] int max = 100);

    [Get("/admin/realms/{realm}/users/count")]
    Task<int> CountUsersAsync(
        string realm,
        [Header("Authorization")] string authorization,
        [Query] string? search = null,
        [Query] string? email = null,
        [Query] string? userName = null,
        [Query] bool enabled = true,
        [Query] bool exact = false
    );

    [Get("/admin/realms/{realm}/users/{userId}")]
    Task<KeycloakUserRepresentation> ReadUserByIdAsync(
        string realm,
        string userId,
        [Header("Authorization")] string authorization);

    [Post("/admin/realms/{realm}/users")]
    Task<ApiResponse<string>> InsertUserAsync(
        string realm,
        [Body] KeycloakUserCreateRequest request,
        [Header("Authorization")] string authorization);

    [Put("/admin/realms/{realm}/users/{userId}")]
    Task<ApiResponse<object>> ModifyUserAsync(
        string realm,
        string userId,
        [Body] KeycloakUserUpdateRequest request,
        [Header("Authorization")] string authorization);

    [Delete("/admin/realms/{realm}/users/{userId}")]
    Task<ApiResponse<object>> RemoveUserAsync(
        string realm,
        string userId,
        [Header("Authorization")] string authorization);

    [Put("/admin/realms/{realm}/users/{userId}/reset-password")]
    Task ResetPasswordAsync(
        string realm,
        string userId,
        [Body] KeycloakCredentialRepresentation request,
        [Header("Authorization")] string authorization);

    [Put("/admin/realms/{realm}/users/{userId}/execute-actions-email")]
    Task ExecuteActionsEmailAsync(
        string realm,
        string userId,
        [Query(CollectionFormat.Multi)] IEnumerable<string> actions,
        [Header("Authorization")] string authorization);


    // ===========================
    // USER ROLES
    // ===========================

    [Get("/admin/realms/{realm}/users/{userId}/role-mappings/realm")]
    Task<List<KeycloakRoleRepresentation>> ReadRealmRolesForUserAsync(
        string realm,
        string userId,
        [Header("Authorization")] string authorization);

    [Post("/admin/realms/{realm}/users/{userId}/role-mappings/realm")]
    Task<ApiResponse<object>> InsertRealmRolesToUserAsync(
        string realm,
        string userId,
        [Body] List<KeycloakRoleRepresentation> roles,
        [Header("Authorization")] string authorization);

    [Delete("/admin/realms/{realm}/users/{userId}/role-mappings/realm")]
    Task<ApiResponse<object>> RemoveRealmRolesFromUserAsync(
        string realm,
        string userId,
        [Body] List<KeycloakRoleRepresentation> roles,
        [Header("Authorization")] string authorization);


    // ===========================
    // USER GROUPS
    // ===========================

    [Get("/admin/realms/{realm}/users/{userId}/groups")]
    Task<List<KeycloakGroupRepresentation>> ReadUserGroupsAsync(
        string realm,
        string userId,
        [Header("Authorization")] string authorization);

    [Put("/admin/realms/{realm}/users/{userId}/groups/{groupId}")]
    Task JoinGroupAsync(
        string realm,
        string userId,
        string groupId,
        [Header("Authorization")] string authorization);

    [Delete("/admin/realms/{realm}/users/{userId}/groups/{groupId}")]
    Task LeaveGroupAsync(
        string realm,
        string userId,
        string groupId,
        [Header("Authorization")] string authorization);


    // ===========================
    // CLIENTS
    // ===========================

    [Get("/admin/realms/{realm}/clients")]
    Task<List<KeycloakClientRepresentation>> ReadClientsAsync(
        string realm,
        [Header("Authorization")] string authorization,
        [Query] string? clientId = null);

    [Get("/admin/realms/{realm}/clients/{clientId}")]
    Task<KeycloakClientRepresentation> ReadClientByIdAsync(
        string realm,
        string clientId,
        [Header("Authorization")] string authorization);

    [Get("/admin/realms/{realm}/clients/{clientId}/roles")]
    Task<List<KeycloakRoleRepresentation>> ReadClientRolesAsync(
        string realm,
        string clientId,
        [Header("Authorization")] string authorization);

    [Post("/admin/realms/{realm}/clients")]
    Task<ApiResponse<string>> InsertClientAsync(
        string realm,
        [Body] KeycloakClientRepresentation request,
        [Header("Authorization")] string authorization);


    // ===========================
    // GROUPS
    // ===========================

    [Get("/admin/realms/{realm}/groups")]
    Task<List<KeycloakGroupRepresentation>> ReadGroupsAsync(
        string realm,
        [Header("Authorization")] string authorization);

    [Post("/admin/realms/{realm}/groups")]
    Task<ApiResponse<string>> InsertGroupAsync(
        string realm,
        [Body] KeycloakGroupRepresentation request,
        [Header("Authorization")] string authorization);


    // ===========================
    // REALM ROLES
    // ===========================

    [Get("/admin/realms/{realm}/roles")]
    Task<List<KeycloakRoleRepresentation>> ReadRealmRolesAsync(
        string realm,
        [Header("Authorization")] string authorization);

    [Get("/admin/realms/{realm}/roles/{roleName}")]
    Task<KeycloakRoleRepresentation> ReadRoleByNameAsync(
        string roleName, 
        string realm, 
        [Header("Authorization")] string authorization);

    [Post("/admin/realms/{realm}/roles")]
    Task<ApiResponse<string>> InsertRealmRoleAsync(
        string realm,
        [Body] KeycloakRoleRepresentation request,
        [Header("Authorization")] string authorization);

    [Put("/admin/realms/{realm}/roles/{roleName}")]
    Task<ApiResponse<object>> ModifyRoleAsync(
        string realm, 
        string roleName, 
        [Body] KeycloakRoleRepresentation role, 
        [Header("Authorization")] string authorization);

    [Delete("/admin/realms/{realm}/roles/{roleName}")]
    Task<ApiResponse<object>> RemoveRole(
        string realm, 
        string roleName, 
        [Header("Authorization")] string authorization);

    // ===========================
    // CLIENT-SCOPE
    // ===========================

    [Get("/admin/realms/{realm}/client-scopes")]
    Task<List<KeycloakClientScopeRepresentation>> ReadClientScopesAsync(
        string realm,
        [Header("Authorization")] string authorization);

    [Post("/admin/realms/{realm}/client-scopes")]
    Task<ApiResponse<string>> InsertClientScopeAsync(
        string realm,
        [Body] KeycloakClientScopeRepresentation request,
        [Header("Authorization")] string authorization);


    // ===========================
    // SESSIONS
    // ===========================

    [Get("/admin/realms/{realm}/users/{userId}/sessions")]
    Task<List<KeycloakUserSessionRepresentation>> ReadUserSessionsAsync(
        string realm,
        string userId,
        [Header("Authorization")] string authorization);


    // ===========================
    // EVENTS
    // ===========================

    [Get("/admin/realms/{realm}/events")]
    Task<List<KeycloakEventRepresentation>> ReadEventsAsync(
        string realm,
        [Header("Authorization")] string authorization);
}
