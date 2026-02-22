using Shared.Abstractions.MultiTenancy;
using Shared.Infrastructure.Middlewares.Extensions;

namespace Auth.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        //app.UseCors();

        app.UseRouting();

        // Add authentication and authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Multi-tenancy Middleware
        app.Use(async (context, next) =>
        {
            var tenantResolver = context.RequestServices.GetRequiredService<ITenantResolver>();
            var tenantId = tenantResolver.ResolveTenant();

            if (tenantId.HasValue)
            {
                context.Items["TenantId"] = tenantId.Value;
            }

            await next();
        });

        app.UseApiLogging();
        app.MapControllers();

        return app;
    }
}
