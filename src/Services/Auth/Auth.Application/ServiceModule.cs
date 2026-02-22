using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Refit;
using Shared.Application;
using Shared.Contracts.Options;
using Shared.Infrastructure.Middlewares.Extensions;
using Shared.Infrastructure.Security.Keycloak.Models.Configurations;
using Shared.Infrastructure.Security.Keycloak.Services;
using System.Reflection;

namespace Auth.Application;

public static class ServiceModule
{
    public static void AddAuthApplicationServices(this IHostApplicationBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add shared application services
        builder.AddApplicationServices(assembly);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddApiLogging();

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("redis");
        });

        builder.Services
            .AddRefitClient<IKeycloakAdminApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(builder.Configuration["Keycloak:ServerUrl"]!);
            });

        builder.Services.Configure<ServiceInfoOptions>(
            builder.Configuration.GetSection("Service"));

        builder.Services.Configure<KeycloakConfiguration>(
            builder.Configuration.GetSection("Keycloak"));

        // Add services to the container.
        builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri("rabbitmq://localhost:5672"));
            });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Auth API",
                Version = "v1",
                Description = "API responsible for providing auth services to all microservices."
            });

            // Add JWT Bearer authentication to Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // Tenant Id header
            c.AddSecurityDefinition("Tenant", new OpenApiSecurityScheme
            {
                Name = "X-Tenant-Id",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Tenant identifier"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Tenant"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}