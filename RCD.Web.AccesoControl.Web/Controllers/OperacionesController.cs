using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;
using System.Security.Claims;

namespace RCD.Web.AccesoControl.Web.Controllers;

[ApiController]
[Route("api/web/accesocontrol/operaciones")]
[ApiExplorerSettings(GroupName = "web-accesocontrol")]
[Authorize(Policy = "AccesoControlWebPolicy")]
public class OperacionesController : ControllerBase
{
    private readonly IAccesoService _acceso;
    private readonly IAuthService _authService;

    public OperacionesController(IAccesoService acceso, IAuthService authService)
    {
        _acceso = acceso;
        _authService = authService;
    }

    // ── Helper para extraer el PerfilId desde el JWT ──
    private async Task<int?> ObtenerPerfilIdAsync()
    {
        var superAdminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(superAdminIdStr, out int superAdminId))
            return null;

        var perfil = await _authService.ObtenerPerfilContextoAsync(superAdminId);
        return perfil?.PerfilId;
    }

    // ── LECTURA ──────────────────────────────────────────────

    [HttpGet("solicitudes")]
    public async Task<IActionResult> ObtenerSolicitudes()
        => Ok(await _acceso.ObtenerSolicitudesPendientesAsync());

    [HttpGet("solicitudes/{id}")]
    public async Task<IActionResult> ObtenerSolicitudPorId(int id)
    {
        var result = await _acceso.ObtenerSolicitudPorIdAsync(id);
        return result == null ? NotFound("La solicitud no existe o ya fue procesada.") : Ok(result);
    }

    [HttpGet("activos")]
    public async Task<IActionResult> ObtenerActivos()
        => Ok(await _acceso.ObtenerAccesosActivosAsync());

    [HttpGet("activosZona")]
    public async Task<IActionResult> ObtenerActivosZona()
        => Ok(await _acceso.ObtenerAccesosActivosZonaAsync());

    [HttpGet("gafetes/disponibles")]
    public async Task<IActionResult> ObtenerGafetesDisponibles()
        => Ok(await _acceso.ObtenerGafetesDisponiblesAsync());

    // ── ACCIONES ─────────────────────────────────────────────

    [HttpPost("aprobar")]
    public async Task<IActionResult> Aprobar([FromBody] AprobarSolicitudRequest request)
    {
        var perfilId = await ObtenerPerfilIdAsync();
        if (perfilId == null)
            return Unauthorized("No se pudo identificar tu perfil activo.");

        var ok = await _acceso.AprobarSolicitudAsync(request, perfilId.Value);
        return ok ? Ok() : BadRequest("No se pudo aprobar la solicitud.");
    }

    [HttpPost("rechazar")]
    public async Task<IActionResult> Rechazar([FromBody] RechazarSolicitudRequest request)
    {
        var perfilId = await ObtenerPerfilIdAsync();
        if (perfilId == null)
            return Unauthorized("No se pudo identificar tu perfil activo.");

        var ok = await _acceso.RechazarSolicitudAsync(request, perfilId.Value);
        return ok ? Ok() : BadRequest("No se pudo rechazar la solicitud.");
    }

    [HttpPost("salida")]
    public async Task<IActionResult> MarcarSalida([FromBody] MarcarSalidaRequest request)
    {
        var perfilId = await ObtenerPerfilIdAsync();
        if (perfilId == null)
            return Unauthorized("No se pudo identificar tu perfil activo.");

        var ok = await _acceso.MarcarSalidaAsync(request, perfilId.Value);
        return ok ? Ok() : BadRequest("No se pudo registrar la salida.");
    }
}