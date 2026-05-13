using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Seguridad;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "SuperAdmin, Admin, Auditor")]
public class LogsAccesoController : ControllerBase
{
    private readonly ILogAccesoService _logAccesoService;

    public LogsAccesoController(ILogAccesoService logAccesoService)
    {
        _logAccesoService = logAccesoService;
    }

    /// Obtiene el registro de accesos con filtros avanzados y paginación.
    [HttpGet]
    public async Task<IActionResult> ObtenerLogs([FromQuery] FiltroLogsDto filtro)
    {
        var resultado = await _logAccesoService.ObtenerLogsAsync(filtro);
        return Ok(resultado);
    }
}