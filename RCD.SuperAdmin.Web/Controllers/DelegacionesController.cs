using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Delegaciones;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "SuperAdmin, Admin, Auditor")]
public class DelegacionesController : ControllerBase
{
    private readonly IDelegacionService _delegacionService;

    public DelegacionesController(IDelegacionService delegacionService)
    {
        _delegacionService = delegacionService;
    }

    /// Obtiene el registro de delegaciones (quién asignó acceso a quién, en qué proyecto y con qué rol),
    /// con filtro opcional por proyecto y paginación.
    [HttpGet]
    public async Task<IActionResult> ObtenerDelegaciones([FromQuery] FiltroDelegacionesDto filtro)
    {
        var resultado = await _delegacionService.ObtenerDelegacionesAsync(filtro);
        return Ok(resultado);
    }
}