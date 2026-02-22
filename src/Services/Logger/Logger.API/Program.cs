using Logger.API.Extensions;
using Logger.Application;
using Logger.Infrastructure;
using MassTransit;
using ServiceDefaults;
using Shared.Infrastructure.MultiTenancy;
using Shared.Kernel.Abstractions.Events;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//builder.AddRabbitMQClient("messaging");

//Console.WriteLine("DOTNET_RUNNING_IN_CONTAINER=" +
//    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"));

//var amqp = new Uri(builder.Configuration.GetConnectionString("messaging")!);

//var rabbitUri = new UriBuilder(amqp)
//{
//    Scheme = "rabbitmq"
//}.Uri;

//Console.WriteLine($"RabbitMQ Connection String: {rabbitUri}");

//builder.Services.AddMassTransit(x =>
//{
//    x.AddConsumers(typeof(Logger.Application.ServiceModule).Assembly);
//    x.SetKebabCaseEndpointNameFormatter();

//    x.UsingRabbitMq((context, cfg) =>
//    {
//        cfg.Host(rabbitUri.Host, "/", h =>
//        {
//            h.Username("guest");
//            h.Password("guest");
//        });
//        cfg.ConfigureEndpoints(context);
//    });
//});

//  BELOW WORKING...

//var rabbitUser = "guest";   // or your mqUsername from Aspire
//var rabbitPass = "guest";   // or mqPassword
//var rabbitHost = "localhost"; // host app connecting to container
//var rabbitPort = 5672;       // mapped port from Aspire

//builder.Services.AddMassTransit(x =>
//{
//    // Minimal test consumer
//    //x.AddConsumer<Logger.Application.Consumers.TestMessageConsumer>();

//    // Automatically registers all consumers from assembly
//    x.AddConsumers(typeof(Logger.Application.ServiceModule).Assembly);

//    x.UsingRabbitMq((context, cfg) =>
//    {
//        cfg.Host(rabbitHost, "/", h =>
//        {
//            h.Username(rabbitUser);
//            h.Password(rabbitPass);
//        });

//        // Auto configure endpoints for consumers (DI-resolved)
//        cfg.ConfigureEndpoints(context);

//        //// Explicit persistent queue
//        //cfg.ReceiveEndpoint("test-queue", e =>
//        //{
//        //    e.Consumer<Logger.Application.Consumers.TestMessageConsumer>();
//        //    e.Durable = true;       // queue stays in RabbitMQ
//        //    e.AutoDelete = false;   // don’t delete when app disconnects
//        //});

//        // Explicit persistent queue
//        cfg.ReceiveEndpoint("log-request", e =>
//        {
//            e.Consumer<Logger.Application.Consumers.LogRequestConsumer>(context);
//            e.Durable = true;       // queue stays in RabbitMQ
//            e.AutoDelete = false;   // don’t delete when app disconnects
//        });

//        cfg.ReceiveEndpoint("log-database-operation", e =>
//        {
//            e.Consumer<Logger.Application.Consumers.LogDatabaseOperationConsumer>(context);
//            e.Durable = true;       // queue stays in RabbitMQ
//            e.AutoDelete = false;   // don’t delete when app disconnects
//        });
//    });
//});
// Add services to the container.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDefaultServices(
    typeof(LoggerDbContext).Assembly,
    typeof(TenantResolver).Assembly,
    typeof(IDomainEventDispatcher).Assembly);

builder.AddLoggerApplicationServices();
builder.Services.AddLoggerInfrastructureServices(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references in JSON serialization
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

await app.InitializeDatabaseAsync();

// Configure pipeline
WebApplicationExtensions.ConfigurePipeline(app);

// Publish a test message on startup
var bus = app.Services.GetRequiredService<IBusControl>();
await bus.StartAsync();

await bus.Publish(new Logger.Application.Consumers.TestMessage("Hello RabbitMQ from host!"));

await app.RunAsync();
