// UsuarioDto.cs
namespace RCD.SuperAdmin.Application.DTOs.Usuarios;

public record UsuarioDto(
    int Id,
    string NombreCompleto,
    string Username,
    string Email,
    bool Activo,
    DateTime? UltimoAcceso,
    IEnumerable<string> Roles
);

public record ActualizarUsuarioRequest(
    string NombreCompleto,
    string Email,
    bool Activo,
    IEnumerable<int> RolIds
);

public record CrearUsuarioRequest(
    string NombreCompleto,
    string Username,
    string Email,
    string Password,
    IEnumerable<int> RolIds
);