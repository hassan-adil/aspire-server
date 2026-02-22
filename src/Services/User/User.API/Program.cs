using ServiceDefaults;
//using Shared.Infrastructure;
using Shared.Infrastructure.MultiTenancy;
using Shared.Kernel.Abstractions.Events;
using System.Text.Json.Serialization;
using User.API.Extensions;
using User.Application;
using User.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDefaultServices(
    typeof(UserDbContext).Assembly,
    typeof(TenantResolver).Assembly,
    typeof(IDomainEventDispatcher).Assembly);

builder.AddUserApplicationServices();
builder.Services.AddUserInfrastructureServices(builder.Configuration);

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

await app.RunAsync();
