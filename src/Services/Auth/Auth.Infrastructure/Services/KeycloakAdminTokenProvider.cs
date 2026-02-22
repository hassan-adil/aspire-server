using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Abstractions.Dependencies;
using Shared.Abstractions.Security.Keycloak.Services;
using Shared.Infrastructure.Security.Keycloak.Models.Configurations;
using Shared.Infrastructure.Security.Keycloak.Models.Tokens;
using Shared.Infrastructure.Security.Keycloak.Services;
using System.Text.Json;

namespace Auth.Infrastructure.Services;

public class KeycloakAdminTokenProvider(
    IDistributedCache redis,
    IKeycloakAdminApi adminApi,
    ILogger<KeycloakAdminTokenProvider> logger,
    IOptions<KeycloakConfiguration> config) : IKeycloakAdminTokenProvider, ISingletonDependency
{
    //private readonly string _realm = config["Keycloak:AdminRealm"] ?? "master";
    private readonly string _clientId = config.Value.AdminClientId;
    private readonly string _adminUserName = config.Value.AdminUsername;
    private readonly string _adminPass = config.Value.AdminPassword;

    private const string CacheKey = "keycloak-admin-token";
    private const string CacheLockKey = "keycloak-admin-token-lock";
    private static readonly TimeSpan LockDuration = TimeSpan.FromSeconds(10);

    public async Task<string> GenerateAdminAccessTokenAsync()
    {
        // 1️⃣ Try to retrieve from Redis
        var cached = await redis.GetStringAsync(CacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            var token = JsonSerializer.Deserialize<KeycloakTokenResponse>(cached)!;

            // If expires in more than 60 seconds → return it
            if (DateTime.UtcNow < token.ExpiresAt.AddMinutes(-3))
                return token.Access_token;
        }

        // 2️⃣ Acquire the distributed lock
        var lockAcquired = await TryAcquireLockAsync();
        if (!lockAcquired)
        {
            // Another instance is refreshing → small wait then return new cached token
            await Task.Delay(500);
            cached = await redis.GetStringAsync(CacheKey);

            if (!string.IsNullOrEmpty(cached))
                return JsonSerializer.Deserialize<KeycloakTokenResponse>(cached)!.Access_token;

            throw new Exception("Failed to retrieve token after waiting for refresh.");
        }

        try
        {
            logger.LogInformation("Refreshing Keycloak Admin Token...");

            var request = new Dictionary<string, object>
            {
                ["grant_type"] = "password",
                ["client_id"] = _clientId,
                ["username"] = _adminUserName,
                ["password"] = _adminPass
            };

            // 3️⃣ Call Keycloak
            var response = await adminApi.GenerateAdminTokenAsync(request);

            // 4️⃣ Cache for full expiry period
            await redis.SetStringAsync(
                CacheKey,
                JsonSerializer.Serialize(response),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(response.Expires_in)
                });

            return response.Access_token;
        }
        finally
        {
            await ReleaseLockAsync();
        }
    }

    private async Task<bool> TryAcquireLockAsync()
    {
        var existing = await redis.GetStringAsync(CacheLockKey);
        if (existing != null) return false;

        await redis.SetStringAsync(
            CacheLockKey,
            "1",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = LockDuration
            });

        return true;
    }

    private Task ReleaseLockAsync() => redis.RemoveAsync(CacheLockKey);
}
