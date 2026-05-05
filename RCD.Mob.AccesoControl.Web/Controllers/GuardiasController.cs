using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;
using RCD.Web.AccesoControl.Infrastructure.Hubs;
using RCD.Web.AccesoControl.Infrastructure.Services;
using System.Runtime.InteropServices;

namespace RCD.Mob.AccesoControl.Web.Controllers;

[ApiController]
[Route("api/mob/accesocontrol/[controller]")]
[ApiExplorerSettings(GroupName = "mobile-accesocontrol")]
[Authorize(Policy = "AccesoControlMobilePolicy")]
public class GuardiasController : ControllerBase
{
    private readonly IAccesoService _acceso;
    private readonly IHubContext<AccesoControlHub> _hub;
    private readonly IAuthService _authService;

    public GuardiasController(IAccesoService acceso, IHubContext<AccesoControlHub> hub, IAuthService authService)
    {
        _acceso = acceso;
        _hub = hub;
        _authService = authService;
    }

    [HttpGet("solicitudes")]
    public async Task<IActionResult> ObtenerSolicitudes()
        => Ok(await _acceso.ObtenerSolicitudesPendientesAsync());

    [HttpGet("solicitudes/{id}")]
    public async Task<IActionResult> ObtenerSolicitudPorId(int id)
    {
        var result = await _acceso.ObtenerSolicitudPorIdAsync(id);
        return result is null
            ? NotFound("La solicitud no existe o ya fue procesada.")
            : Ok(result);
    }

    [HttpGet("activos")]
    public async Task<IActionResult> ObtenerActivos()
        => Ok(await _acceso.ObtenerAccesosActivosAsync());

    [HttpGet("gafetes/disponibles")]
    public async Task<IActionResult> ObtenerGafetesDisponibles()
        => Ok(await _acceso.ObtenerGafetesDisponiblesAsync());

    // ── MÉTODO DE AYUDA (Agrégalo a tu controlador) ──
    private async Task<int?> ObtenerPerfilIdAsync()
    {
        var superAdminIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("id")?.Value;

        if (!int.TryParse(superAdminIdStr, out int superAdminId)) return null;

        var perfil = await _authService.ObtenerPerfilPorSuperAdminIdAsync(superAdminId);
        return perfil?.Activo == true ? perfil.Id : null;
    }

    // ── 1. FIX: Aprobar ──
    [HttpPost("aprobar")]
    public async Task<IActionResult> Aprobar(AprobarSolicitudRequest request)
    {
        var perfilId = await ObtenerPerfilIdAsync();
        if (perfilId == null) return Unauthorized("No tienes un perfil activo.");

        var ok = await _acceso.AprobarSolicitudAsync(request, perfilId.Value);
        if (ok)
        {
            await _hub.Clients.Group("Guardias").SendAsync("SolicitudResuelta", new { solicitudId = request.SolicitudId, estado = "Aprobada" });
            return Ok();
        }
        return BadRequest("No se pudo aprobar la solicitud.");
    }

    // ── 2. FIX: Rechazar ──
    [HttpPost("rechazar")]
    public async Task<IActionResult> Rechazar(RechazarSolicitudRequest request)
    {
        var perfilId = await ObtenerPerfilIdAsync();
        if (perfilId == null) return Unauthorized("No tienes un perfil activo.");

        var ok = await _acceso.RechazarSolicitudAsync(request, perfilId.Value);
        if (ok)
        {
            await _hub.Clients.Group("Guardias").SendAsync("SolicitudResuelta", new { solicitudId = request.SolicitudId, estado = "Rechazada" });
            return Ok();
        }
        return BadRequest("No se pudo rechazar la solicitud.");
    }

    // ── 3. FIX: Marcar Salida ──
    [HttpPost("salida")]
    public async Task<IActionResult> MarcarSalida(MarcarSalidaRequest request)
    {
        var perfilId = await ObtenerPerfilIdAsync();
        if (perfilId == null) return Unauthorized("No tienes un perfil activo.");

        var ok = await _acceso.MarcarSalidaAsync(request, perfilId.Value);
        if (ok)
        {
            await _hub.Clients.Group("Guardias").SendAsync("SalidaRegistrada", request.RegistroId);
            return Ok();
        }
        return BadRequest("No se pudo registrar la salida.");
    }

    // ── 4. FIX: Guardar FCM Token ──
    [HttpPost("fcm-token")]
    public async Task<IActionResult> RegistrarFcmToken([FromBody] RegistrarFcmTokenRequest request)
    {
        var perfilId = await ObtenerPerfilIdAsync();
        if (perfilId == null) return Unauthorized("No tienes un perfil activo.");

        // Usamos perfilId.Value en lugar de request.GuardiaId
        var ok = await _acceso.GuardarFcmTokenAsync(perfilId.Value, request.FcmToken);
        return ok ? Ok() : BadRequest("No se pudo guardar el token.");
    }
}