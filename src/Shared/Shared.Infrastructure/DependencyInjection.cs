using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Context;
using Shared.Abstractions.Security.Keycloak.Services;
using Shared.Abstractions.UnitOfWork;
using Shared.Infrastructure.Context;
using Shared.Infrastructure.DI;
using Shared.Infrastructure.Security.Keycloak.Services;
using Shared.Kernel.Abstractions.Events;
using Shared.Persistence;
using Shared.Persistence.Interceptors;
using System.Reflection;

namespace Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        //services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DbOperationInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        //services.AddScoped<IRequestExecutionContext, RequestExecutionContext>();

        return services;
    }

    public static IServiceCollection AddSharedInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructureServices();

        return services;
    }

    public static IServiceCollection AddDefaultServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            ServiceRegistrationHelper.RegisterServices(services, assembly);
        }

        return services;
    }

    public static IServiceCollection AddUnitOfWork<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<ITransactionalUnitOfWork>(provider =>
        {
            var dbContext = provider.GetRequiredService<TDbContext>();
            var dispatcher = provider.GetRequiredService<IDomainEventDispatcher>();
            return new UnitOfWork<TDbContext>(dbContext, dispatcher);
        });

        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<ITransactionalUnitOfWork>());

        return services;
    }
}
