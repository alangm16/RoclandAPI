using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Permisos;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "Programador,Supervisor")]   // Solo el Super Panel accede aquí
public class PermisosController(IPermisosService permisosService) : ControllerBase
{
    [HttpGet("rol/{rolId:int}")]
    public async Task<IActionResult> MatrizRol(int rolId) =>
        Ok(await permisosService.ObtenerMatrizRolAsync(rolId));

    [HttpGet("usuario/{usuarioId:int}")]
    public async Task<IActionResult> MatrizUsuario(int usuarioId) =>
        Ok(await permisosService.ObtenerMatrizUsuarioAsync(usuarioId));

    [HttpPut("rol")]
    public async Task<IActionResult> UpsertRol([FromBody] AsignarPermisoRolRequest req)
    {
        await permisosService.UpsertPermisoRolAsync(req);
        return NoContent();
    }

    [HttpPut("usuario")]
    public async Task<IActionResult> UpsertUsuario([FromBody] AsignarPermisoUsuarioRequest req)
    {
        await permisosService.UpsertPermisoUsuarioAsync(req);
        return NoContent();
    }

    [HttpDelete("rol")]
    public async Task<IActionResult> RevocarRol(
        [FromQuery] int rolId, [FromQuery] int proyectoId, [FromQuery] int? vistaId)
    {
        await permisosService.RevocarPermisoRolAsync(rolId, proyectoId, vistaId);
        return NoContent();
    }

    [HttpDelete("usuario")]
    public async Task<IActionResult> RevocarUsuario(
        [FromQuery] int usuarioId, [FromQuery] int proyectoId, [FromQuery] int? vistaId)
    {
        await permisosService.RevocarPermisoUsuarioAsync(usuarioId, proyectoId, vistaId);
        return NoContent();
    }
}