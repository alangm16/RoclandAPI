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

        try
        {
            await _tokenService.RegistrarAsync(request);
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
}
