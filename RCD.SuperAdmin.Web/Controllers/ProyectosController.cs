// RCD.SuperAdmin.Web/Controllers/ProyectosController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Proyectos;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "SuperAdmin, Admin")]
public class ProyectosController(IProyectoService proyectoService) : ControllerBase
{
    // 1. GESTIÓN BASE DE PROYECTOS

    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin, Auditor")] // El auditor sí puede ver el catálogo
    public async Task<IActionResult> ObtenerTodos()
    {
        return Ok(await proyectoService.ObtenerTodosAsync());
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "SuperAdmin, Admin, Auditor")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var proyecto = await proyectoService.ObtenerPorIdAsync(id);
        return proyecto is null ? NotFound(new { mensaje = "Proyecto no encontrado." }) : Ok(proyecto);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearProyectoDto request)
    {
        try
        {
            var proyecto = await proyectoService.CrearAsync(request);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = proyecto.Id }, proyecto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message }); // Ej. Código de proyecto duplicado
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProyectoDto request)
    {
        try
        {
            var proyecto = await proyectoService.ActualizarAsync(id, request);
            return Ok(proyecto); // Retornamos la entidad actualizada
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message }); // Ej. Plataforma inválida
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            await proyectoService.DesactivarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    // 2. GESTIÓN DE ROLES DEL PROYECTO

    [HttpGet("{proyectoId:int}/roles")]
    [Authorize(Roles = "SuperAdmin, Admin, Auditor")]
    public async Task<IActionResult> ObtenerRoles(int proyectoId)
    {
        try
        {
            return Ok(await proyectoService.ObtenerRolesAsync(proyectoId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{proyectoId:int}/roles")]
    public async Task<IActionResult> CrearRol(int proyectoId, [FromBody] CrearRolDto request)
    {
        try
        {
            var rol = await proyectoService.CrearRolAsync(proyectoId, request);
            return Ok(rol);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message }); // Ej. Nombre de rol repetido
        }
    }

    [HttpDelete("{proyectoId:int}/roles/{rolId:int}")]
    public async Task<IActionResult> EliminarRol(int proyectoId, int rolId)
    {
        try
        {
            await proyectoService.EliminarRolAsync(proyectoId, rolId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message }); // Ej. Rol asignado a un usuario
        }
    }

    // 3. GESTIÓN DE VISTAS DEL PROYECTO

    [HttpGet("{proyectoId:int}/vistas")]
    [Authorize(Roles = "SuperAdmin, Admin, Auditor")]
    public async Task<IActionResult> ObtenerVistas(int proyectoId)
    {
        try
        {
            return Ok(await proyectoService.ObtenerVistasAsync(proyectoId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{proyectoId:int}/vistas")]
    public async Task<IActionResult> CrearVista(int proyectoId, [FromBody] CrearVistaDto request)
    {
        try
        {
            var vista = await proyectoService.CrearVistaAsync(proyectoId, request);
            return Ok(vista);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message }); // Ej. Código de vista duplicado
        }
    }

    [HttpDelete("{proyectoId:int}/vistas/{vistaId:int}")]
    public async Task<IActionResult> EliminarVista(int proyectoId, int vistaId)
    {
        try
        {
            await proyectoService.EliminarVistaAsync(proyectoId, vistaId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message }); // Ej. Vista en uso en accesos
        }
    }

    [HttpGet("codigo/{codigo}")]
    [Authorize(Roles = "SuperAdmin, Admin, Auditor")]
    public async Task<IActionResult> ObtenerPorCodigo(string codigo)
    {
        var proyecto = await proyectoService.ObtenerPorCodigoAsync(codigo);
        return proyecto is null
            ? NotFound(new { mensaje = "Proyecto no encontrado." })
            : Ok(proyecto);
    }
}