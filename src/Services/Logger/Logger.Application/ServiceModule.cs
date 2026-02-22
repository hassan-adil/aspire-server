using Logger.Application.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Shared.Application;
using Shared.Kernel.Types;
using System.Reflection;
using static MassTransit.Logging.DiagnosticHeaders.Messaging;

namespace Logger.Application;

public static class ServiceModule
{
    public static void AddLoggerApplicationServices(this IHostApplicationBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add shared application services
        builder.AddApplicationServices(assembly);

        //builder.AddRabbitMQClient("messaging");

        builder.Services.AddHttpContextAccessor();

        //builder.Services.AddMassTransit(x =>
        //{
        //    x.AddConsumer<LogRequestConsumer>(cfg =>
        //    {
        //        cfg.UseMessageRetry(r =>
        //        {
        //            r.Interval(5, TimeSpan.FromHours(1));
        //        });
        //    });

        //    x.AddConsumer<LogExceptionConsumer>(cfg =>
        //    {
        //        cfg.UseMessageRetry(r =>
        //        {
        //            r.Interval(5, TimeSpan.FromHours(1));
        //        });
        //    });

        //    x.AddConsumer<LogDatabaseOperationConsumer>(cfg =>
        //    {
        //        cfg.UseMessageRetry(r =>
        //        {
        //            r.Interval(5, TimeSpan.FromHours(1));
        //        });
        //    });

        //    x.SetKebabCaseEndpointNameFormatter();

        //    x.UsingRabbitMq((context, cfg) =>
        //    {
        //        cfg.Host(new Uri("rabbitmq://localhost:5672"));

        //        cfg.ConfigureEndpoints(context);

        //        //cfg.ReceiveEndpoint(MessagingQueue.LogRequest_Queue.ToString(), e =>
        //        //{
        //        //    e.ConfigureConsumer<LogRequestConsumer>(context);
        //        //});

        //        //cfg.ReceiveEndpoint(MessagingQueue.LogException_Queue.ToString(), e =>
        //        //{
        //        //    e.ConfigureConsumer<LogExceptionConsumer>(context);
        //        //});

        //        //cfg.ReceiveEndpoint(MessagingQueue.LogDatabaseOperation_Queue.ToString(), e =>
        //        //{
        //        //    e.ConfigureConsumer<LogDatabaseOperationConsumer>(context);
        //        //});
        //    });
        //});

        //var amqp = new Uri(builder.Configuration.GetConnectionString("messaging")!);

        //var rabbitUri = new UriBuilder(amqp)
        //{
        //    Scheme = "rabbitmq"
        //}.Uri;

        //Console.WriteLine($"RabbitMQ Connection String: {rabbitUri}");

        //builder.Services.AddMassTransit(x =>
        //{
        //    x.AddConsumers(typeof(ServiceModule).Assembly);
        //    x.SetKebabCaseEndpointNameFormatter();

        //    x.UsingRabbitMq((context, cfg) =>
        //    {
        //        cfg.Host(rabbitUri);
        //        cfg.ConfigureEndpoints(context);
        //    });
        //});

        var rabbitUser = "guest";   // or your mqUsername from Aspire
        var rabbitPass = "guest";   // or mqPassword
        var rabbitHost = "localhost"; // host app connecting to container
        var rabbitPort = 5672;       // mapped port from Aspire

        builder.Services.AddMassTransit(x =>
        {
            // Automatically registers all consumers from assembly
            x.AddConsumers(typeof(Logger.Application.ServiceModule).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                // Auto configure endpoints for consumers (DI-resolved)
                cfg.ConfigureEndpoints(context);

                // Explicit persistent queue
                cfg.ReceiveEndpoint("log-request", e =>
                {
                    e.Consumer<Logger.Application.Consumers.LogRequestConsumer>(context);
                    e.Durable = true;       // queue stays in RabbitMQ
                    e.AutoDelete = false;   // don’t delete when app disconnects
                });

                cfg.ReceiveEndpoint("log-database-operation", e =>
                {
                    e.Consumer<Logger.Application.Consumers.LogDatabaseOperationConsumer>(context);
                    e.Durable = true;       // queue stays in RabbitMQ
                    e.AutoDelete = false;   // don’t delete when app disconnects
                });
            });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Logger API",
                Version = "v1",
                Description = "Common logging service API for all microservices."
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
