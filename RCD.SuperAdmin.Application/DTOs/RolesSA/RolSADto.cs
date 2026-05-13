namespace RCD.SuperAdmin.Application.DTOs.RolesSA;

public record RolSADto(
    int Id,
    string Nombre,
    int Nivel,
    string? Descripcion,
    bool Activo
);

public record CrearRolSADto(
    string Nombre,
    int Nivel,
    string? Descripcion
);

public record ActualizarRolSADto(
    string? Nombre,
    int? Nivel,
    string? Descripcion,
    bool? Activo
);
