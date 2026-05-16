namespace RCD.SuperAdmin.Application.DTOs.Usuarios;

/// Fila en la tabla de usuarios del panel SA.
public record UsuarioListDto(
    int Id,
    string NombreCompleto,
    string Username,
    string? Email,
    bool Activo,
    DateTime FechaCreacion,
    DateTime? UltimoAcceso,      
    DateTime? BloqueadoHasta     
);

/// Vista completa de un usuario con sus proyectos y accesos.
public record UsuarioDetalleDto(
    int Id,
    string NombreCompleto,
    string Username,
    string? Email,
    string? QRCode,
    bool Activo,
    DateTime? UltimoAcceso,
    IEnumerable<ProyectoAsignadoDto> Proyectos,
    int IntentosFallidos,        
    DateTime? BloqueadoHasta,    
    string? CreadoPor,           
    DateTime FechaCreacion,      
    string? ModificadoPor,       
    DateTime? FechaModificacion  
);

/// Resumen del acceso de un usuario a un proyecto.
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
    string? QRCode
);

public record ActualizarUsuarioDto(
    string NombreCompleto,
    string? Email,
    string? Password,
    string? QRCode
);

public record AsignarProyectoRolDto(
    int ProyectoId,
    int RolId
);