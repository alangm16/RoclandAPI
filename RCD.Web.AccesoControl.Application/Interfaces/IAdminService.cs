using RCD.Web.AccesoControl.Application.DTOs;

namespace RCD.Web.AccesoControl.Application.Interfaces;

public interface IAdminService
{
    // KPIs
    Task<DashboardKpiDto> ObtenerKpisAsync();
    Task<IEnumerable<FlujoPorHoraDto>> ObtenerFlujoPorHoraHoyAsync();
    Task<IEnumerable<FlujoDiarioDto>> ObtenerFlujoDiarioMesAsync(int anio, int mes);
    Task<IEnumerable<AreaVisitadaDto>> ObtenerAreasMasVisitadasAsync(int dias = 30);

    // Historial
    Task<(IEnumerable<HistorialAccesoDto> Items, int Total)> ObtenerHistorialAsync(
        string? busqueda, string? tipo, DateTime? desde, DateTime? hasta, int pagina, int porPagina);

    // Personas
    Task<(IEnumerable<PersonaPerfilDto> Items, int Total)> ObtenerPersonasPaginadasAsync(string? busqueda, int pagina, int porPagina);
    Task<PersonaPerfilDto?> ObtenerPerfilPersonaAsync(int id);
    Task<IEnumerable<HistorialAccesoDto>> ObtenerHistorialPersonaAsync(int personaId);

    // Catálogos
    Task<bool> CrearAreaAsync(CatalogoCreateDto dto);
    Task<bool> ToggleAreaAsync(int id);
    Task<bool> CrearMotivoAsync(CatalogoCreateDto dto);
    Task<bool> ToggleMotivoAsync(int id);
    Task<bool> CrearTipoIdAsync(CatalogoCreateDto dto);
    Task<bool> ToggleTipoIdAsync(int id);
    Task<IEnumerable<AreaDto>> GetAreasAsync();
    Task<IEnumerable<MotivoDto>> GetMotivosAsync();
    Task<IEnumerable<TipoIdDto>> GetTiposIdAsync();

    // Guardias
    Task<(IEnumerable<GuardiaListDto> Items, int Total)> ObtenerGuardiasAsync(string? busqueda, int pagina, int porPagina);
    Task<bool> CrearGuardiaAsync(GuardiaCreateDto dto);
    Task<bool> ActualizarGuardiaAsync(int id, GuardiaUpdateDto dto);
    Task<bool> ResetPasswordGuardiaAsync(int id, string nuevaPassword);

    // Exportar
    Task<byte[]> ExportarExcelHoyAsync();
    Task<byte[]> ExportarPdfHoyAsync();
}