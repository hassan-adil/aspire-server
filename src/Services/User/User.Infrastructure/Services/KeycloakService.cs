using Microsoft.Extensions.Options;
using Refit;
using Shared.Abstractions.Dependencies;
using Shared.Infrastructure.Helpers;
using Shared.Infrastructure.Security.Keycloak.Models.Configurations;
using Shared.Infrastructure.Security.Keycloak.Models.Roles;
using Shared.Infrastructure.Security.Keycloak.Models.Users;
using Shared.Infrastructure.Security.Keycloak.Services;
using Shared.Infrastructure.ServicesInterCommunication.Resolution;

namespace User.Infrastructure.Services;

public class KeycloakService(IServiceEndpointResolver serviceEndpointResolver, IKeycloakAdminApi adminApi, IOptions<KeycloakConfiguration> config) : IScopedDependency
{
    private readonly KeycloakConfiguration configuration = config.Value;

    private async Task<string?> GenerateAdminAccessTokenAsync(bool isTokenBearer = true)
    {
        var endPoint = serviceEndpointResolver.Resolve("Auth", "GetAdminToken");

        var url = new Uri(endPoint.BaseUri, endPoint.Path);

        var token = await ExternalApiHelper.GetAsync<string>(new HttpClient(), url.ToString());

        return isTokenBearer ? $"Bearer {token}" : token;
    }

    public async Task<(List<KeycloakUserRepresentation> List, int Count)> ReadAllUsersAsync(string? search = null, 
        string? email = null, 
        string? userName = null, 
        bool enabled = true, 
        int page = 1, 
        int pageSize = 100)
    {
        var token = await GenerateAdminAccessTokenAsync();

        var first = (page - 1) * pageSize;
        var max = pageSize;
        bool exact = false;

        if (!string.IsNullOrWhiteSpace(search))
        {
            email = null;
            userName = null;
        }
        else if (!string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(userName))
            search = null;

        try
        {
            var usersTask = adminApi.ReadUsersAsync(
                configuration.Realm,
                token!,
                search,
                email,
                userName,
                enabled,
                exact,
                first: first,
                max: max
                );

            var countTask = adminApi.CountUsersAsync(
                configuration.Realm,
                token!,
                search,
                email,
                userName,
                enabled,
                exact
                );

            await Task.WhenAll(usersTask, countTask);

            return (usersTask.Result, countTask.Result);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }
    
    public async Task<KeycloakUserRepresentation> ReadUserByIdAsync(string userId)
    {
        var token = await GenerateAdminAccessTokenAsync();
        return await adminApi.ReadUserByIdAsync(configuration.Realm, userId, $"{token}");
    }
    
    public async Task<Refit.ApiResponse<string>> InsertUserAsync(KeycloakUserCreateRequest user)
    {
        var token = await GenerateAdminAccessTokenAsync();
        return await adminApi.InsertUserAsync(configuration.Realm, user, $"{token}");
    }
    
    public async Task<ApiResponse<object>> ModifyUserAsync(string userId, KeycloakUserUpdateRequest user)
    {
        var token = await GenerateAdminAccessTokenAsync();
        return await adminApi.ModifyUserAsync(configuration.Realm, userId, user, $"{token}");
    }
    
    public async Task<ApiResponse<object>> RemoveUserAsync(string userId)
    {
        var token = await GenerateAdminAccessTokenAsync();
        return await adminApi.RemoveUserAsync(configuration.Realm, userId, $"{token}");
    }
    
    public async Task<List<KeycloakRoleRepresentation>> ReadUserRolesAsync(string userId)
    {
        var token = await GenerateAdminAccessTokenAsync();
        return await adminApi.ReadRealmRolesForUserAsync(configuration.Realm, userId, $"{token}");
    }
    
    public async Task<ApiResponse<object>> InsertRolesToUserAsync(string userId, List<KeycloakRoleRepresentation> roles)
    {
        var token = await GenerateAdminAccessTokenAsync();
        return await adminApi.InsertRealmRolesToUserAsync(configuration.Realm, userId, roles, $"{token}");
    }
    
    public async Task<ApiResponse<object>> RemoveRolesFromUserAsync(string userId, List<KeycloakRoleRepresentation> roles)
    {
        var token = await GenerateAdminAccessTokenAsync();
        return await adminApi.RemoveRealmRolesFromUserAsync(configuration.Realm, userId, roles, $"{token}");
    }

    public async Task<List<KeycloakRoleRepresentation>> ReadAllRolesAsync()
    {
        var token = await GenerateAdminAccessTokenAsync();
        return await adminApi.ReadRealmRolesAsync(configuration.Realm, $"{token}");
    }
    
    public async Task<KeycloakRoleRepresentation> ReadRoleByNameAsync(string roleName)
    {
        var token = await GenerateAdminAccessTokenAsync();
        return await adminApi.ReadRoleByNameAsync(roleName, configuration.Realm, $"{token}");
    }
    
    public async Task<ApiResponse<string>> InsertRealmRoleAsync(KeycloakRoleRepresentation role)
    {
        var token = await GenerateAdminAccessTokenAsync();
        return await adminApi.InsertRealmRoleAsync(configuration.Realm, role, $"{token}");
    }
    
    public async Task<ApiResponse<object>> ModifyRoleAsync(string roleName, KeycloakRoleRepresentation role)
    {
        var token = await GenerateAdminAccessTokenAsync();
        return await adminApi.ModifyRoleAsync(configuration.Realm, roleName, role, $"{token}");
    }
    
    public async Task<ApiResponse<object>> RemoveRoleAsync(string roleName)
    {
        var token = await GenerateAdminAccessTokenAsync();
        return await adminApi.RemoveRole(configuration.Realm, roleName, $"{token}");
    }
}
