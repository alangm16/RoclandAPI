using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RCD.Web.AccesoControl.Application.Interfaces;

namespace RCD.Web.AccesoControl.Web.Controllers;

[ApiController]
[Route("api/web/accesocontrol/[controller]")]
[ApiExplorerSettings(GroupName = "web-accesocontrol")]
public class VisitantesController : ControllerBase
{
    private readonly IAccesoService _acceso;
    public VisitantesController(IAccesoService acceso) => _acceso = acceso;

    [HttpPost]
    [EnableRateLimiting("FormSubmissionLimit")]
    public async Task<IActionResult> Registrar(
        [FromBody] Application.DTOs.CrearVisitanteRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!request.ConsentimientoFirmado)
            return BadRequest("El consentimiento es obligatorio.");

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _acceso.RegistrarVisitanteAsync(request, ip);
        return Ok(result);
    }
}
