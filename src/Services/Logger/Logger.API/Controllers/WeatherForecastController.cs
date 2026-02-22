using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Logger.API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger, IPublishEndpoint publishEndpoint) : ControllerBase
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

    [HttpPost("publish")]
    public async Task<IActionResult> Publish([FromQuery] string text = "Hello RabbitMQ!")
    {
        await publishEndpoint.Publish(new Application.Consumers.TestMessage(text));

        await publishEndpoint.Publish(new Shared.Contracts.Logging.LogRequest(
            ServiceName: "Logger.API",
            TenantId: Guid.NewGuid(),
            RequestPath: "/weatherforecast/publish",
            HttpMethod: "POST",
            RequestJson: $"{{ \"text\": \"{text}\" }}",
            ResponseJson: $"{{ \"message\": \"Published message: {text}\" }}",
            StatusCode: 200,
            UserId: Guid.NewGuid(),
            Timestamp: DateTime.UtcNow,
            RuntimeMs: 0
        ));

        return Ok($"Published message: {text}");
    }
}
