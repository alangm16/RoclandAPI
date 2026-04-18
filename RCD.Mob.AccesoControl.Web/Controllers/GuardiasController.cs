using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;
using RCD.Web.AccesoControl.Infrastructure.Hubs;
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

    public GuardiasController(IAccesoService acceso, IHubContext<AccesoControlHub> hub)
    {
        _acceso = acceso;
        _hub = hub;
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

    [HttpPost("aprobar")]
    public async Task<IActionResult> Aprobar(AprobarSolicitudRequest request)
    {
        var ok = await _acceso.AprobarSolicitudAsync(request);
        if (!ok) return BadRequest("No se pudo aprobar la solicitud.");

        await _hub.Clients.Group("Guardias")
            .SendAsync("SolicitudResuelta", new
            {
                solicitudId = request.SolicitudId,
                estado = "Aprobada"
            });
        return Ok();
    }

    [HttpPost("rechazar")]
    public async Task<IActionResult> Rechazar(RechazarSolicitudRequest request)
    {
        var ok = await _acceso.RechazarSolicitudAsync(request);
        if (!ok) return BadRequest("No se pudo rechazar la solicitud.");

        await _hub.Clients.Group("Guardias")
            .SendAsync("SolicitudResuelta", new
            {
                solicitudId = request.SolicitudId,
                estado = "Rechazada"
            });
        return Ok();
    }

    [HttpPost("salida")]
    public async Task<IActionResult> MarcarSalida(MarcarSalidaRequest request)
    {
        var ok = await _acceso.MarcarSalidaAsync(request);
        if (!ok) return BadRequest("No se pudo registrar la salida.");

        await _hub.Clients.Group("Guardias")
            .SendAsync("SalidaRegistrada", request.RegistroId);
        return Ok();
    }

    [HttpPost("fcm-token")]
    public async Task<IActionResult> RegistrarFcmToken(
        [FromBody] RegistrarFcmTokenRequest request)
    {
        var ok = await _acceso.GuardarFcmTokenAsync(request.GuardiaId, request.FcmToken);
        return ok ? Ok() : BadRequest("No se pudo guardar el token.");
    }
}