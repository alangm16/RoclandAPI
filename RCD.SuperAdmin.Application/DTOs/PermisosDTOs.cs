
namespace RCD.SuperAdmin.Application.DTOs
{
    // Para el Super Panel — administrar permisos de un usuario
    public record AsignarPermisoRequest(int UsuarioId, int ProyectoId, int? VistaId);
    public record RevocarPermisoRequest(int PermisoId);

    public record MatrizPermisosDto(
        int UsuarioId,
        string NombreCompleto,
        IEnumerable<ProyectoMatrizDto> Proyectos
    );

    public record ProyectoMatrizDto(
        int ProyectoId,
        string Codigo,
        string Nombre,
        bool TieneAccesoTotal,         // true si hay permiso sin VistaId específica
        IEnumerable<VistaMatrizDto> Vistas
    );

    public record VistaMatrizDto(int VistaId, string Codigo, string Nombre, bool TieneAcceso);
}
