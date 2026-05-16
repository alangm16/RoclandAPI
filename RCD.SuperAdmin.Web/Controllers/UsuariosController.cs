using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Usuarios;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "SuperAdmin, Admin")] 
public class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    // ── 1. GESTIÓN BASE DE USUARIOS ─────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
    [FromQuery] bool soloPanel = false,
    [FromQuery] int pagina = 1,
    [FromQuery] int tamanoPagina = 20,
    [FromQuery] bool? activo = null)
    {
        return Ok(await usuarioService.ObtenerTodosAsync(soloPanel, pagina, tamanoPagina, activo));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var usuario = await usuarioService.ObtenerPorIdAsync(id);
        return usuario is null ? NotFound(new { mensaje = "Usuario no encontrado." }) : Ok(usuario);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto request)
    {
        try
        {
            var usuario = await usuarioService.CrearAsync(request);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message }); // Ej. Username duplicado
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioDto request)
    {
        try
        {
            var usuario = await usuarioService.ActualizarAsync(id, request);
            return Ok(usuario);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message }); // Ej. RolSA inactivo
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            await usuarioService.DesactivarAsync(id);
            return NoContent();
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
            await usuarioService.ActivarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    // ── 2. ASIGNACIÓN DE PROYECTOS Y ROLES ──────────────────────────────

    // POST /api/superadmin/usuarios/5/proyectos
    [HttpPost("{usuarioId:int}/proyectos")]
    public async Task<IActionResult> AsignarProyectoRol(int usuarioId, [FromBody] AsignarProyectoRolDto request)
    {
        try
        {
            await usuarioService.AsignarProyectoRolAsync(usuarioId, request);
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

    // DELETE /api/superadmin/usuarios/5/proyectos/3
    [HttpDelete("{usuarioId:int}/proyectos/{proyectoId:int}")]
    public async Task<IActionResult> RevocarProyecto(int usuarioId, int proyectoId)
    {
        try
        {
            await usuarioService.RevocarProyectoAsync(usuarioId, proyectoId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // ── 3. ASIGNACIÓN DE VISTAS (MENÚ) ──────────────────────────────────

    // PUT /api/superadmin/usuarios/5/proyectos/3/vistas
    [HttpPut("{usuarioId:int}/proyectos/{proyectoId:int}/vistas")]
    public async Task<IActionResult> ActualizarVistasAcceso(int usuarioId, int proyectoId, [FromBody] IEnumerable<int> vistaIds)
    {
        try
        {
            await usuarioService.ActualizarVistasAccesoAsync(usuarioId, proyectoId, vistaIds);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message }); // Ej. Vistas que no son del proyecto
        }
    }

    [HttpPut("{id:int}/reset-intentos")]
    public async Task<IActionResult> ResetearIntentos(int id)
    {
        try
        {
            await usuarioService.ResetearIntentosAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}