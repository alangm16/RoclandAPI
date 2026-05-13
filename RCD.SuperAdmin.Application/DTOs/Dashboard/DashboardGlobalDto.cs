namespace RCD.SuperAdmin.Application.DTOs.Dashboard;

public record DashboardGlobalDto(
    int TotalUsuariosActivos,
    int TotalUsuariosInactivos,
    int TotalProyectosActivos,
    int ProyectosProduccion,
    int ProyectosMantenimiento,
    int ProyectosDesarrollo,
    int UsuariosBloqueados,
    int AlertasCriticasNoResueltas,
    IEnumerable<GraficoAccesosDto> GraficoAccesos,
    IEnumerable<ProyectoActividadDto> ProyectosMasAccesos,
    IEnumerable<UsuarioActividadDto> UsuariosMasActividad,
    IEnumerable<ProyectoConAlertasDto> ProyectosConProblemas
);

public record GraficoAccesosDto(
    string Fecha,
    int Exitosos,
    int Fallidos
);

public record ProyectoActividadDto(
    int ProyectoId,
    string NombreProyecto,
    int CantidadAccesos
);

public record UsuarioActividadDto(
    int UsuarioId,
    string NombreUsuario,
    int CantidadAccesos
);

public record ProyectoConAlertasDto(
    int ProyectoId,
    string Codigo,
    string Nombre,
    string Estado,
    int CantidadAlertasCriticas
);

