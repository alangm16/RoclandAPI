using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Auditoria;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "SuperAdmin, Admin, Auditor")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;

    public AuditoriaController(IAuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    /// Obtiene el registro de auditoría global (creaciones, modificaciones) 
    /// con filtros avanzados y paginación.
    [HttpGet]
    public async Task<IActionResult> ObtenerAuditoria([FromQuery] FiltroAuditoriaDto filtro)
    {
        var resultado = await _auditoriaService.ObtenerRegistrosAsync(filtro);
        return Ok(resultado);
    }
}