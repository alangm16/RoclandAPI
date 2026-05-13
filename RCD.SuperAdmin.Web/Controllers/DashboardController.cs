using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "SuperAdmin, Admin, Auditor")] // Todos los roles del panel pueden ver dashboards
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Dashboard global del sistema: totales, gráficas de accesos, proyectos y usuarios más activos.
    /// </summary>
    [HttpGet("global")]
    public async Task<IActionResult> GetResumenGlobal()
    {
        var result = await _dashboardService.GetResumenGlobalAsync();
        return Ok(result);
    }

    /// <summary>
    /// Dashboard detallado de un proyecto específico: KPIs, últimos accesos, usuarios activos hoy, alertas abiertas.
    /// </summary>
    [HttpGet("proyecto/{proyectoId:int}")]
    public async Task<IActionResult> GetDashboardProyecto(int proyectoId)
    {
        try
        {
            var result = await _dashboardService.GetDashboardPorProyectoAsync(proyectoId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}