using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Abstractions.Context;
using Shared.Abstractions.Logging;
using Shared.Contracts.Logging;
using Shared.Contracts.Options;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace Shared.Infrastructure.Middlewares;

public class ApiLoggingMiddleware(ILogger<ApiLoggingMiddleware> logger, 
    ILogPublisher publisher, 
    IRequestExecutionContext executionContext, 
    IOptions<ServiceInfoOptions> _serviceInfoOptions) : IMiddleware
//public class ApiLoggingMiddleware(IBus bus, ILogger<ApiLoggingMiddleware> logger, ISendEndpointProvider send) : IMiddleware
{
    private readonly ServiceInfoOptions serviceInfoOptions = _serviceInfoOptions.Value;

    // ---- Tuning knobs ----
    private const int MaxRequestBodyChars = 20_000;   // ~20 KB-ish
    private const int MaxResponseBodyChars = 50_000;  // ~50 KB-ish

    private static readonly string[] SkipPathPrefixes =
    [
        "/health",
        "/metrics",
        "/swagger",
        "/favicon.ico"
    ];

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // ---- Skip noisy endpoints ----
        if (ShouldSkip(context))
        {
            await next(context);
            return;
        }

        // ---- Collect request-wide values ONCE ----
        var sw = Stopwatch.StartNew();

        var fullUrl = context.Request.GetDisplayUrl();
        var method = context.Request.Method;
        var traceId = context.TraceIdentifier;

        var tenantParsed = Guid.TryParse(context.Request.Headers["X-Tenant-Id"], out var tenantId);
        Guid? tenant = tenantParsed ? tenantId : null;

        Guid? userId = GetUser(context);

        var serviceName = BuildServiceName(context);

        var requestBodyRequired =
            HttpMethods.IsPost(method) ||
            HttpMethods.IsPut(method) ||
            HttpMethods.IsPatch(method);

        // ---- Response capture (optional optimization) ----
        var originalBodyStream = context.Response.Body;
        await using var tempStream = new MemoryStream();
        context.Response.Body = tempStream;


        Exception? pipelineException = null;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred while processing the request.");

            pipelineException = ex;
            executionContext.AddException(ex);

            // Important: restore response body to write exception result
            context.Response.Body = originalBodyStream;
            await WriteErrorResponse(context, ex);

            //throw;
        }
        finally
        {
            sw.Stop();

            // restore response stream (always)
            context.Response.Body = originalBodyStream;

            var statusCode = context.Response.StatusCode;
            var isError = pipelineException != null || statusCode >= 400;

            string? requestBody = null;
            string? responseBody = null;

            context.Request.EnableBuffering();

            // Only read request body if:
            // - request can have a body
            // - AND we have an error
            if (requestBodyRequired && isError && IsLoggableRequestContentType(context))
            {
                requestBody = await SafeReadRequestBody(context);
            }

            // Only read response body if error
            if (isError && IsLoggableRequestContentType(context))
            {
                responseBody = await SafeReadResponseBody(tempStream, MaxResponseBodyChars);
            }

            // ---- Always copy buffered response to client ----
            tempStream.Position = 0;
            await tempStream.CopyToAsync(originalBodyStream);

            try
            { 
                await PublishLogs(
                context,
                serviceName,
                tenant,
                userId,
                fullUrl,
                method,
                traceId,
                requestBody,
                responseBody,
                sw.ElapsedMilliseconds
                ); 
            }
            catch (Exception ex)
            {
                // Logging must never crash API
                logger.LogError(ex, "Failed to publish logs.");
            }
        }
    }

    private async Task PublishLogs(
        HttpContext context,
        string serviceName,
        Guid? tenantId,
        Guid? userId,
        string fullUrl,
        string method,
        string traceId,
        string? requestBody,
        string? responseBody,
        long runtimeMs)
    {
        var statusCode = context.Response.StatusCode;
        var now = DateTimeOffset.UtcNow;

        // 1) Exceptions
        if (executionContext.HasException)
        {
            foreach (var ex in executionContext.Exceptions)
            {
                var exLog = new LogException(
                    ServiceName: serviceName,
                    TenantId: tenantId,
                    RequestPath: fullUrl,
                    HttpMethod: method,
                    RequestJson: requestBody,
                    TraceIdentifier: traceId,
                    StatusCode: statusCode,
                    ErrorMessage: ex.Message,
                    StackTrace: ex.StackTrace,
                    UserId: userId,
                    Timestamp: now,
                    RuntimeMs: runtimeMs
                );

                await publisher.PublishAsync(exLog);
            }
        }

        // 2) DB Operations
        if (executionContext.HasDatabaseOperations)
        {
            foreach (var dbOp in executionContext.DatabaseOperations)
            {
                var dbLog = new LogDatabaseOperation(
                    ServiceName: serviceName,
                    TenantId: tenantId,
                    EntityName: dbOp.EntityName,
                    EntityId: dbOp.EntityId,
                    Action: dbOp.Operation.ToUpperInvariant(),
                    RequestPath: fullUrl,
                    HttpMethod: method,
                    TraceIdentifier: traceId,
                    OldSnapshot: dbOp.OldSnapshot,
                    NewSnapshot: dbOp.NewSnapshot,
                    UserId: userId,
                    Timestamp: dbOp.Timestamp,
                    IsError: dbOp.IsError,
                    ErrorMessage: dbOp.ErrorMessage,
                    RuntimeMs: dbOp.RuntimeMs,
                    RowsAffected: dbOp.RowsAffected,
                    SqlQuery: dbOp.SqlQuery
                );

                await publisher.PublishAsync(dbLog);
            }
        }

        // 3) Request log (always)
        var reqLog = new LogRequest(
            ServiceName: serviceName,
            TenantId: tenantId,
            RequestPath: fullUrl,
            HttpMethod: method,
            RequestJson: requestBody,
            ResponseJson: responseBody,
            StatusCode: statusCode,
            UserId: userId,
            Timestamp: now,
            RuntimeMs: runtimeMs
        );

        await publisher.PublishAsync(reqLog);
    }

    // -------------------------------
    // Body reading helpers (safe + only when needed)
    // -------------------------------

    private static async Task<string?> SafeReadRequestBody(HttpContext context, int maxChars)
    {
        // This is CRITICAL. Without this, reading request body can break downstream.
        context.Request.EnableBuffering();

        // If no body
        if (context.Request.ContentLength is null or 0)
            return null;

        context.Request.Body.Position = 0;

        using var reader = new StreamReader(
            context.Request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 4096,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        // Reset so controllers can still read it (super important)
        context.Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
            return null;

        return Truncate(body, maxChars);
    }

    private static async Task<string?> SafeReadResponseBody(MemoryStream responseBuffer, int maxChars)
    {
        if (responseBuffer.Length == 0)
            return null;

        responseBuffer.Position = 0;

        using var reader = new StreamReader(
            responseBuffer,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 4096,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        // Reset so we can copy it to original stream afterwards
        responseBuffer.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
            return null;

        return Truncate(body, maxChars);
    }

    // -------------------------------
    // Filters
    // -------------------------------

    private static bool ShouldSkip(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        foreach (var prefix in SkipPathPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool IsLoggableRequestContentType(HttpContext context)
    {
        var ct = context.Request.ContentType;
        if (string.IsNullOrWhiteSpace(ct))
            return true; // assume json-ish

        ct = ct.ToLowerInvariant();

        return ct.Contains("application/json")
            || ct.Contains("text/")
            || ct.Contains("application/problem+json")
            || ct.Contains("application/xml");
    }

    private static bool IsLoggableResponseContentType(HttpContext context)
    {
        var ct = context.Response.ContentType;
        if (string.IsNullOrWhiteSpace(ct))
            return true;

        ct = ct.ToLowerInvariant();

        return ct.Contains("application/json")
            || ct.Contains("text/")
            || ct.Contains("application/problem+json")
            || ct.Contains("application/xml");
    }

    private static string Truncate(string value, int maxChars)
    {
        if (value.Length <= maxChars)
            return value;

        return value[..maxChars] + $"... [TRUNCATED length={value.Length}]";
    }

    // -------------------------------
    // Utilities
    // -------------------------------

    private string BuildServiceName(HttpContext context)
    {
        var name = serviceInfoOptions.Name ?? context.Request.Host.ToString();

        if (!string.IsNullOrWhiteSpace(serviceInfoOptions.Version))
            name += $"@{serviceInfoOptions.Version}";

        return name;
    }

    private static Guid? GetUser(HttpContext context)
    {
        _ = Guid.TryParse(context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
        return userId == Guid.Empty ? null : userId;
    }

    private static async Task<string?> SafeReadRequestBody(HttpContext context)
    {
        // ensures you can read the body multiple times
        context.Request.EnableBuffering();

        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        context.Request.Body.Position = 0;
        return string.IsNullOrWhiteSpace(body) ? null : body;
    }

    private static async Task<string?> ReadStreamAsString(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var text = await reader.ReadToEndAsync();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static async Task WriteErrorResponse(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new
        {
            success = false,
            status = 500,
            error = ex.Message,
            exception = ex.GetType().Name,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
