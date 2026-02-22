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
using Shared.Infrastructure.ServicesInterCommunication.Extensions;
using System;
using System.Reflection;
using User.Infrastructure;

namespace User.Application;

public static class ServiceModule
{
    public static void AddUserApplicationServices(this IHostApplicationBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add shared application services
        builder.AddApplicationServices(assembly);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddApiLogging();
        builder.Services.AddServiceRegistry(builder.Configuration);

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
            x.SetKebabCaseEndpointNameFormatter();

            x.AddEntityFrameworkOutbox<UserDbContext>(o =>
            {
                o.UsePostgres();

                // THIS is the magic: publish goes to outbox first
                o.UseBusOutbox();

                // tuning
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.DuplicateDetectionWindow = TimeSpan.FromMinutes(30);
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri("rabbitmq://localhost:5672"));
                //cfg.Host(builder.Configuration.GetConnectionString("messaging"));
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
