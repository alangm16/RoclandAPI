using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Application.DTOs;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "Programador,Supervisor")]   // Solo el Super Panel accede aquí
public class PermisosController(IPermisosService permisosService) : ControllerBase
{
    [HttpGet("matriz/{usuarioId:int}")]
    public async Task<IActionResult> ObtenerMatriz(int usuarioId) =>
        Ok(await permisosService.ObtenerMatrizPermisosAsync(usuarioId));

    [HttpPost("asignar")]
    public async Task<IActionResult> Asignar([FromBody] AsignarPermisoRequest request)
    {
        await permisosService.AsignarPermisoAsync(request);
        return NoContent();
    }

    [HttpDelete("{permisoId:int}")]
    public async Task<IActionResult> Revocar(int permisoId)
    {
        await permisosService.RevocarPermisoAsync(permisoId);
        return NoContent();
    }
}