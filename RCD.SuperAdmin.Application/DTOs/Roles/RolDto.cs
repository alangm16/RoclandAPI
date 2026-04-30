// RCD.SuperAdmin.Application/DTOs/Roles/RolDto.cs
namespace RCD.SuperAdmin.Application.DTOs.Roles;

public record RolDto(
    int Id,
    string Nombre,
    bool Activo
);

public record CrearRolRequest(
    string Nombre
);