using Logger.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.MultiTenancy;

namespace Logger.API.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();

            // Initialize Logger database
            var loggerDb = scope.ServiceProvider.GetRequiredService<LoggerDbContext>();
            await loggerDb.Database.MigrateAsync();
            app.Logger.LogInformation("Logger database initialized successfully");

            // Run automatic data seeding if required
            //await RunAutomaticSeedingAsync(app, scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while initializing the Logger database");

            // In development, you might want to continue anyway
            if (app.Environment.IsDevelopment())
            {
                app.Logger.LogWarning("Continuing despite database initialization error in development environment");
            }
            else
            {
                throw;
            }
        }
    }

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

        app.MapControllers();
        return app;
    }
}
