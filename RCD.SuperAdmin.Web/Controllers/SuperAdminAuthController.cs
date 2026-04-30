using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Application.DTOs;


namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
public class SuperAdminAuthController(ISuperAdminAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (result is null)
            return Unauthorized(new { mensaje = "Credenciales incorrectas." });

        return Ok(result);
    }
}
