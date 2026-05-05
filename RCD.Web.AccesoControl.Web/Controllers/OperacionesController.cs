using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RCD.Web.AccesoControl.Infrastructure.Hubs;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;
using System.Security.Claims;

namespace RCD.Web.AccesoControl.Web.Controllers;

[ApiController]
// ── FIX 1: Renombramos la ruta lógicamente (opcional, pero recomendado)
[Route("api/web/accesocontrol/operaciones")]
[ApiExplorerSettings(GroupName = "web-accesocontrol")]
[Authorize(Policy = "AccesoControlWebPolicy")]
public class OperacionesController : ControllerBase
{
    private readonly IAccesoService _acceso;
    private readonly IHubContext<AccesoControlHub> _hubContext;
    private readonly IAuthService _authService; // ── FIX 2: Inyectamos el AuthService

    public OperacionesController(IAccesoService acceso, IHubContext<AccesoControlHub> hubContext, IAuthService authService)
    {
        _acceso = acceso;
        _hubContext = hubContext;
        _authService = authService;
    }

    // ── Helper para extraer el PerfilId del Token ──
    private async Task<int?> ObtenerPerfilIdAsync()
    {
        var superAdminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
        if (!int.TryParse(superAdminIdStr, out int superAdminId)) return null;

        var perfil = await _authService.ObtenerPerfilPorSuperAdminIdAsync(superAdminId);
        return perfil?.Activo == true ? perfil.Id : null;
    }

    // ── MÉTODOS DE LECTURA (Siguen igual) ──

    [HttpGet("solicitudes")]
    public async Task<IActionResult> ObtenerSolicitudes() => Ok(await _acceso.ObtenerSolicitudesPendientesAsync());

    [HttpGet("activos")]
    public async Task<IActionResult> ObtenerActivos() => Ok(await _acceso.ObtenerAccesosActivosAsync());

    [HttpGet("activosZona")]
    public async Task<IActionResult> ObtenerActivosZona() => Ok(await _acceso.ObtenerAccesosActivosZonaAsync());

    [HttpGet("gafetes/disponibles")]
    public async Task<IActionResult> ObtenerGafetesDisponibles() => Ok(await _acceso.ObtenerGafetesDisponiblesAsync());

    [HttpGet("solicitudes/{id}")]
    public async Task<IActionResult> ObtenerSolicitudPorId(int id)
    {
        var result = await _acceso.ObtenerSolicitudPorIdAsync(id);
        return result == null ? NotFound("La solicitud no existe o ya fue procesada.") : Ok(result);
    }

    // ── MÉTODOS DE ACCIÓN (Corregidos con el PerfilId) ──

    [HttpPost("aprobar")]
    public async Task<IActionResult> Aprobar(AprobarSolicitudRequest request)
    {
        var perfilId = await ObtenerPerfilIdAsync();
        if (perfilId == null) return Unauthorized("No se pudo identificar tu perfil activo.");

        // ── FIX 3: Pasamos el perfilId
        var ok = await _acceso.AprobarSolicitudAsync(request, perfilId.Value);
        if (ok)
        {
            // Notificamos a todos (SignalR). Ya no mandamos "Guardias", mandamos el evento genérico.
            await _hubContext.Clients.All.SendAsync("SolicitudResuelta", new { solicitudId = request.SolicitudId, estado = "Aprobada" });
            return Ok();
        }
        return BadRequest("No se pudo aprobar la solicitud.");
    }

    [HttpPost("rechazar")]
    public async Task<IActionResult> Rechazar(RechazarSolicitudRequest request)
    {
        var perfilId = await ObtenerPerfilIdAsync();
        if (perfilId == null) return Unauthorized("No se pudo identificar tu perfil activo.");

        // ── FIX 4: Pasamos el perfilId
        var ok = await _acceso.RechazarSolicitudAsync(request, perfilId.Value);
        if (ok)
        {
            await _hubContext.Clients.All.SendAsync("SolicitudResuelta", new { solicitudId = request.SolicitudId, estado = "Rechazada" });
            return Ok();
        }
        return BadRequest("No se pudo rechazar la solicitud.");
    }

    [HttpPost("salida")]
    public async Task<IActionResult> MarcarSalida(MarcarSalidaRequest request)
    {
        var perfilId = await ObtenerPerfilIdAsync();
        if (perfilId == null) return Unauthorized("No se pudo identificar tu perfil activo.");

        // ── FIX 5: Pasamos el perfilId
        var ok = await _acceso.MarcarSalidaAsync(request, perfilId.Value);
        if (ok)
        {
            await _hubContext.Clients.All.SendAsync("SalidaRegistrada", request.RegistroId);
            return Ok();
        }
        return BadRequest("No se pudo registrar la salida.");
    }

    [HttpPost("fcm-token")]
    public async Task<IActionResult> RegistrarFcmToken([FromBody] RegistrarFcmTokenRequest request)
    {
        var perfilId = await ObtenerPerfilIdAsync();
        if (perfilId == null) return Unauthorized("No se pudo identificar tu perfil activo.");

        // ── FIX 6: Usamos el perfilId en lugar del GuardiaId del request
        var ok = await _acceso.GuardarFcmTokenAsync(perfilId.Value, request.FcmToken);
        return ok ? Ok() : BadRequest("No se pudo guardar el token.");
    }
}