using Microsoft.AspNetCore.Mvc;
using RCD.Web.AccesoControl.Application.Interfaces;

namespace RCD.Web.AccesoControl.Web.Controllers;

[ApiController]
[Route("api/web/accesocontrol/[controller]/")] // 'ac' para identificar que es del módulo AccesoControl
public class CatalogosController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public CatalogosController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet("areas")]
    public async Task<IActionResult> GetAreas()
        => Ok(await _catalogoService.ObtenerAreasAsync());

    [HttpGet("tipos-id")]
    public async Task<IActionResult> GetTiposId()
        => Ok(await _catalogoService.ObtenerTiposIdentificacionAsync());

    [HttpGet("motivos")]
    public async Task<IActionResult> GetMotivos()
        => Ok(await _catalogoService.ObtenerMotivosVisitaAsync());
}