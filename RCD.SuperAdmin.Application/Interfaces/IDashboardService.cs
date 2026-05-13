using RCD.SuperAdmin.Application.DTOs.Dashboard;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardGlobalDto> GetResumenGlobalAsync();
    Task<DashboardProyectoDto> GetDashboardPorProyectoAsync(int proyectoId);
}