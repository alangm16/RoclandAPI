using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;
using RCD.Web.AccesoControl.Web.Services;

namespace RCD.Web.AccesoControl.Web.Controllers;

[ApiController]
[Route("api/web/accesocontrol/[controller]")]
[ApiExplorerSettings(GroupName = "web-accesocontrol")]
[Authorize(Policy = "AccesoControlWebPolicy")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin) => _admin = admin;

    // ── KPIs ───────────────────────────────────────────────────────────
    [HttpGet("kpis")]
    public async Task<IActionResult> Kpis()
        => Ok(await _admin.ObtenerKpisAsync());

    [HttpGet("flujo/horas")]
    public async Task<IActionResult> FlujoPorHora()
        => Ok(await _admin.ObtenerFlujoPorHoraHoyAsync());

    [HttpGet("flujo/diario")]
    public async Task<IActionResult> FlujoDiario([FromQuery] int anio, [FromQuery] int mes)
        => Ok(await _admin.ObtenerFlujoDiarioMesAsync(anio, mes));

    [HttpGet("areas/ranking")]
    public async Task<IActionResult> AreasRanking([FromQuery] int dias = 30)
        => Ok(await _admin.ObtenerAreasMasVisitadasAsync(dias));

    // ── Historial ──────────────────────────────────────────────────────
    [HttpGet("historial")]
    public async Task<IActionResult> Historial(
        [FromQuery] string? busqueda, [FromQuery] string? tipo,
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] int pagina = 1, [FromQuery] int porPagina = 20)
    {
        var (items, total) = await _admin.ObtenerHistorialAsync(
            busqueda, tipo, desde, hasta, pagina, porPagina);
        return Ok(new { items, total, pagina, porPagina });
    }

    // ── Personas ───────────────────────────────────────────────────────
    [HttpGet("personas")]
    public async Task<IActionResult> Personas([FromQuery] string? busqueda, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (items, total) = await _admin.ObtenerPersonasPaginadasAsync(busqueda, page, pageSize);
        return Ok(new { Items = items, Total = total });
    }

    [HttpGet("personas/{id}")]
    public async Task<IActionResult> PerfilPersona(int id)
    {
        var p = await _admin.ObtenerPerfilPersonaAsync(id);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpGet("personas/{id}/historial")]
    public async Task<IActionResult> HistorialPersona(int id)
        => Ok(await _admin.ObtenerHistorialPersonaAsync(id));

    // ── Guardias (Ahora mapeados a Perfiles) ───────────────────────────
    [HttpGet("guardias")]
    public async Task<IActionResult> Guardias([FromQuery] string? busqueda, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (items, total) = await _admin.ObtenerGuardiasAsync(busqueda, page, pageSize);
        return Ok(new { Items = items, Total = total });
    }

    [HttpGet("usuarios/sinperfil")]
    public async Task<IActionResult> ObtenerUsuariosSinPerfil()
    {
        var lista = await _admin.ObtenerUsuariosSinPerfilAsync();
        return Ok(lista);
    }

    [HttpPost("usuarios/crearperfil")]
    public async Task<IActionResult> CrearPerfil([FromBody] CrearPerfilRequest request)
    {
        if (request == null) return BadRequest();
        var ok = await _admin.CrearPerfilAsync(request);
        return ok ? Ok() : BadRequest("No se pudo crear el perfil. Verifique que el usuario tenga asignación válida.");
    }

    [HttpPut("guardias/{id}/toggle-estado")]
    public async Task<IActionResult> CambiarEstadoPerfil(int id, [FromBody] bool activo)
    {
        var ok = await _admin.ActualizarEstadoPerfilAsync(id, activo);
        return ok ? Ok(true) : NotFound();
    }

    // Mantenemos Actualizar para cambiar cosas como el Turno o el Número de Empleado
    [HttpPut("guardias/{id}/datos")]
    public async Task<IActionResult> ActualizarGuardia(int id, GuardiaUpdateDto dto)
        => Ok(await _admin.ActualizarGuardiaAsync(id, dto));

    // ── Catálogos ──────────────────────────────────────────────────────
    [HttpPost("areas")]
    public async Task<IActionResult> CrearArea(CatalogoCreateDto dto)
        => Ok(await _admin.CrearAreaAsync(dto));
    [HttpPut("areas/{id}/toggle")]
    public async Task<IActionResult> ToggleArea(int id)
        => Ok(await _admin.ToggleAreaAsync(id));

    [HttpPost("motivos")]
    public async Task<IActionResult> CrearMotivo(CatalogoCreateDto dto)
        => Ok(await _admin.CrearMotivoAsync(dto));
    [HttpPut("motivos/{id}/toggle")]
    public async Task<IActionResult> ToggleMotivo(int id)
        => Ok(await _admin.ToggleMotivoAsync(id));

    [HttpPost("tiposid")]
    public async Task<IActionResult> CrearTipoId(CatalogoCreateDto dto)
        => Ok(await _admin.CrearTipoIdAsync(dto));
    [HttpPut("tiposid/{id}/toggle")]
    public async Task<IActionResult> ToggleTipoId(int id)
        => Ok(await _admin.ToggleTipoIdAsync(id));

    [HttpGet("areas")]
    public async Task<IActionResult> GetAreas()
        => Ok(await _admin.GetAreasAsync());

    [HttpGet("motivos")]
    public async Task<IActionResult> GetMotivos()
        => Ok(await _admin.GetMotivosAsync());

    [HttpGet("tiposid")]
    public async Task<IActionResult> GetTiposId()
        => Ok(await _admin.GetTiposIdAsync());

    // ── Exportar ───────────────────────────────────────────────────────
    [HttpGet("exportar/excel")]
    public async Task<IActionResult> ExportarExcel()
    {
        var bytes = await _admin.ExportarExcelHoyAsync();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"accesos_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("exportar/pdf")]
    public async Task<IActionResult> ExportarPdf()
    {
        var bytes = await _admin.ExportarPdfHoyAsync();
        return File(bytes, "application/pdf",
            $"accesos_{DateTime.Now:yyyyMMdd}.pdf");
    }
}