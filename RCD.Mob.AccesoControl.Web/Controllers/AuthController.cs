using Microsoft.AspNetCore.Mvc;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;
using System.Runtime.InteropServices;

namespace RCD.Mob.AccesoControl.Web.Controllers;

[ApiController]
[Route("api/mob/accesocontrol/[controller]")]
[ApiExplorerSettings(GroupName = "mobile-accesocontrol")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("guardia/login")]
    public async Task<IActionResult> LoginGuardia(LoginRequest request)
    {
        var result = await _auth.LoginGuardiaAsync(request);
        return result is null
            ? Unauthorized("Credenciales inválidas")
            : Ok(result);
    }
}