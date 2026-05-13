using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "SuperAdmin, Admin")]
public class VisibilidadController : ControllerBase
{
    private readonly IVisibilidadService _visibilidadService;

    public VisibilidadController(IVisibilidadService visibilidadService)
    {
        _visibilidadService = visibilidadService;
    }

    /// Obtiene todas las vistas de un proyecto para un usuario específico,
    /// indicando cuáles están activas (visibles) y cuáles no.
    [HttpGet("usuarios/{usuarioId:int}/proyectos/{proyectoId:int}/vistas")]
    public async Task<IActionResult> ObtenerVistasAcceso(int usuarioId, int proyectoId)
    {
        try
        {
            var vistas = await _visibilidadService.ObtenerVistasAccesoAsync(usuarioId, proyectoId);
            return Ok(vistas);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// Activa o desactiva la visibilidad de una vista para un usuario específico.
    [HttpPut("usuarios/{usuarioId:int}/vistas/{vistaId:int}")]
    public async Task<IActionResult> ActualizarVistaAcceso(int usuarioId, int vistaId, [FromBody] bool tieneAcceso)
    {
        try
        {
            await _visibilidadService.ActualizarVistaAccesoAsync(usuarioId, vistaId, tieneAcceso);
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