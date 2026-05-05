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
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> QrLogin(
    [FromBody] QrLoginRequest request,
    CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.QrCode))
            return BadRequest(ApiResponse<LoginResponse>.Fail("El código QR es requerido."));

        try
        {
            var response = await _authService.LoginConQrAsync(request.QrCode, ct);
            return Ok(ApiResponse<LoginResponse>.Ok(response, "Login exitoso."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail(ex.Message));
        }
    }
}
