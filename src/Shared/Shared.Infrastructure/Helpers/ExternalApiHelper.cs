using System.Net.Http.Headers;
using System.Text.Json;

namespace Shared.Infrastructure.Helpers;

public static class ExternalApiHelper
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Sends a GET request and deserializes the JSON response to T.
    /// </summary>
    public static async Task<T?> GetAsync<T>(HttpClient client, string url, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    /// <summary>
    /// Sends a POST request with JSON payload and deserializes the JSON response to TResponse.
    /// </summary>
    public static async Task<TResponse?> PostAsync<TRequest, TResponse>(HttpClient client, string url, TRequest payload, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(JsonSerializer.Serialize(payload));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var response = await client.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<TResponse>(json, _jsonOptions);
    }

    /// <summary>
    /// Sends a POST request with no payload and deserializes the JSON response to TResponse.
    /// </summary>
    public static async Task<TResponse?> PostAsync<TResponse>(HttpClient client, string url, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<TResponse>(json, _jsonOptions);
    }
}
