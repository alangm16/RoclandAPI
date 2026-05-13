namespace RCD.SuperAdmin.Application.DTOs.Dashboard;

public record DashboardProyectoDto(
    int ProyectoId,
    string Nombre,
    string Codigo,
    string Descripcion,
    string Estado,
    string Plataforma,
    string? Version,
    string? UrlBase,
    int UsuariosAsignados,
    int RolesDefinidos,
    int VistasConfiguradas,
    int TokensActivos,
    IEnumerable<UltimoAccesoDto> UltimosAccesos,
    int UsuariosActivosHoy,
    int AlertasAbiertas
);

public record UltimoAccesoDto(
    string Username,
    DateTime Fecha
);