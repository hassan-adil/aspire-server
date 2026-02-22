using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shared.Infrastructure;
using System.Reflection;

namespace Logger.Infrastructure;

public static class ServiceModule
{
    public static IServiceCollection AddLoggerInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add shared infrastructure services with Keycloak integration
        services.AddSharedInfrastructureServices(configuration);

        var connectionString = configuration.GetConnectionString("logger-db");

        // Create an NpgsqlDataSource with dynamic JSON enabled
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        // Add DbContext
        services.AddDbContext<LoggerDbContext>(options =>
        {
            options.UseNpgsql(dataSource);
        });

        // Add UnitOfWork
        services.AddUnitOfWork<LoggerDbContext>();

        return services;
    }
}
