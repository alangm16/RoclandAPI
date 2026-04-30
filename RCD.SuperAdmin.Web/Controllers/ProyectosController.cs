// RCD.SuperAdmin.Web/Controllers/ProyectosController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Proyectos;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "Programador,Supervisor")]
public class ProyectosController(IProyectoService proyectoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos() =>
        Ok(await proyectoService.ObtenerTodosAsync());

    [HttpPost]
    [Authorize(Roles = "Programador")]
    public async Task<IActionResult> CrearProyecto([FromBody] CrearProyectoRequest request)
    {
        var proyecto = await proyectoService.CrearProyectoAsync(request);
        return CreatedAtAction(nameof(ObtenerTodos), new { id = proyecto.Id }, proyecto);
    }

    [HttpPost("{proyectoId:int}/vistas")]
    [Authorize(Roles = "Programador")]
    public async Task<IActionResult> CrearVista(int proyectoId, [FromBody] CrearVistaRequest request)
    {
        var proyecto = await proyectoService.CrearVistaAsync(proyectoId, request);
        return CreatedAtAction(nameof(ObtenerTodos), new { id = proyectoId }, proyecto);
    }
}