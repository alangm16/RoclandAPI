// RCD.SuperAdmin.Web/Controllers/RolesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Roles;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "Programador,Supervisor")]
public class RolesController(IRolService rolService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos() =>
        Ok(await rolService.ObtenerTodosAsync());

    [HttpPost]
    [Authorize(Roles = "Programador")]
    public async Task<IActionResult> Crear([FromBody] CrearRolRequest request)
    {
        var rol = await rolService.CrearAsync(request);
        return CreatedAtAction(nameof(ObtenerTodos), new { id = rol.Id }, rol);
    }
}