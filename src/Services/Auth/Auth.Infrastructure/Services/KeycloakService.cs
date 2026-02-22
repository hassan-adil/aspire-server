//using Microsoft.Extensions.Options;
//using Shared.Abstractions.Dependencies;
//using Shared.Abstractions.Security.Keycloak.Services;
//using Shared.Infrastructure.Security.Keycloak.Models.Configurations;
//using Shared.Infrastructure.Security.Keycloak.Models.Users;
//using Shared.Infrastructure.Security.Keycloak.Services;

//namespace Auth.Infrastructure.Services;

//public class KeycloakService(IKeycloakAdminTokenProvider adminTokenProvider, IKeycloakAdminApi adminApi, IOptions<KeycloakConfiguration> config) : IScopedDependency
//{
//    private readonly KeycloakConfiguration configuration = config.Value;

//    public async Task<List<KeycloakUserRepresentation>> GetUsersAsync()
//    {
//        var token = await adminTokenProvider.GenerateAdminAccessTokenAsync();
//        return await adminApi.ReadUsersAsync(configuration.Realm, $"Bearer {token}");
//    }
//}
