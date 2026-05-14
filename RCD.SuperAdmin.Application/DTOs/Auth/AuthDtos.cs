namespace RCD.SuperAdmin.Application.DTOs.Auth;

/// Flujo 1 — App Móvil / Desktop.
/// El cliente especifica a qué proyecto quiere entrar.
public record LoginDirectoDto(
    string Username,
    string Password,
    string CodigoProyecto,  // ej: "ACCESO-CONTROL-MOVIL"
    string Plataforma       // Web | Desktop | Mobile
);

public record LoginQrDto(
    string QrCode,
    string CodigoProyecto,  // ej: "ACCESO-CONTROL-MOVIL"
    string Plataforma       // ej: "Mobile"
);

/// Flujo 2 — Panel Web SA.
/// Sin proyecto: SuperAdmin devuelve la lista de proyectos accesibles.
public record LoginMaestroDto(
    string Username,
    string Password,
    string Plataforma = "Web"
);

public record RefreshTokenDto(
    string RefreshToken,
    string Plataforma
);

/// Respuesta del flujo directo (móvil/desktop) y del refresh.
/// El JWT contiene claims [Usuario, Proyecto, Rol, Plataforma].
public record AuthResultDto(
    string AccessToken,
    string RefreshToken,
    DateTime Expiracion,
    UsuarioTokenDto Usuario
);

/// Respuesta del flujo maestro (panel web).
/// El JWT maestro no lleva proyecto; la lista de proyectos es la que
/// permite al frontend mostrar el selector de acceso.
public record AuthMaestroResultDto(
    string AccessToken,          // token maestro (esMaestro=true en claims)
    string RefreshToken,
    DateTime Expiracion,
    UsuarioTokenDto Usuario,
    IEnumerable<ProyectoAccesoDto> ProyectosAccesibles
);

public record UsuarioTokenDto(
    int Id,
    string NombreCompleto,
    string Username,
    string? Email
);

/// Proyecto al que el usuario tiene acceso (usado en el flujo maestro).
/// El frontend renderiza el menú de iconos con esta lista.
public record ProyectoAccesoDto(
    int Id,
    string Codigo,
    string Nombre,
    string Plataforma,
    string? IconoCss,
    string? UrlBase,
    string RolEnProyecto,   
    int NivelRol
);

// CLAIMS INTERNOS (para IJwtService)

/// Claims para generar un JWT de acceso directo a proyecto
public record TokenDirectoClaimsDto(
    int UsuarioId,
    string Username,
    int ProyectoId,
    string CodigoProyecto,
    int RolId,
    string NombreRol,
    int NivelRol,
    string Plataforma
);

public record TokenMaestroClaimsDto(
    int UsuarioId,
    string Username,
    string Rol,       
    int Nivel,        
    string Plataforma
);

/// Claims extraídos al validar cualquier JWT.
public record TokenClaimsDto(
    int UsuarioId,
    string Username,
    bool EsMaestro,
    int? ProyectoId,
    string? CodigoProyecto,
    string? NombreRol,   
    int? NivelRol,       
    string Plataforma
);