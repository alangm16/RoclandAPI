using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RCD.Web.AccesoControl.Application.Interfaces;
using RCD.Web.AccesoControl.Application.DTOs; 
namespace RCD.Web.AccesoControl.Web.Controllers;

[ApiController]
[Route("api/web/accesocontrol/[controller]")]
[ApiExplorerSettings(GroupName = "web-accesocontrol")]
// Sigue siendo público (sin [Authorize])
public class VisitantesController : ControllerBase
{
    private readonly IAccesoService _acceso;

    public VisitantesController(IAccesoService acceso) => _acceso = acceso;

    [HttpPost]
    [EnableRateLimiting("FormSubmissionLimit")]
    public async Task<IActionResult> Registrar(
        [FromBody] CrearVisitanteRequest request) // Usamos el request directo
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!request.ConsentimientoFirmado)
            return BadRequest("El consentimiento es obligatorio.");

        // ── FIX: Definimos el ID del Perfil que representará al "Kiosco" o "Autoregistro".
        // Mantenemos el '1' que usabas antes para el GuardiaEntradaId hardcodeado.
        int perfilKioskoId = 1;

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // ── FIX: Agregamos el perfilKioskoId a la llamada
        var result = await _acceso.RegistrarVisitanteAsync(request, perfilKioskoId, ip);

        return Ok(result);
    }
}