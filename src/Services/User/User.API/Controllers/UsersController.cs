using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Controller;
using User.Application.CQRS.Users.Queries;

namespace User.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(ISender mediator) : BaseApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ReadAllUsersQuery query) => await SendQuery(query);

    [HttpGet("ReadUserById")]
    public async Task<IActionResult> GetById([FromQuery] ReadUserByIdQuery query) => await SendQuery(query);    
}
