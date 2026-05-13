using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Configuracion;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede cambiar la configuración general
public class ConfiguracionController : ControllerBase
{
    private readonly IConfiguracionService _configuracionService;

    public ConfiguracionController(IConfiguracionService configuracionService)
    {
        _configuracionService = configuracionService;
    }

    /// Obtiene los parámetros de configuración general del sistema.
    /// (Visible también para Admin y Auditor en lectura)

    [HttpGet]
    [Authorize(Roles = "SuperAdmin, Admin, Auditor")]
    public async Task<IActionResult> ObtenerConfiguracion()
    {
        try
        {
            var config = await _configuracionService.ObtenerAsync();
            return Ok(config);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// Actualiza los parámetros de configuración general.

    [HttpPut]
    public async Task<IActionResult> ActualizarConfiguracion([FromBody] ConfiguracionSistemaDto dto)
    {
        try
        {
            await _configuracionService.ActualizarAsync(dto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}