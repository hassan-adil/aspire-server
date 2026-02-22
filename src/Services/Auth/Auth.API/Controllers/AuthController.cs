using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Security.Keycloak.Services;

namespace Auth.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController(IKeycloakAdminTokenProvider adminTokenProvider) : ControllerBase
{
    [HttpGet("TestAdminToken")]
    public async Task<IActionResult> TestAdminToken()
    {
        return Ok(await adminTokenProvider.GenerateAdminAccessTokenAsync());
    }
}
