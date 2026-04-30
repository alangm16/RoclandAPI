// MatrizPermisosDto.cs
namespace RCD.SuperAdmin.Application.DTOs.Permisos;

// Lo que devuelve el endpoint de la pantalla de administración de permisos
public record MatrizPermisosDto(
    int EntidadId,           // UsuarioId o RolId
    string NombreEntidad,
    string TipoEntidad,      // "Usuario" | "Rol"
    IEnumerable<ProyectoMatrizDto> Proyectos
);

public record ProyectoMatrizDto(
    int ProyectoId,
    string Codigo,
    string Nombre,
    PermisoCrudDto? PermisoProyecto,  // null = sin acceso al proyecto completo
    IEnumerable<VistaMatrizDto> Vistas
);

public record VistaMatrizDto(
    int VistaId,
    string Codigo,
    string Nombre,
    string? Icono,
    PermisoCrudDto? Permiso     // null = sin permiso específico en esta vista
);