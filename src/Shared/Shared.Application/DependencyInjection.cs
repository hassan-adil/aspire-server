using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Application.Behaviors;
using System.Reflection;

namespace Shared.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedApplicationServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Register FluentValidation validators from shared and provided assemblies
        services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);
        services.AddValidatorsFromAssemblies(assemblies);

        // Register MediatR with behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
            //cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            //cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });

        return services;
    }

    public static void AddApplicationServices(this IHostApplicationBuilder builder, params Assembly[] assemblies)
    {
        builder.Services.AddSharedApplicationServices(assemblies);
    }
}
