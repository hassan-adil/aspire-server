using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Context;

namespace User.API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(IRequestExecutionContext executionContext, 
                                       ILogger<WeatherForecastController> logger) : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
    
    [HttpGet("DbLogTest")]
    public IActionResult DbLogTest()
    {
        var now = DateTimeOffset.UtcNow;

        var entId = Guid.NewGuid().ToString();

        var firstObject = new Dictionary<string, object>
        {
            { "Name", "Old Name" },
            { "Description", "Old Description" }
        };

        var log = new DatabaseOperation(
                Operation: "POST",
                EntityName: "TestEntity",
                EntityId: entId,
                SqlQuery: "",
                OldSnapshot: null,
                NewSnapshot: firstObject,
                now,
                100,
                0,
                false,
                null
            );

        executionContext.AddDatabaseOperation(log);

        logger.LogInformation("LogDatabaseOperation sent to queue.");

        var secondObject = new Dictionary<string, object>
        {
            ["Name"] = "New Name",
            ["Description"] = "New Description",
        };

        var log2 = new DatabaseOperation(
                Operation: "PUT",
                EntityName: "TestEntity",
                EntityId: entId,
                SqlQuery: "",
                OldSnapshot: firstObject,
                NewSnapshot: secondObject,
                now,
                100,
                0,
                false,
                null
            );

        executionContext.AddDatabaseOperation(log2);

        logger.LogInformation("LogDatabaseOperation sent to queue.");

        return Ok();
    }
}
