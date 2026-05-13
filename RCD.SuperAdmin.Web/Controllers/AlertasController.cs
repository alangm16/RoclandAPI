using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Alertas;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "SuperAdmin, Admin, Auditor")]
public class AlertasController : ControllerBase
{
    private readonly IAlertaService _alertaService;

    public AlertasController(IAlertaService alertaService)
    {
        _alertaService = alertaService;
    }

    /// Obtiene las alertas según los filtros aplicados (proyecto, tipo, estado, rango de fechas) con paginación.
    [HttpGet]
    public async Task<IActionResult> ObtenerAlertas([FromQuery] FiltroAlertasDto filtro)
    {
        var resultado = await _alertaService.ObtenerAlertasAsync(filtro);
        return Ok(resultado);
    }

    /// Marca una alerta como resuelta (solo SuperAdmin y Admin).
    [HttpPatch("{id:int}/resolver")]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> MarcarResuelta(int id)
    {
        try
        {
            await _alertaService.MarcarResueltaAsync(id);
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