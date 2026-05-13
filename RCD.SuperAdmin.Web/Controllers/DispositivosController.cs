using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Dispositivos;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize]
public class DispositivosController : ControllerBase
{
    private readonly ITokenDispositivoService _tokenService;

    public DispositivosController(ITokenDispositivoService tokenService)
    {
        _tokenService = tokenService;
    }

    /// Registra o actualiza el token FCM/device del usuario autenticado
    /// en el proyecto y plataforma especificados por el JWT.
    [HttpPost("token")]
    public async Task<IActionResult> RegistrarToken([FromBody] RegistrarDispositivoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FcmToken) && string.IsNullOrWhiteSpace(request.DeviceToken))
            return BadRequest(new { mensaje = "Debe proporcionar al menos FcmToken o DeviceToken." });

        // Obtener User-Agent del request
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        if (string.IsNullOrEmpty(userAgent))
            userAgent = "Desconocido";

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        // Si el frontend no envió DispositivoInfo, usamos el User-Agent
        if (string.IsNullOrWhiteSpace(request.DispositivoInfo))
            request = request with { DispositivoInfo = userAgent };

        try
        {
            await _tokenService.RegistrarAsync(request, userAgent);
            return Ok(new { mensaje = "Token registrado exitosamente." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("usuarios/{usuarioId:int}")]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> ObtenerPorUsuario(
    int usuarioId,
    [FromQuery] int pagina = 1,
    [FromQuery] int tamanoPagina = 10)
    {
        try
        {
            var resultado = await _tokenService.ObtenerPorUsuarioAsync(usuarioId, pagina, tamanoPagina);
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Revoca (desactiva) un dispositivo específico.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "SuperAdmin, Admin")] // Solo administradores pueden revocar dispositivos
    public async Task<IActionResult> RevocarDispositivo(int id)
    {
        try
        {
            await _tokenService.RevocarDispositivoAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
