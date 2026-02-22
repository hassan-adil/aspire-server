namespace Shared.Abstractions.Security.Keycloak.Services;

public interface IKeycloakAdminTokenProvider
{
    Task<string> GenerateAdminAccessTokenAsync();
}
