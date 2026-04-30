// LoginResponse.cs
namespace RCD.SuperAdmin.Application.DTOs.Auth;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpira,
    string NombreCompleto,
    string Username,
    IEnumerable<string> Roles,
    IEnumerable<ProyectoPermitidoDto> ProyectosPermitidos
);