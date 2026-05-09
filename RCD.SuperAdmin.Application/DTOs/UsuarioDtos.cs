namespace RCD.SuperAdmin.Application.DTOs.Usuarios;

/// <summary>Fila en la tabla de usuarios del panel SA.</summary>
public record UsuarioListDto(
    int Id,
    string NombreCompleto,
    string Username,
    string? Email,
    string? RolSA,
    bool Activo,
    DateTime FechaCreacion
);

/// <summary>Vista completa de un usuario con sus proyectos y accesos.</summary>
public record UsuarioDetalleDto(
    int Id,
    string NombreCompleto,
    string Username,
    string? Email,
    string? QRCode,
    string? RolSA,
    bool Activo,
    DateTime UltimoAcceso,
    IEnumerable<ProyectoAsignadoDto> Proyectos
);

/// <summary>Resumen del acceso de un usuario a un proyecto.</summary>
public record ProyectoAsignadoDto(
    int ProyectoId,
    string CodigoProyecto,
    string NombreProyecto,
    string Rol,
    int NivelRol,
    bool Activo
);

public record CrearUsuarioDto(
    string NombreCompleto,
    string Username,
    string? Email,
    string Password,
    int? RolSAId    // null = sin acceso al panel SA
);

public record ActualizarUsuarioDto(
    string NombreCompleto,
    string? Email,
    int? RolSAId
);

public record AsignarProyectoRolDto(
    int ProyectoId,
    int RolId
);