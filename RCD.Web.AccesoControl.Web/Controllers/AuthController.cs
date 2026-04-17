using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;

namespace RCD.Web.AccesoControl.Web.Controllers;

[ApiController]
[Route("api/web/accesocontrol/[controller]")]
[ApiExplorerSettings(GroupName = "web-accesocontrol")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("guardia/login")]
    public async Task<IActionResult> LoginGuardia(LoginRequest request)
    {
        var result = await _auth.LoginGuardiaAsync(request);
        return result is null ? Unauthorized("Credenciales inválidas") : Ok(result);
    }

    [HttpPost("admin/login")]
    public async Task<IActionResult> LoginAdmin(LoginRequest request)
    {
        var result = await _auth.LoginAdminAsync(request);
        return result is null ? Unauthorized("Credenciales inválidas") : Ok(result);
    }

#if DEBUG
 
    /// Genera un hash BCrypt para insertar en la BD.
    /// Remover en producción o proteger con autenticación.

    [HttpGet("dev/hash")]
    public IActionResult GenerarHash([FromQuery] string pwd)
    {
        if (string.IsNullOrWhiteSpace(pwd)) return BadRequest();
        return Ok(new { hash = BCrypt.Net.BCrypt.HashPassword(pwd) });
    }
#endif
}