using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infrastructure.Middlewares.Extensions;

public static class ApiLoggingMiddlewareExtensions
{
    public static IServiceCollection AddApiLogging(this IServiceCollection services)
    {
        // Register middleware as transient/scoped depending on need
        services.AddScoped<ApiLoggingMiddleware>();
        return services;
    }

    public static IApplicationBuilder UseApiLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiLoggingMiddleware>();
    }
}
