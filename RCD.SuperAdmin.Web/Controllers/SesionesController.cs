using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Seguridad;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "SuperAdmin, Admin")]
public class SesionesController : ControllerBase
{
    private readonly ISesionService _sesionService;

    public SesionesController(ISesionService sesionService)
    {
        _sesionService = sesionService;
    }

    /// Obtiene las sesiones activas (refresh tokens no revocados ni expirados)
    /// con filtros opcionales por usuario y/o proyecto, y paginación.
    [HttpGet]
    public async Task<IActionResult> ObtenerSesiones([FromQuery] FiltroSesionesDto filtro)
    {
        var resultado = await _sesionService.ObtenerSesionesAsync(filtro);
        return Ok(resultado);
    }

    /// Revoca una sesión específica forzando el cierre de sesión.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RevocarSesion(int id)
    {
        try
        {
            await _sesionService.RevocarSesionAsync(id);
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