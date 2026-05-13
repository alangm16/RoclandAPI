using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.RolesSA;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "SuperAdmin")]
public class RolesSAController : ControllerBase
{
    private readonly IRolSAService _rolSAService;

    public RolesSAController(IRolSAService rolSAService)
    {
        _rolSAService = rolSAService;
    }

    /// Obtiene todos los roles del panel SuperAdmin.
    /// (Visible también para Admin y Auditor)

    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin, Auditor")]
    public async Task<IActionResult> ObtenerTodos()
    {
        var roles = await _rolSAService.ObtenerTodosAsync();
        return Ok(roles);
    }

    /// Obtiene un rol SA por su Id.

    [HttpGet("{id:int}")]
    [Authorize(Roles = "SuperAdmin, Admin, Auditor")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var rol = await _rolSAService.ObtenerPorIdAsync(id);
            return Ok(rol);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// Crea un nuevo rol del panel SuperAdmin.

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearRolSADto dto)
    {
        try
        {
            var rol = await _rolSAService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = rol.Id }, rol);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// Actualiza un rol SA existente.

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarRolSADto dto)
    {
        try
        {
            var rol = await _rolSAService.ActualizarAsync(id, dto);
            return Ok(rol);
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

    /// Desactiva un rol SA (eliminación lógica).
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            await _rolSAService.DesactivarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}