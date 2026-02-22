using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.Data;
using Shared.Abstractions.MultiTenancy;
using Shared.Infrastructure.Middlewares.Extensions;
using User.Infrastructure;

namespace User.API.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();

            // Initialize User database
            var userDb = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            await userDb.Database.MigrateAsync();
            app.Logger.LogInformation("User database initialized successfully");

            // Run automatic data seeding if required
            await RunAutomaticSeedingAsync(app, scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while initializing the User database");

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

    private static async Task RunAutomaticSeedingAsync(WebApplication app, IServiceProvider scopedServiceProvider)
    {
        try
        {
            // Get the seeding service from the scoped provider (not root provider)
            var seedingService = scopedServiceProvider.GetRequiredService<IDataSeedingService>();

            await seedingService.SeedAllAsync();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Error during automatic data seeding");

            // In development, log and continue
            if (app.Environment.IsDevelopment())
            {
                app.Logger.LogWarning("Continuing despite seeding error in development environment");
            }
            else
            {
                // In production, seeding failures should not prevent startup
                app.Logger.LogError("Seeding failed in production environment. Manual intervention may be required.");
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

        app.UseApiLogging();
        app.MapControllers();

        return app;
    }
}
