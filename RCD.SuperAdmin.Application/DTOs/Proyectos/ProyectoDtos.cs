namespace RCD.SuperAdmin.Application.DTOs.Proyectos;

public record ProyectoListDto(
    int Id,
    string Codigo,
    string Nombre,
    string Plataforma,
    string? IconoCss,
    string Estado,
    string? Version,
    int Orden,
    bool Activo
);

public record ProyectoDetalleDto(
    int Id,
    string Codigo,
    string Nombre,
    string Plataforma,
    string? IconoCss,
    string? UrlBase,
    string Estado,
    string? Version,
    string? Descripcion,
    int Orden,
    bool Activo,
    IEnumerable<RolDto> Roles,
    IEnumerable<VistaDto> Vistas
);

public record RolDto(
    int Id,
    string Nombre,
    int Nivel,
    string? Descripcion,
    bool Activo
);

public record CrearRolDto(
    string Nombre,
    int Nivel,
    string? Descripcion
);

public record VistaDto(
    int Id,
    string Codigo,
    string Nombre,
    string Ruta,
    string? Icono,
    string? Descripcion,
    int Orden,
    bool Activo,
    // ── Campos de jerarquía ──────────────────────────────────────────
    int? VistaPadreId,
    bool EsContenedor
);

public record CrearVistaDto(
    string Codigo,
    string Nombre,
    string Ruta,
    string? Icono,
    string? Descripcion,
    int? VistaPadreId = null,   
    bool EsContenedor = false,  
    int Orden = 0
);

public record CrearProyectoDto(
    string Codigo,
    string Nombre,
    string Plataforma,
    string? IconoCss,
    string? UrlBase,
    string? Version,
    string? Descripcion,
    int Orden = 0
);

public record ActualizarProyectoDto(
    string Nombre,
    string Plataforma,
    string? IconoCss,
    string? UrlBase,
    string Estado,
    string? Version,
    string? Descripcion,
    int Orden
);

public record ActualizarRolDto(
    string Nombre,
    int Nivel,
    string? Descripcion,
    bool Activo
);

public record ActualizarVistaDto(
    string Codigo,
    string Nombre,
    string Ruta,
    string? Icono,
    string? Descripcion,
    int? VistaPadreId,
    bool EsContenedor,
    int Orden,
    bool Activo
);

public record UsuarioProyectoDto(
    int UsuarioId,
    string Username,
    string NombreCompleto,
    string? Email,
    string Rol,
    int NivelRol,
    bool Activo,              // Estado de la asignación
    string? CreadoPor,
    DateTime FechaAsignacion
);

public record ReordenarProyectosDto(
    IEnumerable<ProyectoOrdenDto> Items
);

public record ProyectoOrdenDto(
    int Id,
    int Orden
);