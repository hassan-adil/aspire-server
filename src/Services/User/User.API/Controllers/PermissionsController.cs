using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Controller;
using User.Application.CQRS.Permissions.Commands;

namespace User.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class PermissionsController(ISender mediator) : BaseApiController(mediator)
{
    [HttpPost("InsertPermission")]
    public async Task<IActionResult> InsertPermission([FromBody] InsertPermissionCommand command) => await SendCommand(command);
}
