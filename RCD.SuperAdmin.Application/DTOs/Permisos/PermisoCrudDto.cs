// PermisoCrudDto.cs — bloque CRUD reutilizado en ambas tablas de permisos
namespace RCD.SuperAdmin.Application.DTOs.Permisos;

public record PermisoCrudDto(
    bool PuedeLeer,
    bool PuedeCrear,
    bool PuedeEditar,
    bool PuedeBorrar
);

// Para asignar o actualizar permisos de ROL
public record AsignarPermisoRolRequest(
    int RolId,
    int ProyectoId,
    int? VistaId,
    bool PuedeLeer,
    bool PuedeCrear,
    bool PuedeEditar,
    bool PuedeBorrar
);

// Para asignar o actualizar permisos de USUARIO (override)
public record AsignarPermisoUsuarioRequest(
    int UsuarioId,
    int ProyectoId,
    int? VistaId,
    bool PuedeLeer,
    bool PuedeCrear,
    bool PuedeEditar,
    bool PuedeBorrar
);

