using Logger.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Logger.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LogsController(IRequestLogRepository requestLogRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? httpMethod)
    {
        var logs = await requestLogRepository.ReadLogs(httpMethod: httpMethod);

        return Ok(logs);
    }
}
