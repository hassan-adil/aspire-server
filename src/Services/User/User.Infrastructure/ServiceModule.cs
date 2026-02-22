using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shared.Abstractions.Data;
using Shared.Infrastructure;
using Shared.Infrastructure.Logging.Extensions;
using Shared.Persistence.Data;
using System.Reflection;

namespace User.Infrastructure;

public static class ServiceModule
{
    public static IServiceCollection AddUserInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add shared infrastructure services with Keycloak integration
        services.AddSharedInfrastructureServices(configuration);

        // Add DbContext
        services.AddDbContext<UserDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("user-db");

            // Create an NpgsqlDataSource with dynamic JSON enabled
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            var saveChangesInterceptor = serviceProvider.GetServices<ISaveChangesInterceptor>();

            options.UseNpgsql(dataSource);
            options.AddInterceptors(saveChangesInterceptor);
        });

        // Add UnitOfWork
        services.AddUnitOfWork<UserDbContext>();

        services.AddSharedLoggingWithOutbox();

        services.AddScoped<IDataSeedingService, DataSeedingService>();

        return services;
    }
}
