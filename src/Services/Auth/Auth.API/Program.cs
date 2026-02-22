using Auth.API.Extensions;
using Auth.Application;
using Auth.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using ServiceDefaults;
//using Shared.Infrastructure;
using Shared.Infrastructure.MultiTenancy;
using Shared.Kernel.Abstractions.Events;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDefaultServices(
    typeof(TenantResolver).Assembly,
    typeof(IDomainEventDispatcher).Assembly,
    typeof(InfrastructureAssemblyMarker).Assembly);

builder.AddAuthApplicationServices();
builder.Services.AddAuthInfrastructureServices(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references in JSON serialization
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

app.ConfigurePipeline();

app.MapPost("/auth/token", (HttpResponse ctx) =>
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes("this_is_a_very_long_super_secret_key_12345!!!")
    );

    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim("value", "test123")
    };

    var token = new JwtSecurityToken(
        issuer: "TestIssuer",
        audience: "TestAudience",
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30),
        signingCredentials: creds
    );

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

    ctx.Cookies.Append("jwt", jwt, new CookieOptions
    {
        HttpOnly = true,
        Secure = false,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddHours(1)
    });

    ctx.Headers.Append("X-Debug-JWT", jwt);

    return Results.Ok(new { message = "Logged in" });
})
.WithOpenApi(o =>
{
    Console.WriteLine("WithOpenApi executed");

    o.Responses["200"] = new Microsoft.OpenApi.Models.OpenApiResponse
    {
        Description = "Returns auth cookie",
        Headers =
        {
            ["Set-Cookie"] = new Microsoft.OpenApi.Models.OpenApiHeader
            {
                Description = "JWT Cookie",
                Schema = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" }
            }
        }
    };
    return o;
});

app.MapGet("/exception/test", () =>
{
    throw new Exception("Test exception for logging middleware");
});

app.Run();

