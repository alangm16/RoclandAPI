using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[Authorize] 
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis()
    {
        return Ok(await _dashboardService.GetKpisAsync());
    }

    [HttpGet("accesos-semana")]
    public async Task<IActionResult> GetGraficaSemana()
    {
        return Ok(await _dashboardService.GetAccesosSemanaAsync());
    }

    [HttpGet("modulos-usage")]
    public async Task<IActionResult> GetModulosUsage()
    {
        return Ok(await _dashboardService.GetUsoModulosAsync());
    }

    [HttpGet("alertas")]
    public async Task<IActionResult> GetAlertas()
    {
        return Ok(await _dashboardService.GetAlertasActivasAsync());
    }

    [HttpGet("logs-recientes")]
    public async Task<IActionResult> GetLogsRecientes([FromQuery] int limit = 5)
    {
        return Ok(await _dashboardService.GetLogsRecientesAsync(limit));
    }
}