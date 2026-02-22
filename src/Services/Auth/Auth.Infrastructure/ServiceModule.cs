using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;
using Shared.Infrastructure.Logging.Extensions;

namespace Auth.Infrastructure;

public static class ServiceModule
{
    public static IServiceCollection AddAuthInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add shared infrastructure services
        services.AddSharedInfrastructureServices(configuration);

        //services.AddSharedLoggingWithOutbox();
        services.AddSharedLoggingWithoutDb();

        return services;
    }
}

