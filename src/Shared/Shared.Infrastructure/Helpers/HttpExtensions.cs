using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Helpers;

public static class HttpExtensions
{
    public static async Task<string> ReadBodyAsStringAsync(this HttpRequest request)
    {
        request.EnableBuffering();
        request.Body.Position = 0;

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        request.Body.Position = 0;
        return body;
    }

    public static async Task<string> ReadBodyAsStringAsync(this HttpResponse response)
    {
        // If body is NOT seekable (normal Kestrel stream)
        if (!response.Body.CanSeek)
        {
            using var buffer = new MemoryStream();
            await response.Body.CopyToAsync(buffer);
            buffer.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(buffer);
            return await reader.ReadToEndAsync();
        }

        // If body IS seekable (MemoryStream)
        response.Body.Seek(0, SeekOrigin.Begin);
        using var seekerReader = new StreamReader(response.Body, leaveOpen: true);
        string text = await seekerReader.ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        return text;
    }
}
