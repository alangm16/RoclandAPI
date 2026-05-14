// RCD.SuperAdmin.Web/Controllers/ProyectosController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs;
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
    public async Task<IActionResult> DesactivarRol(int proyectoId, int rolId)
    {
        try
        {
            await proyectoService.DesactivarRolAsync(proyectoId, rolId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { mensaje = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { mensaje = ex.Message }); }
    }

    [HttpPut("{proyectoId:int}/roles/{rolId:int}/activar")]
    public async Task<IActionResult> ActivarRol(int proyectoId, int rolId)
    {
        try
        {
            await proyectoService.ActivarRolAsync(proyectoId, rolId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { mensaje = ex.Message }); }
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
    public async Task<IActionResult> DesactivarVista(int proyectoId, int vistaId)
    {
        try
        {
            await proyectoService.DesactivarVistaAsync(proyectoId, vistaId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message }); // Ej. Vista con hijos activos o accesos
        }
    }

    [HttpPut("{proyectoId:int}/vistas/{vistaId:int}/activar")]
    public async Task<IActionResult> ActivarVista(int proyectoId, int vistaId)
    {
        try
        {
            await proyectoService.ActivarVistaAsync(proyectoId, vistaId);
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

    [HttpGet("codigo/{codigo}")]
    [Authorize(Roles = "SuperAdmin, Admin, Auditor")]
    public async Task<IActionResult> ObtenerPorCodigo(string codigo)
    {
        var proyecto = await proyectoService.ObtenerPorCodigoAsync(codigo);
        return proyecto is null
            ? NotFound(new { mensaje = "Proyecto no encontrado." })
            : Ok(proyecto);
    }

    [HttpPut("{proyectoId:int}/roles/{rolId:int}")]
    public async Task<IActionResult> ActualizarRol(int proyectoId, int rolId, [FromBody] ActualizarRolDto dto)
    {
        try
        {
            var rol = await proyectoService.ActualizarRolAsync(proyectoId, rolId, dto);
            return Ok(rol);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{proyectoId:int}/vistas/{vistaId:int}")]
    public async Task<IActionResult> ActualizarVista(int proyectoId, int vistaId, [FromBody] ActualizarVistaDto dto)
    {
        try
        {
            var vista = await proyectoService.ActualizarVistaAsync(proyectoId, vistaId, dto);
            return Ok(vista);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{proyectoId:int}/usuarios")]
    [Authorize(Roles = "SuperAdmin, Admin, Auditor")]
    public async Task<IActionResult> ObtenerUsuariosDelProyecto(
    int proyectoId,
    [FromQuery] int pagina = 1,
    [FromQuery] int tamanoPagina = 20)
    {
        try
        {
            var resultado = await proyectoService.ObtenerUsuariosPorProyectoAsync(proyectoId, pagina, tamanoPagina);
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}/activar")]
    public async Task<IActionResult> Activar(int id)
    {
        try
        {
            await proyectoService.ActivarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPut("reordenar")]
    public async Task<IActionResult> Reordenar([FromBody] ReordenarProyectosDto request)
    {
        try
        {
            await proyectoService.ReordenarAsync(request.Items);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}