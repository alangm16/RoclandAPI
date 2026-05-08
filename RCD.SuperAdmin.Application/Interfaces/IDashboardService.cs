using RCD.SuperAdmin.Application.DTOs.Dashboard;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IDashboardService
{
    Task<SuperAdminDashboardKpisDto> GetKpisAsync();
    Task<IEnumerable<AccesosDiaSemanaDto>> GetAccesosSemanaAsync();
    Task<IEnumerable<ModuloUsageDto>> GetUsoModulosAsync();
    Task<IEnumerable<AlertaDto>> GetAlertasActivasAsync();
    Task<IEnumerable<AccesoLogDto>> GetLogsRecientesAsync(int limit);
}