using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Auth;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Infrastructure.Services;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await authService.LoginAsync(request, ip);
        return result is null
            ? Unauthorized(new { mensaje = "Credenciales incorrectas o cuenta bloqueada." })
            : Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await authService.RefreshAsync(request.RefreshToken, ip);
        return result is null
            ? Unauthorized(new { mensaje = "Refresh token inválido o expirado." })
            : Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await authService.RevocarRefreshTokenAsync(request.RefreshToken);
        return NoContent();
    }

    [HttpPost("qr-login")]
    public async Task<IActionResult> QrLogin([FromBody] QrLoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.QrCode))
            return BadRequest(new { mensaje = "El código QR es requerido." });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        // Usamos authService (sin guion bajo) y le pasamos los datos para el Log
        var result = await authService.LoginConQrAsync(request.QrCode, ip, "Mobile", ct);

        return result is null
            ? Unauthorized(new { mensaje = "Código QR inválido, usuario inactivo o cuenta bloqueada." })
            : Ok(result);
    }

    [HttpGet("descubrir-proyectos")]
    [AllowAnonymous] 
    public async Task<IActionResult> DescubrirProyectos([FromQuery] string identificador, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(identificador))
            return BadRequest(new { mensaje = "El identificador (usuario o correo) es requerido." });

        var proyectos = await authService.DescubrirProyectosAsync(identificador, ct);

        return Ok(proyectos);
    }
}
